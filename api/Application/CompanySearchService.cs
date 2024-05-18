using Api.Domain;

namespace Api.Application
{

    public class CompanySearchService : ICompanySearchService
    {
        public Task<IEnumerable<IIndexable>> SearchAsync(string tenant, string query, int limit, int skip)
        {
            throw new NotImplementedException();
        }
    }
}
