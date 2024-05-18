namespace Api.Application.Services
{
    using Api.Domain;

    public interface ICompanySearchService
    {
        Task<IEnumerable<IIndexable>> SearchAsync(string tenant, string query, int limit, int skip);
    }
}