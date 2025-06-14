using Linguana.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Linguana.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeechController : ControllerBase
{
    private readonly ISpeechService _speechService;

    public SpeechController(ISpeechService speechService)
    {
        _speechService = speechService;
    }

    [HttpPost("transcribe")]
    public async Task<IActionResult> Transcribe([FromForm] IFormFile audioFile)
    {
        if (audioFile == null || audioFile.Length == 0)
            return BadRequest("No audio file provided.");

        try
        {
            var transcription = await _speechService.TranscribeAudioAsync(audioFile);

            return Ok(new { transcription });
        } catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred during transcription.", details = ex.Message });
        }
    }
}