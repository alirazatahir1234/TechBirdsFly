namespace BillingService.WebAPI.Controllers;

using BillingService.Application.DTOs;
using BillingService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Payments controller for managing payments
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentApplicationService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentApplicationService paymentService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetPayment(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.GetPaymentAsync(id, cancellationToken);
        if (payment is null)
            return NotFound(ApiResponse<PaymentDto>.ErrorResponse("Payment not found"));

        return Ok(ApiResponse<PaymentDto>.SuccessResponse(payment));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> CreatePayment(
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentService.CreatePaymentAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, ApiResponse<PaymentDto>.SuccessResponse(payment, "Payment created"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(500, ApiResponse<PaymentDto>.ErrorResponse("Failed to create payment", ex.Message));
        }
    }

    [HttpPost("{id}/process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> ProcessPayment(
        Guid id,
        ProcessPaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentService.ProcessPaymentAsync(id, request, cancellationToken);
            return Ok(ApiResponse<PaymentDto>.SuccessResponse(payment, "Payment processed"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<PaymentDto>.ErrorResponse("Payment not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return StatusCode(500, ApiResponse<PaymentDto>.ErrorResponse("Failed to process payment", ex.Message));
        }
    }

    [HttpPost("{id}/refund")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> RefundPayment(
        Guid id,
        RefundPaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentService.RefundPaymentAsync(id, request, cancellationToken);
            return Ok(ApiResponse<PaymentDto>.SuccessResponse(payment, "Payment refunded"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<PaymentDto>.ErrorResponse("Payment not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaymentDto>.ErrorResponse("Failed to refund payment", ex.Message));
        }
    }
}

/// <summary>
/// Subscriptions controller for managing subscriptions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionApplicationService _subscriptionService;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        ISubscriptionApplicationService subscriptionService,
        ILogger<SubscriptionsController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> GetSubscription(Guid id, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionService.GetSubscriptionAsync(id, cancellationToken);
        if (subscription is null)
            return NotFound(ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found"));

        return Ok(ApiResponse<SubscriptionDto>.SuccessResponse(subscription));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> CreateSubscription(
        CreateSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await _subscriptionService.CreateSubscriptionAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, ApiResponse<SubscriptionDto>.SuccessResponse(subscription, "Subscription created"));
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ApiResponse<SubscriptionDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription");
            return StatusCode(500, ApiResponse<SubscriptionDto>.ErrorResponse("Failed to create subscription", ex.Message));
        }
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> CancelSubscription(
        Guid id,
        CancelSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await _subscriptionService.CancelSubscriptionAsync(id, request, cancellationToken);
            return Ok(ApiResponse<SubscriptionDto>.SuccessResponse(subscription, "Subscription cancelled"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SubscriptionDto>.ErrorResponse("Failed to cancel subscription", ex.Message));
        }
    }

    [HttpPost("{id}/renew")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> RenewSubscription(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await _subscriptionService.RenewSubscriptionAsync(id, cancellationToken);
            return Ok(ApiResponse<SubscriptionDto>.SuccessResponse(subscription, "Subscription renewed"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SubscriptionDto>.ErrorResponse("Failed to renew subscription", ex.Message));
        }
    }
}

/// <summary>
/// Plans controller for managing billing plans
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class PlansController : ControllerBase
{
    private readonly IPlanApplicationService _planService;
    private readonly ILogger<PlansController> _logger;

    public PlansController(
        IPlanApplicationService planService,
        ILogger<PlansController> logger)
    {
        _planService = planService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlanDto>>>> GetAllPlans(CancellationToken cancellationToken)
    {
        var plans = await _planService.GetAllPlansAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<PlanDto>>.SuccessResponse(plans));
    }

    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlanDto>>>> GetActivePlans(CancellationToken cancellationToken)
    {
        var plans = await _planService.GetActivePlansAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<PlanDto>>.SuccessResponse(plans));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PlanDto>>> GetPlan(Guid id, CancellationToken cancellationToken)
    {
        var plan = await _planService.GetPlanAsync(id, cancellationToken);
        if (plan is null)
            return NotFound(ApiResponse<PlanDto>.ErrorResponse("Plan not found"));

        return Ok(ApiResponse<PlanDto>.SuccessResponse(plan));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PlanDto>>> CreatePlan(
        CreatePlanRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _planService.CreatePlanAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetPlan), new { id = plan.Id }, ApiResponse<PlanDto>.SuccessResponse(plan, "Plan created"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PlanDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating plan");
            return StatusCode(500, ApiResponse<PlanDto>.ErrorResponse("Failed to create plan", ex.Message));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PlanDto>>> UpdatePlan(
        Guid id,
        UpdatePlanRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _planService.UpdatePlanAsync(id, request, cancellationToken);
            return Ok(ApiResponse<PlanDto>.SuccessResponse(plan, "Plan updated"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<PlanDto>.ErrorResponse("Plan not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PlanDto>.ErrorResponse("Failed to update plan", ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePlan(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _planService.DeletePlanAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Plan not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Failed to delete plan", ex.Message));
        }
    }
}
