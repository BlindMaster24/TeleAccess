namespace TeleAccess.Core;

public sealed class NullSpeechService : ISpeechService
{
    public bool IsAvailable => false;

    public string BackendName => string.Empty;

    public void Speak(string text, bool interrupt = true)
    {
    }

    public void StopSpeaking()
    {
    }
}
