namespace Woggle.Tests.PooledListTests;

public class MiscMethodTests
{
    [Fact]
    public void Clear_ResetsCountToZero()
    {
        // Arrange
        using var list = new PooledList<int> { 1, 2, 3 };

        // Act
        list.Clear();

        // Assert
        Assert.Empty(list);
    }

    [Fact]
    public void Contains_FindsExistingItem()
    {
        // Arrange
        using var list = new PooledList<string> { "a", "b", "c" };

        // Assert
        Assert.Contains("b", list);
        Assert.DoesNotContain("d", list);
    }

    [Fact]
    public void IndexOf_FindsFirstOccurrence()
    {
        // Arrange
        using var list = new PooledList<string> { "a", "b", "b", "c" };

        // Assert
        Assert.Equal(1, list.IndexOf("b"));
        Assert.Equal(-1, list.IndexOf("d"));
    }

    [Fact]
    public void CopyTo_CopiesAllElementsToDestinationArray()
    {
        // Arrange
        using var list = new PooledList<int> { 10, 20, 30 };
        var destination = new int[5];

        // Act - Using corrected CopyTo implementation
        list.CopyTo(destination, 1);

        // Assert
        Assert.Equal([0, 10, 20, 30, 0], destination);
    }
}
