namespace Api.Application.Services
{
    using Api.Domain;
    using Api.Infrastructure;
    using Typesense;

    public class CompanySearchService : ICompanySearchService
    {
        private readonly ISearchClientProvider _searchClientProvider;

        public CompanySearchService(ISearchClientProvider searchClientProvider)
        {
            _searchClientProvider = searchClientProvider;
        }

        public async Task<IEnumerable<IIndexable>> SearchAsync(string tenant, string query, int limit, int skip)
        {
            var client = await _searchClientProvider.GetSearchClient(tenant);
            var searchParameters = new SearchParameters(query, TypesenseHelper.GetFieldName<Company>(x => x.CompanyName))
            {
                Limit = limit,
                Offset = skip
            };
            var searchResult = await client.Search<Company>(TypesenseHelper.GetCollectionName<Company>(), searchParameters);

            return searchResult.Hits.Select(x => x.Document);
        }
    }
}
