using CAR.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CAR.Api.Services.Interfaces;

public interface ICarOrchestrator
{
    Task<CarResponse> ProcessAsync(
        CarRequest request,
        CancellationToken cancellationToken = default);
}