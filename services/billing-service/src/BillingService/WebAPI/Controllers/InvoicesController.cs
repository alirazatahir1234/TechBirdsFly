namespace BillingService.WebAPI.Controllers;

using BillingService.Application.DTOs;
using BillingService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Invoices controller for managing invoices
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class InvoicesController : ControllerBase
{
    private readonly IInvoiceApplicationService _invoiceService;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(
        IInvoiceApplicationService invoiceService,
        ILogger<InvoicesController> logger)
    {
        _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all invoices
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceDto>>>> GetAllInvoices(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching all invoices");
            var invoices = await _invoiceService.GetUserInvoicesAsync(Guid.Empty, cancellationToken);
            return Ok(ApiResponse<IEnumerable<InvoiceDto>>.SuccessResponse(invoices, "Invoices retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching invoices");
            return StatusCode(500, ApiResponse<IEnumerable<InvoiceDto>>.ErrorResponse("Failed to retrieve invoices", ex.Message));
        }
    }

    /// <summary>
    /// Get invoice by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetInvoiceById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse<InvoiceDto>.ErrorResponse("Invalid invoice ID"));

            _logger.LogInformation("Fetching invoice: {InvoiceId}", id);
            var invoice = await _invoiceService.GetInvoiceAsync(id, cancellationToken);

            if (invoice is null)
                return NotFound(ApiResponse<InvoiceDto>.ErrorResponse("Invoice not found"));

            return Ok(ApiResponse<InvoiceDto>.SuccessResponse(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching invoice: {InvoiceId}", id);
            return StatusCode(500, ApiResponse<InvoiceDto>.ErrorResponse("Failed to retrieve invoice", ex.Message));
        }
    }

    /// <summary>
    /// Create a new invoice
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> CreateInvoice(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<InvoiceDto>.ErrorResponse("Invalid request data", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToArray()));

            if (request.Amount <= 0)
                return BadRequest(ApiResponse<InvoiceDto>.ErrorResponse("Amount must be greater than zero"));

            _logger.LogInformation("Creating invoice for user: {UserId}", request.UserId);
            var invoice = await _invoiceService.CreateInvoiceAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetInvoiceById), new { id = invoice.Id }, ApiResponse<InvoiceDto>.SuccessResponse(invoice, "Invoice created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice");
            return StatusCode(500, ApiResponse<InvoiceDto>.ErrorResponse("Failed to create invoice", ex.Message));
        }
    }

    /// <summary>
    /// Issue an invoice (move from draft to sent)
    /// </summary>
    [HttpPost("{id}/issue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> IssueInvoice(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse<InvoiceDto>.ErrorResponse("Invalid invoice ID"));

            _logger.LogInformation("Issuing invoice: {InvoiceId}", id);
            var invoice = await _invoiceService.IssueInvoiceAsync(id, cancellationToken);

            return Ok(ApiResponse<InvoiceDto>.SuccessResponse(invoice, "Invoice issued successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<InvoiceDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<InvoiceDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error issuing invoice: {InvoiceId}", id);
            return StatusCode(500, ApiResponse<InvoiceDto>.ErrorResponse("Failed to issue invoice", ex.Message));
        }
    }

    /// <summary>
    /// Update an invoice
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> UpdateInvoice(
        Guid id,
        UpdateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse<InvoiceDto>.ErrorResponse("Invalid invoice ID"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<InvoiceDto>.ErrorResponse("Invalid request data"));

            _logger.LogInformation("Updating invoice: {InvoiceId}", id);
            var invoice = await _invoiceService.UpdateInvoiceAsync(id, request, cancellationToken);

            return Ok(ApiResponse<InvoiceDto>.SuccessResponse(invoice, "Invoice updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<InvoiceDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<InvoiceDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice: {InvoiceId}", id);
            return StatusCode(500, ApiResponse<InvoiceDto>.ErrorResponse("Failed to update invoice", ex.Message));
        }
    }

    /// <summary>
    /// Delete an invoice
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteInvoice(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid invoice ID"));

            _logger.LogInformation("Deleting invoice: {InvoiceId}", id);
            await _invoiceService.DeleteInvoiceAsync(id, cancellationToken);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting invoice: {InvoiceId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Failed to delete invoice", ex.Message));
        }
    }
}
