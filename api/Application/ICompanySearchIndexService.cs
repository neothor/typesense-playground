namespace Api.Application
{
    using Api.Domain;

    public interface ICompanySearchIndexService
    {
        Task IndexAsync(ICollection<Company> companies);
    }
}