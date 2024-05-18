namespace Api.Domain;

public interface ICompaniesRepository
{
    void SetSource(IEnumerable<Company> companies);

    Task<IEnumerable<Company>> GetAsync(string tenant, Func<Company, bool>? where = null, int limit = 20, int skip = 0);
}