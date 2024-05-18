using Api.Application;
using Api.Domain;
using Typesense.Setup;
using System.Linq;
using CompaniesRepository = Api.Domain.CompaniesRepository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var tsSection = builder.Configuration
    .GetSection(TypesenseConfig.Section);

builder.Services
    .Configure<TypesenseConfig>(tsSection)
    .AddSingleton<ICompaniesRepository, CompaniesRepository>()
    .AddTransient<ICompaniesService, CompaniesService>()
    .AddTransient<ICompanySearchService, CompanySearchService>()
    .AddTransient<ICompanySearchIndexService, CompanySearchIndexService>()
    .AddTypesenseClient(c => {
        var config = tsSection.Get<TypesenseConfig>()!;
        c.ApiKey = config.ApiKey;
        c.Nodes = config.NodeUris
            .Select(x => new Node(x.Host, x.Port.ToString(), x.Scheme))
            .ToList();
    })
    .AddHostedService<CompanyGenerationService>();

builder.Services.AddControllers(x => { });
builder.Services.AddRouting(options => options.LowercaseUrls = true);
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
