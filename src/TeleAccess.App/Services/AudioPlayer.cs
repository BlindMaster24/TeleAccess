using NAudio.Wave;

namespace TeleAccess.App.Services;

public sealed class AudioPlayer : IDisposable
{
    private WaveOut? _output;
    private MediaFoundationReader? _reader;

    public bool IsPlaying => _output?.PlaybackState == PlaybackState.Playing;

    public void Play(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        Stop();
        _reader = new MediaFoundationReader(path);
        _output = new WaveOut();
        _output.Init(_reader);
        _output.Play();
    }

    public void Stop()
    {
        _output?.Stop();
        _output?.Dispose();
        _output = null;
        _reader?.Dispose();
        _reader = null;
    }

    public void Dispose() => Stop();
}
