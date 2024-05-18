namespace Api.Domain;

public class CompaniesRepository : ICompaniesRepository
{
    private IEnumerable<Company> _companies = Enumerable.Empty<Company>();

    public void SetSource(IEnumerable<Company> companies)
    {
        _companies = companies;
    }

    public Task<IEnumerable<Company>> GetAsync(string tenant, Func<Company, bool>? where = null, int limit = 20,
        int skip = 0)
    {
        var query = _companies.Where(c => c.Tenant == tenant);
        if (where != null)
        {
            query = query.Where(where);
        }

        return Task.FromResult(query.Skip(skip).Take(limit));
    }
}