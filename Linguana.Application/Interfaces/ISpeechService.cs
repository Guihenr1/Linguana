using Microsoft.AspNetCore.Http;

namespace Linguana.Application.Interfaces;

public interface ISpeechService
{
    Task<string> TranscribeAudioAsync(IFormFile audioFile);
}