namespace TeleAccess.Core;

public interface ISpeechService
{
    bool IsAvailable { get; }

    string BackendName { get; }

    void Speak(string text, bool interrupt = true);

    void StopSpeaking();
}
