namespace TeleAccess.Core.Formatting;

public static class SimpleMarkup
{
    public static MarkupResult Parse(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return new MarkupResult(string.Empty, []);
        }

        var text = new System.Text.StringBuilder(input.Length);
        var entities = new List<EntitySpan>();

        var index = 0;
        while (index < input.Length)
        {
            if (TryReadDelimited(input, index, "**", EntityKind.Bold, text, entities, out var next) ||
                TryReadDelimited(input, index, "__", EntityKind.Bold, text, entities, out next) ||
                TryReadDelimited(input, index, "_", EntityKind.Italic, text, entities, out next) ||
                TryReadDelimited(input, index, "`", EntityKind.Code, text, entities, out next))
            {
                index = next;
                continue;
            }

            text.Append(input[index]);
            index++;
        }

        return new MarkupResult(text.ToString(), entities);
    }

    private static bool TryReadDelimited(
        string input,
        int start,
        string delimiter,
        EntityKind kind,
        System.Text.StringBuilder text,
        List<EntitySpan> entities,
        out int next)
    {
        next = start;

        if (string.CompareOrdinal(input, start, delimiter, 0, delimiter.Length) != 0)
        {
            return false;
        }

        var contentStart = start + delimiter.Length;
        var end = input.IndexOf(delimiter, contentStart, StringComparison.Ordinal);

        if (end <= contentStart)
        {
            return false;
        }

        var content = input[contentStart..end];

        if (content.Contains(delimiter, StringComparison.Ordinal))
        {
            return false;
        }

        var offset = text.Length;
        text.Append(content);
        entities.Add(new EntitySpan(offset, content.Length, kind));
        next = end + delimiter.Length;
        return true;
    }
}
