namespace Api.Application.Jobs
{
    using Api.Application.Services;
    using Api.Domain;
    using Bogus;

    public class SearchIndexSeedingJob : BackgroundService
    {
        private static readonly char[] Tenants = ['A', 'B', 'C'];
        private readonly ICompanySearchIndexService _indexService;
        private readonly ICompaniesRepository _repository;
        private ILogger<SearchIndexSeedingJob> _logger;
        private const int CompaniesPerTenant = 100;

        public SearchIndexSeedingJob(ICompanySearchIndexService indexService, ICompaniesRepository repository, ILogger<SearchIndexSeedingJob> logger)
        {
            _indexService = indexService;
            _repository = repository;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            await IndexCompanies();
        }

        private async Task IndexCompanies()
        {
            var companies = GenerateTenantsWithCompanies();
            await _indexService.IndexAsync(companies);
            _repository.SetSource(companies);
        }

        private List<Company> GenerateTenantsWithCompanies()
        {
            List<Company> result = new List<Company>();
            var multiplier = 1;
            foreach (var tenant in Tenants)
            {
                var prefix = 10000 * multiplier;
                var companyFaker = new Faker<Company>()
                    .RuleFor(x => x.Tenant, f => Tenant.GetTenantKey(tenant))
                    .RuleFor(c => c.CompanyName, f => f.Company.CompanyName())
                    .RuleFor(c => c.Address, f => f.Address.StreetAddress())
                    .RuleFor(c => c.City, f => f.Address.City())
                    .RuleFor(c => c.Country, f => f.Address.Country())
                    .RuleFor(c => c.PhoneNumber, f => f.Phone.PhoneNumber())
                    .RuleFor(c => c.Email, f => f.Internet.Email());

                for (var i = 0; i < CompaniesPerTenant; i++)
                {
                    var company = companyFaker.Generate();
                    company.Id = (prefix + i).ToString();
                    result.Add(company);
                }

                multiplier++;
            }

            return result;
        }
    }
}
