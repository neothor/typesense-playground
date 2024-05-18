namespace Api.Application.Services
{
    using Typesense;

    public interface ISearchClientProvider
    {
        Task<ITypesenseClient> GetAdminClient();
        Task<ITypesenseClient> GetSearchClient(string tenant);
    }
}
