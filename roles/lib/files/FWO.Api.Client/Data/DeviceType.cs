using System.ComponentModel;
using System.Globalization;
using System.Text.Json.Serialization;
using FWO.Api.Client;
using Newtonsoft.Json;
namespace FWO.Api.Data
{
    public class DeviceType
    {
        [JsonProperty("id"), JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonProperty("name"), JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonProperty("version"), JsonPropertyName("version")]
        public string Version { get; set; } = "";

        [JsonProperty("manufacturer"), JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; } = "";

        [JsonProperty("isPureRoutingDevice"), JsonPropertyName("isPureRoutingDevice")]
        public bool IsPureRoutingDevice { get; set; }

        [JsonProperty("isManagement"), JsonPropertyName("isManagement")]
        public bool IsManagement { get; set; }

        private static List<int> LegacyDevTypeList = new List<int> 
        {
            2, // Netscreen 5.x-6.x
            4, // FortiGateStandalone 5ff
            5, // Barracuda Firewall Control Center Vx
            6, // phion netfence 3.x
            7, // Check Point R5x-R7x
            8  // JUNOS 10-21
        };

        private static Dictionary<int, int> SupermanagerMap = new Dictionary<int, int>
        {  
            // Mgmt -> Supermgmt
            { 11, 12 }, // FortiADOM 5ff -> FortiManager 5ff
            { 9, 13 }  // Check Point R8x -> Check Point MDS R8x
        };
        private static Dictionary<int, int> SupermanagerGatewayMap = new Dictionary<int, int>
        {  
            // Supermgmt -> Gateway
            { 12, 10},  // FortiManager 5ff-> FortiGate 5ff
            { 13, 9 },   // Check Point MDS R8x-> Check Point R8x (?)
            { 9, 9 },   // Check Point R8x Mgr-> Check Point R8x Mgr
            { 14, 16}   // Cisco Firepower
        };

        private static List<int> CheckPointManagers = new List<int>
        {  
            13, 9   // Check Point MDS R8x and Check Point R8x
        };

        private static List<int> FortiManagers = new List<int>
        {  
            12   // FortiManager 5ff
        };


        public DeviceType()
        {}
        
        public DeviceType(DeviceType deviceType)
        {
            Id = deviceType.Id;
            Name = deviceType.Name;
            Version = deviceType.Version;
            Manufacturer = deviceType.Manufacturer;
            IsPureRoutingDevice = deviceType.IsPureRoutingDevice;
            IsManagement = deviceType.IsManagement;
        }

        public string NameVersion()
        {
            return Name + " " + Version;
        }

        public bool IsLegacyDevType()
        {
            return LegacyDevTypeList.Contains(Id);
        }

        public bool CanHaveDomain()
        {
            return !FortiManagers.Contains(Id);
        }

        public bool IsDummyRouter()
        {
            return Manufacturer == "DummyRouter";
        }

        public bool CanHaveSupermanager()
        {
            return SupermanagerMap.Keys.Contains(Id);
        }

        public bool CanBeSupermanager()
        {
            return SupermanagerMap.Values.Contains(Id);
        }

        public bool CanBeAutodiscovered(Management mgmt)
        {
            return !IsUri(mgmt.Hostname) && (SupermanagerMap.Values.Contains(Id) || (CheckPointManagers.Contains(Id) && mgmt.SuperManagerId==null));
        }
        private static bool IsUri(string hostname)
        {
            return hostname.StartsWith("https://") || hostname.StartsWith("http://") || hostname.StartsWith("file://");
        }


        public int GetSupermanagerId()
        {
            return SupermanagerMap[Id];
        }

        public int GetManagementTypeId()
        {
            return SupermanagerMap.FirstOrDefault(x => x.Value == Id).Key;
        }

        public int GetGatewayTypeId()
        {
            return SupermanagerGatewayMap[Id];
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
