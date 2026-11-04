namespace TeleAccess.Core.Keyboards;

public static class KeyboardCursor
{
    public static KeyboardPosition Move(KeyboardPosition current, KeyboardDirection direction, int rows, int columns)
    {
        if (rows < 0 || columns < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and columns cannot be negative.");
        }

        if (rows == 0 || columns == 0)
        {
            return KeyboardPosition.Start;
        }

        var row = Math.Clamp(current.Row, 1, rows);
        var column = Math.Clamp(current.Column, 1, columns);

        return direction switch
        {
            KeyboardDirection.Up => new KeyboardPosition(Math.Max(1, row - 1), column),
            KeyboardDirection.Down => new KeyboardPosition(Math.Min(rows, row + 1), column),
            KeyboardDirection.Left => new KeyboardPosition(row, Math.Max(1, column - 1)),
            KeyboardDirection.Right => new KeyboardPosition(row, Math.Min(columns, column + 1)),
            _ => new KeyboardPosition(row, column)
        };
    }

    public static int IndexOf(KeyboardPosition position, int columns) =>
        columns <= 0 ? 0 : ((position.Row - 1) * columns) + position.Column - 1;
}
