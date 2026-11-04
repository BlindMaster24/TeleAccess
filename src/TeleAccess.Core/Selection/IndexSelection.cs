namespace TeleAccess.Core.Selection;

public static class IndexSelection
{
    public static bool TrySelect(string? answer, int count, out int index)
    {
        index = -1;

        if (count <= 0 || string.IsNullOrWhiteSpace(answer))
        {
            return false;
        }

        if (!int.TryParse(answer.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var number))
        {
            return false;
        }

        if (number < 1 || number > count)
        {
            return false;
        }

        index = number - 1;
        return true;
    }
}
