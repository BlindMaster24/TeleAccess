using Prismatoid;
using TeleAccess.Core;

namespace TeleAccess.App.Services;

public sealed class PrismSpeechService : ISpeechService, IDisposable
{
    private PrismContext? _context;
    private IPrismBackend? _backend;

    public PrismSpeechService()
    {
        try
        {
            _context = new PrismContext(null, 1000, 3, 5000, false);
            _backend = _context.AcquireBestBackend();
        }
        catch (Exception)
        {
            _backend = null;
        }
    }

    public bool IsAvailable => _backend is not null;

    public string BackendName => _backend?.Name ?? string.Empty;

    public void Speak(string text, bool interrupt = true)
    {
        if (_backend is null || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        try
        {
            _backend.Speak(text, interrupt);
        }
        catch (Exception)
        {
            _backend = null;
        }
    }

    public void StopSpeaking()
    {
        try
        {
            _backend?.Stop();
        }
        catch (Exception)
        {
            _backend = null;
        }
    }

    public void Dispose()
    {
        (_backend as IDisposable)?.Dispose();
        _backend = null;
        _context?.Dispose();
        _context = null;
    }
}
