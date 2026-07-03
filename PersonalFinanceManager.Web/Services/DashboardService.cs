using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public class DashboardService : IDashboardService
{
    private readonly IApiClient _api;

    public DashboardService(IApiClient api)
    {
        _api = api;
    }

    public async Task<ApiResult<DashboardDto>> GetAsync()
        => await _api.GetAsync<DashboardDto>("api/dashboard");
}
