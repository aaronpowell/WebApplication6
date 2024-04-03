resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' existing = {
    name: '${postgresServerName}'
}

resource postgresConfig 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2022-12-01' = {
  dependsOn: [
    postgresServer
  ]
  name: '${postgresServerName}/azure.extensions'
  properties: {
    value: 'PGVECTOR'
    source: 'user-override'
  }
}