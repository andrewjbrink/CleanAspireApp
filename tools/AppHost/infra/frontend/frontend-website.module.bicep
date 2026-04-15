@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param plan_outputs_azure_container_registry_endpoint string

param plan_outputs_planid string

param plan_outputs_azure_container_registry_managed_identity_id string

param plan_outputs_azure_container_registry_managed_identity_client_id string

param frontend_containerimage string

param frontend_containerport string

param plan_outputs_azure_app_service_dashboard_uri string

param plan_outputs_azure_website_contributor_managed_identity_id string

param plan_outputs_azure_website_contributor_managed_identity_principal_id string

resource mainContainer 'Microsoft.Web/sites/sitecontainers@2025-03-01' = {
  name: 'main'
  properties: {
    authType: 'UserAssigned'
    image: frontend_containerimage
    isMain: true
    targetPort: frontend_containerport
    userManagedIdentityClientId: plan_outputs_azure_container_registry_managed_identity_client_id
  }
  parent: webapp
}

resource webapp 'Microsoft.Web/sites@2025-03-01' = {
  name: take('${toLower('frontend')}-${uniqueString(resourceGroup().id)}', 60)
  location: location
  properties: {
    serverFarmId: plan_outputs_planid
    siteConfig: {
      numberOfWorkers: 1
      linuxFxVersion: 'SITECONTAINERS'
      acrUseManagedIdentityCreds: true
      acrUserManagedIdentityID: plan_outputs_azure_container_registry_managed_identity_client_id
      appSettings: [
        {
          name: 'WEBSITES_PORT'
          value: frontend_containerport
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
          value: frontend_containerport
        }
        {
          name: 'API_HTTP'
          value: 'http://${take('${toLower('api')}-${uniqueString(resourceGroup().id)}', 60)}.azurewebsites.net'
        }
        {
          name: 'services__api__http__0'
          value: 'http://${take('${toLower('api')}-${uniqueString(resourceGroup().id)}', 60)}.azurewebsites.net'
        }
        {
          name: 'API_HTTPS'
          value: 'https://${take('${toLower('api')}-${uniqueString(resourceGroup().id)}', 60)}.azurewebsites.net'
        }
        {
          name: 'services__api__https__0'
          value: 'https://${take('${toLower('api')}-${uniqueString(resourceGroup().id)}', 60)}.azurewebsites.net'
        }
        {
          name: 'ASPIRE_ENVIRONMENT_NAME'
          value: 'plan'
        }
        {
          name: 'OTEL_SERVICE_NAME'
          value: 'frontend'
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
    }
  }
}

resource frontend_website_ra 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
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
      'API_HTTP'
      'services__api__http__0'
      'API_HTTPS'
      'services__api__https__0'
      'OTEL_SERVICE_NAME'
    ]
  }
  parent: webapp
}