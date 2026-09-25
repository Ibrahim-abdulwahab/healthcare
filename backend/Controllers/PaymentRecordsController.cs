using Healthcare.Api.DTOs.PaymentRecords;
using Healthcare.Api.Services.PaymentRecords;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentRecordsController : ControllerBase
{
    private readonly IPaymentRecordService _paymentRecordService;

    public PaymentRecordsController(
        IPaymentRecordService paymentRecordService)
    {
        _paymentRecordService = paymentRecordService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentRecordDto>>> GetAll()
    {
        var paymentRecords =
            await _paymentRecordService.GetAllAsync();

        return Ok(paymentRecords);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentRecordDto>> GetById(int id)
    {
        var paymentRecord =
            await _paymentRecordService.GetByIdAsync(id);

        if (paymentRecord is null)
            return NotFound();

        return Ok(paymentRecord);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentRecordDto>> Create(
        CreatePaymentRecordDto dto)
    {
        try
        {
            var paymentRecord =
                await _paymentRecordService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = paymentRecord.Id },
                paymentRecord
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PaymentRecordDto>> Update(
        int id,
        UpdatePaymentRecordDto dto)
    {
        try
        {
            var paymentRecord =
                await _paymentRecordService.UpdateAsync(id, dto);

            if (paymentRecord is null)
                return NotFound();

            return Ok(paymentRecord);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _paymentRecordService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}