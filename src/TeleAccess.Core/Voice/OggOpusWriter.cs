using System.Buffers.Binary;

namespace TeleAccess.Core.Voice;

public sealed class OggOpusWriter : IDisposable
{
    private const int MaxSegments = 255;

    private static readonly byte[] CapturePattern = "OggS"u8.ToArray();

    private static readonly byte[] OpusHeadMagic = "OpusHead"u8.ToArray();

    private static readonly byte[] OpusTagsMagic = "OpusTags"u8.ToArray();

    private readonly Stream _output;
    private readonly int _channels;
    private readonly int _sampleRate;
    private readonly uint _serial;
    private readonly List<byte[]> _pending = [];
    private int _sequence;
    private long _granule;
    private bool _headersWritten;
    private bool _disposed;

    public OggOpusWriter(Stream output, int sampleRate = 48000, int channels = 1, uint serial = 0x71456141)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(channels);

        _output = output;
        _sampleRate = sampleRate;
        _channels = channels;
        _serial = serial;
    }

    public void WritePacket(ReadOnlySpan<byte> packet, int samples)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(samples);

        WriteHeaders();

        _pending.Add(packet.ToArray());
        _granule += samples;

        if (_pending.Count >= MaxSegments)
        {
            FlushPage();
        }
    }

    public void Complete()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        WriteHeaders();

        if (_pending.Count > 0)
        {
            FlushPage();
        }

        _output.Flush();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _output.Dispose();
    }

    private void WriteHeaders()
    {
        if (_headersWritten)
        {
            return;
        }

        var head = new byte[19];
        OpusHeadMagic.CopyTo(head, 0);
        head[8] = 1;
        head[9] = (byte)_channels;
        BinaryPrimitives.WriteUInt16LittleEndian(head.AsSpan(10), 312);
        BinaryPrimitives.WriteUInt32LittleEndian(head.AsSpan(12), (uint)_sampleRate);
        BinaryPrimitives.WriteInt16LittleEndian(head.AsSpan(16), 0);
        head[18] = 0;

        WritePage(head, 0);

        var vendor = "TeleAccess"u8;
        var tags = new byte[OpusTagsMagic.Length + 4 + vendor.Length + 4];
        OpusTagsMagic.CopyTo(tags, 0);
        BinaryPrimitives.WriteUInt32LittleEndian(tags.AsSpan(8), (uint)vendor.Length);
        vendor.CopyTo(tags.AsSpan(12));
        BinaryPrimitives.WriteUInt32LittleEndian(tags.AsSpan(12 + vendor.Length), 0);

        WritePage(tags, 0);
        _headersWritten = true;
    }

    private void FlushPage()
    {
        if (_pending.Count == 0)
        {
            return;
        }

        var payloadSize = _pending.Sum(p => p.Length);
        var page = new byte[payloadSize];
        var offset = 0;

        foreach (var packet in _pending)
        {
            packet.CopyTo(page, offset);
            offset += packet.Length;
        }

        WritePage(page, _granule);
        _pending.Clear();
    }

    private void WritePage(byte[] payload, long granule)
    {
        var segments = BuildSegmentTable(payload.Length);
        var headerSize = 27 + segments.Length;
        var page = new byte[headerSize + payload.Length];
        var span = page.AsSpan();

        CapturePattern.CopyTo(span);
        span[4] = 0;
        span[5] = (byte)(_pending.Count > 0 ? 0 : 0x02);
        BinaryPrimitives.WriteInt64LittleEndian(span[6..], granule);
        BinaryPrimitives.WriteUInt32LittleEndian(span[14..], _serial);
        BinaryPrimitives.WriteInt32LittleEndian(span[18..], _sequence++);
        BinaryPrimitives.WriteUInt32LittleEndian(span[22..], 0);
        span[26] = (byte)segments.Length;
        segments.CopyTo(span[27..]);
        payload.CopyTo(span[headerSize..]);

        BinaryPrimitives.WriteUInt32LittleEndian(span[22..], Crc32(span));

        _output.Write(page);
    }

    private static byte[] BuildSegmentTable(int payloadSize)
    {
        var table = new List<byte>();

        while (payloadSize >= 255)
        {
            table.Add(255);
            payloadSize -= 255;
        }

        table.Add((byte)payloadSize);
        return [.. table];
    }

    private static uint Crc32(ReadOnlySpan<byte> data)
    {
        uint crc = 0;

        foreach (var value in data)
        {
            crc ^= (uint)value << 24;

            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc & 0x80000000) != 0 ? (crc << 1) ^ 0x04C11DB7 : crc << 1;
            }
        }

        return crc;
    }
}
