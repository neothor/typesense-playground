using Api.Domain;
using Typesense.Setup;
using CompaniesRepository = Api.Domain.CompaniesRepository;
using Api.Application.Services;
using Api.Application.Jobs;
using Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var tsSection = builder.Configuration
    .GetSection(TypesenseConfig.Section);

builder.Services
    .Configure<TypesenseConfig>(tsSection)
    .AddSingleton<ICompaniesRepository, CompaniesRepository>()
    .AddSingleton<ISearchClientProvider, SearchClientProvider>()
    .AddTransient<ICompaniesService, CompaniesService>()
    .AddTransient<ICompanySearchService, CompanySearchService>()
    .AddTransient<ICompanySearchIndexService, CompanySearchIndexService>()
    .AddTypesenseClient(c => {
        var config = tsSection.Get<TypesenseConfig>()!;
        c.ApiKey = config.ApiKey;
        c.Nodes = TypesenseHelper.CreateNodes(config.NodeUris);
    })
    .AddHostedService<SearchIndexSeedingJob>();

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
app.UseCors(b => b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.MapControllers();

app.Run();
