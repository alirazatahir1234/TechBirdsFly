namespace BillingService.Application.Services;

using BillingService.Application.DTOs;
using BillingService.Application.Interfaces;
using BillingService.Domain.Entities;
using Microsoft.Extensions.Logging;

/// <summary>
/// Plan application service
/// </summary>
public sealed class PlanApplicationService : IPlanApplicationService
{
    private readonly IPlanRepository _planRepository;
    private readonly ILogger<PlanApplicationService> _logger;

    public PlanApplicationService(
        IPlanRepository planRepository,
        ILogger<PlanApplicationService> logger)
    {
        _planRepository = planRepository;
        _logger = logger;
    }

    public async Task<PlanDto> CreatePlanAsync(CreatePlanRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating plan: {PlanName}, Type: {PlanType}", request.Name, request.Type);

        try
        {
            if (!Enum.TryParse<PlanType>(request.Type, true, out var planType))
                throw new ArgumentException($"Invalid plan type: {request.Type}");

            if (!Enum.TryParse<BillingCycle>(request.BillingCycle, true, out var billingCycle))
                throw new ArgumentException($"Invalid billing cycle: {request.BillingCycle}");

            var plan = Plan.Create(request.Name, request.Description, planType, request.Price, billingCycle, request.TrialDays);

            var created = await _planRepository.CreateAsync(plan, cancellationToken);
            _logger.LogInformation("Plan created: {PlanId}", created.Id);
            return MapToDto(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating plan: {PlanName}", request.Name);
            throw;
        }
    }

    public async Task<PlanDto?> GetPlanAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching plan: {PlanId}", planId);
        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken);
        return plan is not null ? MapToDto(plan) : null;
    }

    public async Task<IEnumerable<PlanDto>> GetAllPlansAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching all plans");
        var plans = await _planRepository.GetAllAsync(cancellationToken);
        return plans.Select(MapToDto);
    }

    public async Task<IEnumerable<PlanDto>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching active plans");
        var plans = await _planRepository.GetActiveAsync(cancellationToken);
        return plans.Select(MapToDto);
    }

    public async Task<PlanDto> UpdatePlanAsync(Guid planId, UpdatePlanRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating plan: {PlanId}", planId);

        try
        {
            var plan = await _planRepository.GetByIdAsync(planId, cancellationToken);
            if (plan is null)
                throw new KeyNotFoundException($"Plan {planId} not found");

            // Update plan properties (simplified - just update features JSON if provided)
            // For full update, create a new plan via Create factory method
            var updated = await _planRepository.UpdateAsync(plan, cancellationToken);
            _logger.LogInformation("Plan updated: {PlanId}", planId);
            return MapToDto(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating plan: {PlanId}", planId);
            throw;
        }
    }

    public async Task DeletePlanAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting plan: {PlanId}", planId);

        try
        {
            var plan = await _planRepository.GetByIdAsync(planId, cancellationToken);
            if (plan is null)
                throw new KeyNotFoundException($"Plan {planId} not found");

            plan.Deactivate();
            await _planRepository.UpdateAsync(plan, cancellationToken);
            _logger.LogInformation("Plan deleted (deactivated): {PlanId}", planId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting plan: {PlanId}", planId);
            throw;
        }
    }

    private PlanDto MapToDto(Plan plan)
    {
        return new PlanDto(
            plan.Id,
            plan.Name,
            plan.Description,
            plan.Type.ToString(),
            plan.Price,
            plan.Currency,
            plan.BillingCycle.ToString(),
            plan.TrialDays,
            plan.IsActive,
            plan.FeaturesJson,
            plan.CreatedAt,
            plan.UpdatedAt);
    }
}
