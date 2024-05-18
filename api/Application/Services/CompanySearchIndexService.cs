namespace Api.Application.Services
{
    using Api.Domain;
    using Api.Infrastructure;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Typesense;

    public class CompanySearchIndexService : ICompanySearchIndexService
    {
        private Schema _schema;
        private readonly ITypesenseClient _client;
        private readonly ILogger<CompanySearchIndexService> _logger;

        public CompanySearchIndexService(ITypesenseClient client, ILogger<CompanySearchIndexService> logger)
        {
            _logger = logger;
            _client = client;
        }

        public async Task IndexAsync(ICollection<Company> companies)
        {
            var schema = await ValidateCollection();

            var importResults = await _client.ImportDocuments(
                schema.Name,
                companies,
                importType: ImportType.Upsert);

            if (importResults.Any(i => !i.Success))
            {
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
            if(_schema != null)
            {
                return _schema;
            }

            _schema = GetSchema();
            var collection = await _client.RetrieveCollections()
                .ContinueWith(x => x.Result.SingleOrDefault(c => c.Name == _schema.Name));

            if (collection == null)
            {
                var result = await _client.CreateCollection(_schema);
                _logger.LogInformation("Created new collection {name}", _schema.Name);
                return _schema;
            }

            // Needs to improve
            if (collection.Fields.Count != _schema.Fields.Count(f => f.Name != TypesenseHelper.GetFieldName<Company>(x => x.Id)))
            {
                throw new NotImplementedException();
            }

            return _schema;
        }

        private static Schema GetSchema()
        {
            return TypesenseHelper.Create<Company>()
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
