namespace Api.Application.Controllers
{
    using Api.Application.Services;
    using Api.Domain;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("tenants/{tenant}/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompaniesService _companiesService;
        private readonly ILogger<CompaniesController> _logger;

        public CompaniesController(ICompaniesService companiesService,  ILogger<CompaniesController> logger)
        {
            _companiesService = companiesService;
            _logger = logger;
        }

        [HttpGet(Name = "GetCompanies")]
        public async Task<ActionResult<IEnumerable<Company>>> Get(
            [FromRoute(Name = "tenant")]char tenant, 
            [FromQuery(Name = "q")]string? query = null,
            [FromQuery]int limit = 20,
            [FromQuery]int skip = 0)
        {
            var companies = await _companiesService.GetAsync(Tenant.GetTenantKey(tenant), query, limit, skip);
            return Ok(companies);
        }
    }
}
