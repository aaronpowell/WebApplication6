using Aspire.Hosting.Azure;

var builder = DistributedApplication.CreateBuilder(args);


var postgres = builder.AddPostgres("pg");

if (builder.ExecutionContext.IsPublishMode)
{
    postgres.AsAzurePostgresFlexibleServer((resource, construct, server) =>
    {
        construct.AddOutput(server.AddOutput("name", data => data.Name));
    });

    var template = builder.AddBicepTemplateString("vector-extension", """
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
    """);

    if (azurePostgres is not null)
    {
        template.WithParameter("postgresServerName", azurePostgres.GetOutput("name"));
    }
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
