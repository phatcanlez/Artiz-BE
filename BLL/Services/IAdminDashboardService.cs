using BO.DTOs;

namespace BLL.Services;

public interface IAdminDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}


