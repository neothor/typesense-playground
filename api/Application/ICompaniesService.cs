using Api.Domain;

namespace Api.Application;

public interface ICompaniesService
{
    Task<IEnumerable<Company>> GetAsync(
        string tenant,
        string? query = null,
        int limit = 20,
        int skip = 0);
}