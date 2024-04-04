using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDataSource("db", null, builder => builder.UseVector());
var app = builder.Build();

var datasource = app.Services.GetRequiredService<NpgsqlDataSource>();
using var conn = datasource.OpenConnection();
using var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS vector;", conn);
await cmd.ExecuteNonQueryAsync();

app.MapDefaultEndpoints();

app.MapGet("/", (NpgsqlDataSource datasource) =>
{
    using var conn = datasource.OpenConnection();
    using var cmd = new NpgsqlCommand("SELECT * FROM pg_extension;", conn);
    using var reader = cmd.ExecuteReader();

    StringBuilder stringBuilder = new();
    while (reader.Read())
    {
        stringBuilder.AppendLine(reader.GetString(1));
    }

    return stringBuilder.ToString();
});

app.Run();
