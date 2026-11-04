using System.IO;
using Concentus;
using Concentus.Enums;
using NAudio.Wave;
using TeleAccess.Core.Voice;

namespace TeleAccess.App.Services;

public sealed class VoiceRecorder : IDisposable
{
    private const int SampleRate = 48000;
    private const int FrameSamples = 960;

    private readonly List<short> _pcm = [];
    private WaveIn? _input;
    private bool _disposed;

    public bool IsRecording => _input is not null;

    public TimeSpan Duration => TimeSpan.FromSeconds((double)_pcm.Count / SampleRate);

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_input is not null)
        {
            return;
        }

        _pcm.Clear();

        _input = new WaveIn
        {
            WaveFormat = new WaveFormat(SampleRate, 16, 1),
            BufferMilliseconds = 20,
            NumberOfBuffers = 3
        };

        _input.DataAvailable += OnDataAvailable;
        _input.StartRecording();
    }

    public string? StopAndEncode()
    {
        if (_input is null)
        {
            return null;
        }

        _input.DataAvailable -= OnDataAvailable;
        _input.StopRecording();
        _input.Dispose();
        _input = null;

        if (_pcm.Count < FrameSamples)
        {
            return null;
        }

        var path = Path.Combine(Path.GetTempPath(), $"teleaccess-voice-{Guid.NewGuid():N}.ogg");

        using (var stream = File.Create(path))
        using (var writer = new OggOpusWriter(stream, SampleRate, 1))
        using (var encoder = OpusCodecFactory.CreateEncoder(SampleRate, 1, OpusApplication.OPUS_APPLICATION_VOIP))
        {
            encoder.Bitrate = 24000;

            var frame = new short[FrameSamples];
            var encoded = new byte[1024];
            var offset = 0;

            while (offset + FrameSamples <= _pcm.Count)
            {
                _pcm.CopyTo(offset, frame, 0, FrameSamples);
                var written = encoder.Encode(frame, FrameSamples, encoded, encoded.Length);
                writer.WritePacket(encoded.AsSpan(0, written), FrameSamples);
                offset += FrameSamples;
            }

            writer.Complete();
        }

        return path;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_input is not null)
        {
            _input.DataAvailable -= OnDataAvailable;
            _input.StopRecording();
            _input.Dispose();
            _input = null;
        }
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        for (var i = 0; i + 1 < e.BytesRecorded; i += 2)
        {
            _pcm.Add(BitConverter.ToInt16(e.Buffer, i));
        }
    }
}
