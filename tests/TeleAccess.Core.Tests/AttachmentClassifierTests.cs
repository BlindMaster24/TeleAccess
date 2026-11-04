using Shouldly;
using TeleAccess.Core.Attachments;

namespace TeleAccess.Core.Tests;

public class AttachmentClassifierTests
{
    [Theory]
    [InlineData("photo.jpg", AttachmentKind.Photo)]
    [InlineData("photo.JPEG", AttachmentKind.Photo)]
    [InlineData("scan.PNG", AttachmentKind.Photo)]
    [InlineData("wave.gif", AttachmentKind.Animation)]
    [InlineData("clip.mp4", AttachmentKind.Video)]
    [InlineData("clip.MKV", AttachmentKind.Video)]
    [InlineData("sticker.webp", AttachmentKind.Sticker)]
    [InlineData("tgs.tgs", AttachmentKind.Sticker)]
    [InlineData("song.mp3", AttachmentKind.Audio)]
    [InlineData("voice.opus", AttachmentKind.Audio)]
    [InlineData("notes.txt", AttachmentKind.Document)]
    [InlineData("archive.zip", AttachmentKind.Document)]
    [InlineData("no-extension", AttachmentKind.Document)]
    public void Classifies_by_extension(string path, AttachmentKind expected)
    {
        AttachmentClassifier.Classify(path).ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_empty_paths(string path)
    {
        Should.Throw<ArgumentException>(() => AttachmentClassifier.Classify(path));
    }

    [Fact]
    public void Rejects_null()
    {
        Should.Throw<ArgumentException>(() => AttachmentClassifier.Classify(null!));
    }
}
