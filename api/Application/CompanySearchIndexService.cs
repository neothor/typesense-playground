namespace Api.Application
{
    using Api.Domain;
    using Api.Infrastructure;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Typesense;

    public class CompanySearchIndexService : ICompanySearchIndexService
    {
        private readonly ITypesenseClient _client;
        private readonly ILogger<CompanySearchIndexService> _logger;
        private readonly TypesenseConfig _config;

        public CompanySearchIndexService(ITypesenseClient client, ILogger<CompanySearchIndexService> logger, IOptions<TypesenseConfig> config)
        {
            _logger = logger;
            _config = config.Value;
            _client = client;
        }

        public async Task IndexAsync(ICollection<Company> companies)
        {
            var schema = await ValidateCollection();

            var importResults = await _client.ImportDocuments<Company>(
                schema.Name, 
                companies,
                importType: ImportType.Upsert);

            if(importResults.Any(i => !i.Success)) {
                var errors = importResults
                    .Where(x => !x.Success)
                    .GroupBy(x => x.Error)
                    .Select(x => $"{x.Key!} (x{x.Count()}");

                _logger.LogCritical("Importing documents into {name} failed: {errors}", schema.Name, string.Join(", ", errors));
                throw new Exception($"Importing documents into {schema.Name} failed: {string.Join(", ", errors)}");
            }

            _logger.LogInformation("Imported {num} documents into {name}", importResults.Count, schema.Name);
        }

        private async Task<Schema> ValidateCollection()
        {
            var schema = GetSchema();
            var collection = await _client.RetrieveCollections()
                .ContinueWith(x => x.Result.SingleOrDefault(c => c.Name == schema.Name));

            if (collection == null)
            {
                var result = await _client.CreateCollection(schema);
                _logger.LogInformation("Created new collection {name}", schema.Name);
                return schema;
            }
                       
            // Needs to improve
            if(collection.Fields.Count != schema.Fields.Count(f => f.Name != SchemaHelper.GetFieldName<Company>(x => x.Id)))
            {
                throw new NotImplementedException();
            }

            return schema;
        }


        private static Schema GetSchema()
        {
            return SchemaHelper.Create<Company>()
                .AddField(x => x.Id, false)
                .AddField(x => x.Tenant, false)
                .AddField(x => x.CompanyName, false)
                .AddField(x => x.Address, false)
                .AddField(x => x.City, false)
                .AddField(x => x.Country, false)
                .AddField(x => x.PhoneNumber, false)
                .AddField(x => x.Email, false)
                .Build();
        }
    }
}
