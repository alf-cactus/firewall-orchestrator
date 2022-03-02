﻿using FWO.ApiClient;
using FWO.ApiClient.Queries;
using FWO.Config.Api.Data;
using FWO.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FWO.Config.Api
{
    public abstract class Config : ConfigData
    {
        /// <summary>
        /// Internal connection to api server. Used to get/edit config data.
        /// </summary>
        protected readonly APIConnection apiConnection;

        public readonly int UserId;

        public event Action<Config, ConfigItem[]>? OnChange;

        protected SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        protected Config(APIConnection apiConnection, int userId)
        {
            UserId = userId;
            this.apiConnection = apiConnection;
            apiConnection.GetSubscription<ConfigItem[]>(SubscriptionExceptionHandler, SubscriptionUpdateHandler, ConfigQueries.getConfigSubscription, new { UserId });
        }

        protected void SubscriptionUpdateHandler(ConfigItem[] configItems)
        {
            semaphoreSlim.Wait();
            try 
            { 
                Update(configItems);
                OnChange?.Invoke(this, configItems);
            }
            finally { semaphoreSlim.Release(); }
        }

        public void Update(ConfigItem[] configItems)
        {
            foreach (PropertyInfo property in GetType().GetProperties())
            {
                // Is Property storing config value?
                if (property.GetCustomAttribute<JsonPropertyNameAttribute>() != null)
                {
                    string key = property.GetCustomAttribute<JsonPropertyNameAttribute>()!.Name;
                    ConfigItem? configItem = configItems.First(configItem => configItem.Key == key);

                    if (configItem != null)
                    {
                        try
                        {
                            TypeConverter converter = TypeDescriptor.GetConverter(property.GetType());
                            property.SetValue(this, converter.ConvertFromString(configItem.Value
                            ?? throw new Exception($"Config value (with key: {configItem.Key}) is null."))
                            ?? throw new Exception($"Config value (with key: {configItem.Key}) is not convertible to {property.GetType()}."));
                        }
                        catch (Exception exception)
                        {
                            Log.WriteError("Load Config Items", $"Config item with key \"{key}\" could not be loaded. Using default value.", exception);
                        }
                    }
                    else
                    {
                        Log.WriteError("Load Config Items", $"Config item with key \"{key}\" could not be found. Using default value.");
                    }
                }
            }
        }

        protected async Task WriteToDatabase(ConfigData editedData)
        {
            await semaphoreSlim.WaitAsync();
            try 
            {
                foreach (PropertyInfo property in GetType().GetProperties())
                {
                    // Is Property storing config value?
                    if (property.GetCustomAttribute<JsonPropertyNameAttribute>() != null)
                    {
                        // Was config value changed?
                        if (property.GetValue(this) != property.GetValue(editedData))
                        {
                            string key = property.GetCustomAttribute<JsonPropertyNameAttribute>()!.Name;

                            try
                            {
                                TypeConverter converter = TypeDescriptor.GetConverter(property.GetType());
                                string stringValue = converter.ConvertToString(property.GetValue(editedData)
                                                ?? throw new Exception($"Config value (with key: {key}) is null"))
                                                ?? throw new Exception($"Config value (with key: {key}) is not convertible to {property.GetType()}.");
                                // Update or insert config item
                                await apiConnection.SendQueryAsync<object>(ConfigQueries.upsertConfigItem, new ConfigItem { Key = key, Value = stringValue, User = UserId });
                            }
                            catch (Exception exception)
                            {
                                Log.WriteError("Load Config Items", $"Config item with key \"{key}\" and value: {property.GetValue(editedData)} could not be stored.", exception);
                            }
                        }
                    }
                }
            }
            finally { semaphoreSlim.Release(); }
        }

        protected async Task<ConfigData> GetEditableConfig()
        {
            await semaphoreSlim.WaitAsync();
            try { return (ConfigData)Clone(); }
            finally { semaphoreSlim.Release(); }
        }

        protected void SubscriptionExceptionHandler(Exception exception)
        {
            Log.WriteError("Config Subscription", "Config subscription lead to error.", exception);
        }

        // TODO: Move method
        public static string ShowBool(bool boolVal)
        {
            return boolVal ? "\u2714" : "\u2716";
        }

        protected void InvokeOnChange(Config config, ConfigItem[] configItems)
        {
            OnChange?.Invoke(config, configItems);
        }
    }
}
