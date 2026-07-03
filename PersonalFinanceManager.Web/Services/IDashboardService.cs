using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public interface IDashboardService
{
    Task<ApiResult<DashboardDto>> GetAsync();
}
