using System.Collections.Concurrent;

namespace Api.Application.Services
{
    using Api.Domain;
    using Api.Infrastructure;
    using Microsoft.Extensions.Options;
    using System.Net;
    using Typesense;

    public class SearchClientProvider : ISearchClientProvider
    {
        private static readonly string SearchKey = $"{nameof(CompanySearchService)}_{nameof(SearchKey)}";
        private static readonly string SearchAction = "documents:search";
        private static readonly string SearchAllCollection = "*";

        private readonly ITypesenseClient _client;
        private readonly TypesenseConfig _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SearchClientProvider> _logger;

        private string _searchKey;
        private readonly ConcurrentDictionary<string, string> _tenantSearchKeys = new();
        private readonly ConcurrentDictionary<string, ITypesenseClient> _tenantSearchClients = new();

        public SearchClientProvider(ITypesenseClient mainClient, IHttpClientFactory httpClientFactory, IOptions<TypesenseConfig> options, ILogger<SearchClientProvider> logger)
        {
            _client = mainClient;
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public Task<ITypesenseClient> GetAdminClient()
        {
            return Task.FromResult(_client);
        }

        public async Task<ITypesenseClient> GetSearchClient(string tenant)
        {
            if(_tenantSearchClients.TryGetValue(tenant, out var client)) 
            { 
                return client; 
            }

            client = await CreateTenantClient(tenant);
            return _tenantSearchClients.TryAdd(tenant, client) ? client : _tenantSearchClients[tenant];
        }

        private async Task<ITypesenseClient> CreateTenantClient(string tenant)
        {
            var config = new Typesense.Setup.Config(
                TypesenseHelper.CreateNodes(_options.NodeUris),
                await GetOrCreateTenantSearchKeyAsync(tenant)
            );

            var httpClient = _httpClientFactory.CreateClient($"{nameof(SearchClientProvider)}_{tenant}");
            httpClient.DefaultRequestVersion = HttpVersion.Version30;

            var client = new TypesenseClient(Options.Create(config), httpClient);
            return client;
        }

        private async Task<string> GetOrCreateTenantSearchKeyAsync(string tenant)
        {
            if (string.IsNullOrEmpty(_searchKey))
            {
                _searchKey = await GetOrCreateSearchKeyAsync();
            }

            if (_tenantSearchKeys.TryGetValue(tenant, out var tenantKey))
            {
                return tenantKey;
            }

            // Very ugly
            tenantKey = _client.GenerateScopedSearchKey(_searchKey, 
                "{\"filter_by\":\"" + $"{TypesenseHelper.GetFieldName<Company>(x => x.Tenant)}:{tenant}" + "\"}");

            return _tenantSearchKeys.TryAdd(tenant, tenantKey) ? tenantKey : _tenantSearchKeys[tenant];
        }

        private async Task<string> GetOrCreateSearchKeyAsync()
        {
            var result = await _client.ListKeys();
            var existingKeys = result.Keys.Where(x => x.Description == SearchKey).ToList();
            if (existingKeys.Any())
            {
                foreach (var existingKey in existingKeys)
                {
                    await _client.DeleteKey(existingKey.Id);
                }
            }

            var keyRequest = new Key(SearchKey, new[] { SearchAction }, new[] { SearchAllCollection });
            var keyResponse = await _client.CreateKey(keyRequest);
            return keyResponse.Value;
        }
    }
}
