using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Application.Dashboard;
using PersonalFinanceManager.Application.Dashboard.Dtos;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
public class DashboardController : BaseApiController
{
	private readonly DashboardAppService _dashboard;

	public DashboardController(DashboardAppService dashboard) => _dashboard = dashboard;

	/// <summary>Returns aggregated totals, recent transactions, active budgets, and spending by category.</summary>
	[HttpGet]
	public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
		=> Ok(await _dashboard.GetAsync(ct));
}
