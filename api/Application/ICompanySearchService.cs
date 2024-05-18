using Api.Domain;

namespace Api.Application
{
    public interface ICompanySearchService
    {
        Task<IEnumerable<IIndexable>> SearchAsync(string tenant, string query, int limit, int skip);
    }
}