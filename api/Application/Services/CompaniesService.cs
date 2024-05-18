namespace Api.Application.Services;
using Api.Domain;

public class CompaniesService : ICompaniesService
{
    private readonly ICompaniesRepository _repository;
    private readonly ICompanySearchService _searchService;

    public CompaniesService(ICompaniesRepository repository, ICompanySearchService searchService)
    {
        _repository = repository;
        _searchService = searchService;
    }

    public async Task<IEnumerable<Company>> GetAsync(string tenant, string? query = null, int limit = 20, int skip = 0)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await _repository.GetAsync(tenant, limit: limit, skip: skip);
        }

        var searchResult = await _searchService.SearchAsync(tenant, query, limit, skip);
        return await _repository.GetAsync(tenant, c => searchResult.Any(x => x.Id == c.Id), limit, skip);
    }
}