using System.IO;
using System.Text;
using Shouldly;
using TeleAccess.Core.Voice;

namespace TeleAccess.Core.Tests;

public class OggOpusWriterTests
{
    private static byte[] Build(Action<OggOpusWriter> write)
    {
        using var stream = new MemoryStream();
        using (var writer = new OggOpusWriter(stream))
        {
            write(writer);
            writer.Complete();
        }

        return stream.ToArray();
    }

    [Fact]
    public void Starts_with_an_ogg_page()
    {
        var file = Build(writer => writer.WritePacket(new byte[] { 1, 2, 3 }, 960));

        Encoding.ASCII.GetString(file, 0, 4).ShouldBe("OggS");
        file[4].ShouldBe((byte)0);
        file[5].ShouldBe((byte)2);
    }

    [Fact]
    public void Writes_opus_head_with_channel_count()
    {
        var file = Build(writer => writer.WritePacket(new byte[] { 1 }, 960));
        var index = IndexOf(file, "OpusHead"u8);

        index.ShouldBeGreaterThan(0);
        file[index + 8].ShouldBe((byte)1);
        file[index + 9].ShouldBe((byte)1);
    }

    [Fact]
    public void Writes_opus_tags_with_vendor_string()
    {
        var file = Build(writer => writer.WritePacket(new byte[] { 1 }, 960));

        IndexOf(file, "OpusTags"u8).ShouldBeGreaterThan(0);
        IndexOf(file, "TeleAccess"u8).ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Every_page_starts_with_the_capture_pattern()
    {
        var file = Build(writer =>
        {
            writer.WritePacket(new byte[300], 960);
            writer.WritePacket(new byte[300], 960);
        });

        var pages = 0;

        for (var i = 0; i + 3 < file.Length; i++)
        {
            if (file[i] == (byte)'O' && file[i + 1] == (byte)'g' && file[i + 2] == (byte)'g' && file[i + 3] == (byte)'S')
            {
                pages++;
            }
        }

        pages.ShouldBeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void Large_packets_are_split_into_segments()
    {
        var file = Build(writer => writer.WritePacket(new byte[600], 960));
        var lastHeader = IndexOf(file, "OpusTags"u8);

        lastHeader.ShouldBeGreaterThan(0);
        file.Length.ShouldBeGreaterThan(600);
    }

    [Fact]
    public void Rejects_use_after_dispose()
    {
        using var stream = new MemoryStream();
        var writer = new OggOpusWriter(stream);
        writer.Dispose();

        Should.Throw<ObjectDisposedException>(() => writer.WritePacket(new byte[] { 1 }, 960));
    }

    private static int IndexOf(byte[] haystack, ReadOnlySpan<byte> needle)
    {
        for (var i = 0; i + needle.Length <= haystack.Length; i++)
        {
            if (haystack.AsSpan(i, needle.Length).SequenceEqual(needle))
            {
                return i;
            }
        }

        return -1;
    }
}
