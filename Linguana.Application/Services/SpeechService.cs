using Linguana.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OpenAI.Audio;

namespace Linguana.Application.Services;

public class SpeechService : ISpeechService
{
    private readonly IConfiguration _configuration;

    public SpeechService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> TranscribeAudioAsync(IFormFile audioFile)
    {
        var openAiApiKey = _configuration["OpenAiApiKey"];
        var audioOptions = new AudioTranscriptionOptions()
        {
            ResponseFormat = AudioTranscriptionFormat.Srt,
            Language = "de"
        };

        // Get the file extension from the uploaded file
        var extension = Path.GetExtension(audioFile.FileName);
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}{extension}");

        using (var stream = System.IO.File.Create(tempFilePath))
        {
            await audioFile.CopyToAsync(stream);
        }

        var audioClient = new AudioClient("whisper-1", openAiApiKey);
        var response = await audioClient.TranscribeAudioAsync(tempFilePath, audioOptions);

        // Clean up the temporary file
        File.Delete(tempFilePath);

        return response.Value.Text;
    }
}