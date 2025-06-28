using Linguana.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Audio;

namespace Linguana.Application.Services;

public class SpeechService : ISpeechService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SpeechService> _logger;
    
    public SpeechService(IConfiguration configuration, ILogger<SpeechService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> TranscribeAudioAsync(IFormFile audioFile)
    {
        var openAiApiKey = _configuration["OpenAiApiKey"];
        
        if (string.IsNullOrEmpty(openAiApiKey))
        {
            _logger.LogError("OpenAI API key not found in configuration");
            throw new InvalidOperationException("OpenAI API key not configured");
        }

        try
        {
            var audioOptions = new AudioTranscriptionOptions()
            {
                ResponseFormat = AudioTranscriptionFormat.Srt,
                Language = "de"
            };

            var extension = Path.GetExtension(audioFile.FileName);
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}{extension}");

            using (var stream = System.IO.File.Create(tempFilePath))
            {
                await audioFile.CopyToAsync(stream);
            }

            var audioClient = new AudioClient("whisper-1", openAiApiKey);
            var response = await audioClient.TranscribeAudioAsync(tempFilePath, audioOptions);

            File.Delete(tempFilePath);
            return response.Value.Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transcribing audio file");
            throw;
        }
    }
}