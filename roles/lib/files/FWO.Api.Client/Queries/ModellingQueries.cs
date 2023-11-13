﻿using FWO.Logging;

namespace FWO.Api.Client.Queries
{
    public class ModellingQueries : Queries
    {
        public static readonly string appServerDetailsFragment;
        public static readonly string appRoleDetailsFragment;
        public static readonly string serviceDetailsFragment;
        public static readonly string serviceGroupDetailsFragment;
        public static readonly string connectionDetailsFragment;

        public static readonly string getAreas;
        public static readonly string newArea;
        public static readonly string deleteArea;
        public static readonly string newAreaSubnet;
        public static readonly string deleteAreaSubnet;

        public static readonly string getAppServers;
        public static readonly string getImportedAppServers;
        public static readonly string newAppServer;
        public static readonly string updateAppServer;
        public static readonly string markAppServerDeleted;
        public static readonly string deleteAppServer;
        public static readonly string getAppRolesForAppServer;
        public static readonly string getConnectionsForAppServer;

        public static readonly string getInterfaces;
        public static readonly string getInterfaceById;
        public static readonly string getConnections;
        public static readonly string newConnection;
        public static readonly string updateConnection;
        public static readonly string deleteConnection;
        public static readonly string addAppServerToConnection;
        public static readonly string removeAppServerFromConnection;
        public static readonly string addAppRoleToConnection;
        public static readonly string removeAppRoleFromConnection;
        public static readonly string addServiceToConnection;
        public static readonly string removeServiceFromConnection;
        public static readonly string addServiceGroupToConnection;
        public static readonly string removeServiceGroupFromConnection;

        public static readonly string getAppRoles;
        public static readonly string newAppRole;
        public static readonly string updateAppRole;
        public static readonly string deleteAppRole;
        public static readonly string getAppServerForAppRole;
        public static readonly string addAppServerToAppRole;
        public static readonly string removeAppServerFromAppRole;

        public static readonly string getServicesForApp;
        public static readonly string getGlobalServices;
        public static readonly string newService;
        public static readonly string updateService;
        public static readonly string deleteService;

        public static readonly string getServiceGroupsForApp;
        public static readonly string getGlobalServiceGroups;
        public static readonly string newServiceGroup;
        public static readonly string updateServiceGroup;
        public static readonly string deleteServiceGroup;
        public static readonly string addServiceToServiceGroup;
        public static readonly string removeServiceFromServiceGroup;

        public static readonly string getHistory;
        public static readonly string addHistoryEntry;


        static ModellingQueries()
        {
            try
            {
                appServerDetailsFragment = File.ReadAllText(QueryPath + "modelling/fragments/appServerDetails.graphql");
                appRoleDetailsFragment = File.ReadAllText(QueryPath + "modelling/fragments/appRoleDetails.graphql");
                serviceDetailsFragment = File.ReadAllText(QueryPath + "modelling/fragments/serviceDetails.graphql");
                serviceGroupDetailsFragment = File.ReadAllText(QueryPath + "modelling/fragments/serviceGroupDetails.graphql");
                connectionDetailsFragment = appServerDetailsFragment + appRoleDetailsFragment + serviceDetailsFragment + serviceGroupDetailsFragment +
                    File.ReadAllText(QueryPath + "modelling/fragments/connectionDetails.graphql");

                getAreas = File.ReadAllText(QueryPath + "modelling/getAreas.graphql");
                newArea = File.ReadAllText(QueryPath + "modelling/newArea.graphql");
                deleteArea = File.ReadAllText(QueryPath + "modelling/deleteArea.graphql");
                newAreaSubnet = File.ReadAllText(QueryPath + "modelling/newAreaSubnet.graphql");
                deleteAreaSubnet = File.ReadAllText(QueryPath + "modelling/deleteAreaSubnet.graphql");

                getAppServers = appServerDetailsFragment + File.ReadAllText(QueryPath + "modelling/getAppServers.graphql");
                getImportedAppServers = appServerDetailsFragment + File.ReadAllText(QueryPath + "modelling/getImportedAppServers.graphql");
                newAppServer = File.ReadAllText(QueryPath + "modelling/newAppServer.graphql");
                updateAppServer = File.ReadAllText(QueryPath + "modelling/updateAppServer.graphql");
                markAppServerDeleted = File.ReadAllText(QueryPath + "modelling/markAppServerDeleted.graphql");
                deleteAppServer = File.ReadAllText(QueryPath + "modelling/deleteAppServer.graphql");
                getAppRolesForAppServer = File.ReadAllText(QueryPath + "modelling/getAppRolesForAppServer.graphql");
                getConnectionsForAppServer = File.ReadAllText(QueryPath + "modelling/getConnectionsForAppServer.graphql");                

                getInterfaces = connectionDetailsFragment + File.ReadAllText(QueryPath + "modelling/getInterfaces.graphql");
                getInterfaceById = connectionDetailsFragment + File.ReadAllText(QueryPath + "modelling/getInterfaceById.graphql");                
                getConnections = connectionDetailsFragment + File.ReadAllText(QueryPath + "modelling/getConnections.graphql");
                newConnection = File.ReadAllText(QueryPath + "modelling/newConnection.graphql");
                updateConnection = File.ReadAllText(QueryPath + "modelling/updateConnection.graphql");
                deleteConnection = File.ReadAllText(QueryPath + "modelling/deleteConnection.graphql");
                addAppServerToConnection = File.ReadAllText(QueryPath + "modelling/addAppServerToConnection.graphql");
                removeAppServerFromConnection = File.ReadAllText(QueryPath + "modelling/removeAppServerFromConnection.graphql");
                addAppRoleToConnection = File.ReadAllText(QueryPath + "modelling/addAppRoleToConnection.graphql");
                removeAppRoleFromConnection = File.ReadAllText(QueryPath + "modelling/removeAppRoleFromConnection.graphql");
                addServiceToConnection = File.ReadAllText(QueryPath + "modelling/addServiceToConnection.graphql");
                removeServiceFromConnection = File.ReadAllText(QueryPath + "modelling/removeServiceFromConnection.graphql");
                addServiceGroupToConnection = File.ReadAllText(QueryPath + "modelling/addServiceGroupToConnection.graphql");
                removeServiceGroupFromConnection = File.ReadAllText(QueryPath + "modelling/removeServiceGroupFromConnection.graphql");

                getAppRoles = appServerDetailsFragment + appRoleDetailsFragment + File.ReadAllText(QueryPath + "modelling/getAppRoles.graphql");
                newAppRole = File.ReadAllText(QueryPath + "modelling/newAppRole.graphql");
                updateAppRole = File.ReadAllText(QueryPath + "modelling/updateAppRole.graphql");
                deleteAppRole = File.ReadAllText(QueryPath + "modelling/deleteAppRole.graphql");
                // getAppServerForAppRole = appServerDetailsFragment + File.ReadAllText(QueryPath + "modelling/getAppServerForAppRole.graphql");
                addAppServerToAppRole = File.ReadAllText(QueryPath + "modelling/addAppServerToAppRole.graphql");
                removeAppServerFromAppRole = File.ReadAllText(QueryPath + "modelling/removeAppServerFromAppRole.graphql");

                getServicesForApp = serviceDetailsFragment + File.ReadAllText(QueryPath + "modelling/getServicesForApp.graphql");
                getGlobalServices = serviceDetailsFragment + File.ReadAllText(QueryPath + "modelling/getGlobalServices.graphql");
                newService = File.ReadAllText(QueryPath + "modelling/newService.graphql");
                updateService = File.ReadAllText(QueryPath + "modelling/updateService.graphql");
                deleteService = File.ReadAllText(QueryPath + "modelling/deleteService.graphql");

                getServiceGroupsForApp = serviceDetailsFragment + serviceGroupDetailsFragment + File.ReadAllText(QueryPath + "modelling/getServiceGroupsForApp.graphql");
                getGlobalServiceGroups = serviceDetailsFragment + serviceGroupDetailsFragment + File.ReadAllText(QueryPath + "modelling/getGlobalServiceGroups.graphql");
                newServiceGroup = File.ReadAllText(QueryPath + "modelling/newServiceGroup.graphql");
                updateServiceGroup = File.ReadAllText(QueryPath + "modelling/updateServiceGroup.graphql");
                deleteServiceGroup = File.ReadAllText(QueryPath + "modelling/deleteServiceGroup.graphql");
                addServiceToServiceGroup = File.ReadAllText(QueryPath + "modelling/addServiceToServiceGroup.graphql");
                removeServiceFromServiceGroup = File.ReadAllText(QueryPath + "modelling/removeServiceFromServiceGroup.graphql");

                getHistory = File.ReadAllText(QueryPath + "modelling/getHistory.graphql");
                addHistoryEntry = File.ReadAllText(QueryPath + "modelling/addHistoryEntry.graphql");
            }
            catch (Exception exception)
            {
                Log.WriteError("Initialize ModellingQueries", "Api ModellingQueries could not be loaded.", exception);
                Environment.Exit(-1);
            }
        }
    }
}
