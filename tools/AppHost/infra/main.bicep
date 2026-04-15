targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention, the name of the resource group for your application will use this name, prefixed with rg-')
param environmentName string

@minLength(1)
@description('The location used for all deployed resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string = ''


var tags = {
  'azd-env-name': environmentName
}

resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module api_identity 'api-identity/api-identity.module.bicep' = {
  name: 'api-identity'
  scope: rg
  params: {
    location: location
  }
}
module api_roles_sql 'api-roles-sql/api-roles-sql.module.bicep' = {
  name: 'api-roles-sql'
  scope: rg
  params: {
    location: location
    principalId: api_identity.outputs.principalId
    principalName: api_identity.outputs.principalName
    sql_outputs_name: sql.outputs.name
    sql_outputs_sqlserveradminname: sql.outputs.sqlServerAdminName
  }
}
module insights 'insights/insights.module.bicep' = {
  name: 'insights'
  scope: rg
  params: {
    location: location
    log_analytics_outputs_loganalyticsworkspaceid: log_analytics.outputs.logAnalyticsWorkspaceId
  }
}
module log_analytics 'log-analytics/log-analytics.module.bicep' = {
  name: 'log-analytics'
  scope: rg
  params: {
    location: location
  }
}
module migrations_identity 'migrations-identity/migrations-identity.module.bicep' = {
  name: 'migrations-identity'
  scope: rg
  params: {
    location: location
  }
}
module migrations_roles_sql 'migrations-roles-sql/migrations-roles-sql.module.bicep' = {
  name: 'migrations-roles-sql'
  scope: rg
  params: {
    location: location
    principalId: migrations_identity.outputs.principalId
    principalName: migrations_identity.outputs.principalName
    sql_outputs_name: sql.outputs.name
    sql_outputs_sqlserveradminname: sql.outputs.sqlServerAdminName
  }
}
module plan 'plan/plan.module.bicep' = {
  name: 'plan'
  scope: rg
  params: {
    location: location
    plan_acr_outputs_name: plan_acr.outputs.name
    userPrincipalId: principalId
  }
}
module plan_acr 'plan-acr/plan-acr.module.bicep' = {
  name: 'plan-acr'
  scope: rg
  params: {
    location: location
  }
}
module sql 'sql/sql.module.bicep' = {
  name: 'sql'
  scope: rg
  params: {
    location: location
  }
}
output API_IDENTITY_CLIENTID string = api_identity.outputs.clientId
output API_IDENTITY_ID string = api_identity.outputs.id
output AZURE_APP_SERVICE_DASHBOARD_URI string = plan.outputs.AZURE_APP_SERVICE_DASHBOARD_URI
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = plan.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output INSIGHTS_APPINSIGHTSCONNECTIONSTRING string = insights.outputs.appInsightsConnectionString
output MIGRATIONS_IDENTITY_CLIENTID string = migrations_identity.outputs.clientId
output MIGRATIONS_IDENTITY_ID string = migrations_identity.outputs.id
output PLAN_AZURE_APP_SERVICE_DASHBOARD_URI string = plan.outputs.AZURE_APP_SERVICE_DASHBOARD_URI
output PLAN_AZURE_CONTAINER_REGISTRY_ENDPOINT string = plan.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output PLAN_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_CLIENT_ID string = plan.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_CLIENT_ID
output PLAN_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = plan.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID
output PLAN_AZURE_WEBSITE_CONTRIBUTOR_MANAGED_IDENTITY_ID string = plan.outputs.AZURE_WEBSITE_CONTRIBUTOR_MANAGED_IDENTITY_ID
output PLAN_AZURE_WEBSITE_CONTRIBUTOR_MANAGED_IDENTITY_PRINCIPAL_ID string = plan.outputs.AZURE_WEBSITE_CONTRIBUTOR_MANAGED_IDENTITY_PRINCIPAL_ID
output PLAN_PLANID string = plan.outputs.planId
output SQL_SQLSERVERFQDN string = sql.outputs.sqlServerFqdn
