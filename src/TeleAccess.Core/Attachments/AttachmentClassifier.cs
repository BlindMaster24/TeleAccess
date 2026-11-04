namespace TeleAccess.Core.Attachments;

public static class AttachmentClassifier
{
    private static readonly string[] _photos = [".JPG", ".JPEG", ".PNG", ".BMP", ".TIF", ".TIFF"];

    private static readonly string[] _animations = [".GIF"];

    private static readonly string[] _videos = [".MP4", ".MOV", ".AVI", ".MKV", ".WEBM"];

    private static readonly string[] _stickers = [".WEBP", ".TGS"];

    private static readonly string[] _audios = [".MP3", ".M4A", ".FLAC", ".AAC", ".WAV", ".OGG", ".OPUS"];

    public static AttachmentKind Classify(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var extension = Path.GetExtension(path).ToUpperInvariant();

        if (_photos.Contains(extension, StringComparer.Ordinal))
        {
            return AttachmentKind.Photo;
        }

        if (_animations.Contains(extension, StringComparer.Ordinal))
        {
            return AttachmentKind.Animation;
        }

        if (_videos.Contains(extension, StringComparer.Ordinal))
        {
            return AttachmentKind.Video;
        }

        if (_stickers.Contains(extension, StringComparer.Ordinal))
        {
            return AttachmentKind.Sticker;
        }

        return _audios.Contains(extension, StringComparer.Ordinal) ? AttachmentKind.Audio : AttachmentKind.Document;
    }
}
