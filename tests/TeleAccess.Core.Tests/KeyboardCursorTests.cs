using Shouldly;
using TeleAccess.Core.Keyboards;

namespace TeleAccess.Core.Tests;

public class KeyboardCursorTests
{
    [Fact]
    public void Moves_inside_the_grid()
    {
        var position = KeyboardCursor.Move(new KeyboardPosition(2, 2), KeyboardDirection.Right, 3, 4);
        position.ShouldBe(new KeyboardPosition(2, 3));

        position = KeyboardCursor.Move(position, KeyboardDirection.Down, 3, 4);
        position.ShouldBe(new KeyboardPosition(3, 3));
    }

    [Fact]
    public void Clamps_at_the_edges()
    {
        KeyboardCursor.Move(new KeyboardPosition(1, 1), KeyboardDirection.Up, 3, 4).ShouldBe(new KeyboardPosition(1, 1));
        KeyboardCursor.Move(new KeyboardPosition(1, 1), KeyboardDirection.Left, 3, 4).ShouldBe(new KeyboardPosition(1, 1));
        KeyboardCursor.Move(new KeyboardPosition(3, 4), KeyboardDirection.Down, 3, 4).ShouldBe(new KeyboardPosition(3, 4));
        KeyboardCursor.Move(new KeyboardPosition(3, 4), KeyboardDirection.Right, 3, 4).ShouldBe(new KeyboardPosition(3, 4));
    }

    [Fact]
    public void Returns_start_for_an_empty_grid()
    {
        KeyboardCursor.Move(new KeyboardPosition(2, 2), KeyboardDirection.Right, 0, 0).ShouldBe(KeyboardPosition.Start);
    }

    [Fact]
    public void Clamps_an_initial_out_of_range_position()
    {
        KeyboardCursor.Move(new KeyboardPosition(9, 9), KeyboardDirection.Left, 2, 2).ShouldBe(new KeyboardPosition(2, 1));
    }

    [Fact]
    public void Index_is_zero_based_over_the_grid()
    {
        KeyboardCursor.IndexOf(new KeyboardPosition(1, 1), 4).ShouldBe(0);
        KeyboardCursor.IndexOf(new KeyboardPosition(2, 3), 4).ShouldBe(6);
        KeyboardCursor.IndexOf(KeyboardPosition.Start, 0).ShouldBe(0);
    }

    [Fact]
    public void Negative_sizes_are_rejected()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => KeyboardCursor.Move(KeyboardPosition.Start, KeyboardDirection.Up, -1, 2));
    }
}
