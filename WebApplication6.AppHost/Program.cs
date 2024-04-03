var builder = DistributedApplication.CreateBuilder(args);


var postgres = builder.AddPostgres("pg");

if (builder.ExecutionContext.IsPublishMode)
{
    var template = builder.AddBicepTemplateString("vector-extension", """
    param postgresServerName string

    @description('')
    param location string = resourceGroup().location

    resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' existing = {
        name: postgresServerName
    }

    resource postgresConfig 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2022-12-01' = {
      parent: postgresServer
      name: 'azure.extensions'
      properties: {
        value: 'VECTOR'
        source: 'user-override'
      }
    }
    """);

    postgres.AsAzurePostgresFlexibleServer((resource, construct, server) =>
    {
        construct.AddOutput(server.AddOutput("name", data => data.Name));
        template.WithParameter("postgresServerName", resource.GetOutput("name"));
    });
}
else
{
    postgres
    .WithImage("ankane/pgvector")
    .WithImageTag("latest");
}

var db = postgres.AddDatabase("db");

builder.AddProject<Projects.WebApplication6>("webapplication6")
    .WithReference(db);

builder.Build().Run();
