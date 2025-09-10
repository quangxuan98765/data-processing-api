using DataProcessingAPI.Application.DTOs;

namespace DataProcessingAPI.Application.Interfaces;

public interface ISpeedTestService
{
    Task<List<SpeedTestDto>> GetSpeedTestResultsAsync(DateTime? startDate, DateTime? endDate);
    Task<SpeedTestDto?> GetSpeedTestByIdAsync(long id);
    Task<long> CreateSpeedTestAsync(SpeedTestDto speedTest);
    Task<bool> UpdateSpeedTestAsync(long id, SpeedTestDto speedTest);
    Task<bool> DeleteSpeedTestAsync(long id, string userId);
}
