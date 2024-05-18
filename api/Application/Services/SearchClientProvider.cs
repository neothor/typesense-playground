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

        private string _searchKey;
        private readonly Dictionary<string, string> _tenantSearchKeys = new Dictionary<string, string>();
        private readonly Dictionary<string, ITypesenseClient> _tenantSearchClients = new Dictionary<string, ITypesenseClient>();

        public SearchClientProvider(ITypesenseClient mainClient, IOptions<TypesenseConfig> options, IHttpClientFactory httpClientFactory)
        {
            _client = mainClient;
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
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
            _tenantSearchClients.Add(tenant, client);
            return client;
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

            _tenantSearchKeys.Add(tenant, tenantKey);
            return tenantKey;
        }

        private async Task<string> GetOrCreateSearchKeyAsync()
        {
            var result = await _client.ListKeys();
            var keyResponse = result.Keys.SingleOrDefault(x => x.Description == SearchKey);
            if (keyResponse != null)
            {
                return keyResponse.Value;
            }

            var keyRequest = new Key(SearchKey, [SearchAction], [SearchAllCollection]);
            keyResponse = await _client.CreateKey(keyRequest);
            return keyResponse.Value;
        }
    }
}
