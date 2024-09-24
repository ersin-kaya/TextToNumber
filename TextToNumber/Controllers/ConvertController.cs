using Microsoft.AspNetCore.Mvc;
using TextToNumber.Models.Requests;
using TextToNumber.Services;

namespace TextToNumber.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConvertController : ControllerBase
{
    private readonly IConvertService _convertService;

    public ConvertController(IConvertService convertService)
    {
        _convertService = convertService;
    }

    [HttpPost("convert")]
    public async Task<IActionResult> Convert([FromBody] ConvertTextToNumberRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = _convertService.ConvertTextToNumber(request);
        return Ok(result);
    }
}