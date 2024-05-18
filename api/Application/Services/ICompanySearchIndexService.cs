namespace Api.Application.Services
{
    using Api.Domain;

    public interface ICompanySearchIndexService
    {
        Task IndexAsync(ICollection<Company> companies);
    }
}