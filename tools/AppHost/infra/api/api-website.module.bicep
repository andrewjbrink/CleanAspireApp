@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param plan_outputs_azure_container_registry_endpoint string

param plan_outputs_planid string

param plan_outputs_azure_container_registry_managed_identity_id string

param plan_outputs_azure_container_registry_managed_identity_client_id string

param api_containerimage string

param api_containerport string

param sql_outputs_sqlserverfqdn string

param insights_outputs_appinsightsconnectionstring string

param api_identity_outputs_id string

param api_identity_outputs_clientid string

param plan_outputs_azure_app_service_dashboard_uri string

param plan_outputs_azure_website_contributor_managed_identity_id string

param plan_outputs_azure_website_contributor_managed_identity_principal_id string

resource mainContainer 'Microsoft.Web/sites/sitecontainers@2025-03-01' = {
  name: 'main'
  properties: {
    authType: 'UserAssigned'
    image: api_containerimage
    isMain: true
    targetPort: api_containerport
    userManagedIdentityClientId: plan_outputs_azure_container_registry_managed_identity_client_id
  }
  parent: webapp
}

resource webapp 'Microsoft.Web/sites@2025-03-01' = {
  name: take('${toLower('api')}-${uniqueString(resourceGroup().id)}', 60)
  location: location
  properties: {
    serverFarmId: plan_outputs_planid
    keyVaultReferenceIdentity: api_identity_outputs_id
    siteConfig: {
      numberOfWorkers: 1
      linuxFxVersion: 'SITECONTAINERS'
      acrUseManagedIdentityCreds: true
      acrUserManagedIdentityID: plan_outputs_azure_container_registry_managed_identity_client_id
      appSettings: [
        {
          name: 'WEBSITES_PORT'
          value: api_containerport
        }
        {
          name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY'
          value: 'in_memory'
        }
        {
          name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
          value: 'true'
        }
        {
          name: 'HTTP_PORTS'
          value: api_containerport
        }
        {
          name: 'ConnectionStrings__CleanArchitecture'
          value: 'Server=tcp:${sql_outputs_sqlserverfqdn},1433;Encrypt=True;Authentication="Active Directory Default";Database=clean-architecture'
        }
        {
          name: 'CLEANARCHITECTURE_HOST'
          value: sql_outputs_sqlserverfqdn
        }
        {
          name: 'CLEANARCHITECTURE_PORT'
          value: '1433'
        }
        {
          name: 'CLEANARCHITECTURE_URI'
          value: 'mssql://${sql_outputs_sqlserverfqdn}:1433/clean-architecture'
        }
        {
          name: 'CLEANARCHITECTURE_JDBCCONNECTIONSTRING'
          value: 'jdbc:sqlserver://${sql_outputs_sqlserverfqdn}:1433;database=clean-architecture;encrypt=true;trustServerCertificate=false'
        }
        {
          name: 'CLEANARCHITECTURE_DATABASENAME'
          value: 'clean-architecture'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: insights_outputs_appinsightsconnectionstring
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: api_identity_outputs_clientid
        }
        {
          name: 'AZURE_TOKEN_CREDENTIALS'
          value: 'ManagedIdentityCredential'
        }
        {
          name: 'ASPIRE_ENVIRONMENT_NAME'
          value: 'plan'
        }
        {
          name: 'OTEL_SERVICE_NAME'
          value: 'api'
        }
        {
          name: 'OTEL_EXPORTER_OTLP_PROTOCOL'
          value: 'grpc'
        }
        {
          name: 'OTEL_EXPORTER_OTLP_ENDPOINT'
          value: 'http://localhost:6001'
        }
        {
          name: 'WEBSITE_ENABLE_ASPIRE_OTEL_SIDECAR'
          value: 'true'
        }
        {
          name: 'OTEL_COLLECTOR_URL'
          value: plan_outputs_azure_app_service_dashboard_uri
        }
        {
          name: 'OTEL_CLIENT_ID'
          value: plan_outputs_azure_container_registry_managed_identity_client_id
        }
      ]
      alwaysOn: true
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${plan_outputs_azure_container_registry_managed_identity_id}': { }
      '${api_identity_outputs_id}': { }
    }
  }
}

resource api_website_ra 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(webapp.id, plan_outputs_azure_website_contributor_managed_identity_id, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'de139f84-1756-47ae-9be6-808fbbe84772'))
  properties: {
    principalId: plan_outputs_azure_website_contributor_managed_identity_principal_id
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'de139f84-1756-47ae-9be6-808fbbe84772')
    principalType: 'ServicePrincipal'
  }
  scope: webapp
}

resource slotConfigNames 'Microsoft.Web/sites/config@2025-03-01' = {
  name: 'slotConfigNames'
  properties: {
    appSettingNames: [
      'OTEL_SERVICE_NAME'
    ]
  }
  parent: webapp
}