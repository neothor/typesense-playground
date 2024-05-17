using Api.Application;
using Api.Domain;
using Typesense.Setup;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var tsSection = builder.Configuration
    .GetSection(TypesenseConfig.Section);

builder.Services
    .Configure<TypesenseConfig>(tsSection)
    .AddTransient<ICompanySearchIndexService, CompanySearchIndexService>()
    .AddTypesenseClient(c => {
        var config = tsSection.Get<TypesenseConfig>()!;
        c.ApiKey = config.ApiKey;
        c.Nodes = config.NodeUris
            .Select(x => new Node(x.Host, x.Port.ToString(), x.Scheme))
            .ToList();
    })
    .AddHostedService<CompanyGenerationService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
