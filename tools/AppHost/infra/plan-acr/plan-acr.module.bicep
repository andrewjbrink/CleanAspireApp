@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource plan_acr 'Microsoft.ContainerRegistry/registries@2025-04-01' = {
  name: take('planacr${uniqueString(resourceGroup().id)}', 50)
  location: location
  sku: {
    name: 'Basic'
  }
  tags: {
    'aspire-resource-name': 'plan-acr'
  }
}

output name string = plan_acr.name

output loginServer string = plan_acr.properties.loginServer