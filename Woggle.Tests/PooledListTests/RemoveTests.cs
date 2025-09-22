namespace Woggle.Tests.PooledListTests;

public class RemoveTests
{
    [Fact]
    public void RemoveAt_RemovesItemAndShiftsElements()
    {
        // Arrange
        using var list = new PooledList<string> { "a", "b", "c" };

        // Act
        list.RemoveAt(1); // Remove "b"

        // Assert
        Assert.Equal(2, list.Count);
        Assert.Equal("a", list[0]);
        Assert.Equal("c", list[1]);
    }

    [Fact]
    public void Remove_ExistingItem_ReturnsTrueAndRemovesItem()
    {
        // Arrange
        using var list = new PooledList<string> { "a", "b", "c" };

        // Act
        bool result = list.Remove("b");

        // Assert
        Assert.True(result);
        Assert.Equal(2, list.Count);
        Assert.DoesNotContain("b", list);
    }

    [Fact]
    public void Remove_NonExistingItem_ReturnsFalse()
    {
        // Arrange
        using var list = new PooledList<string> { "a", "b", "c" };

        // Act
        bool result = list.Remove("d");

        // Assert
        Assert.False(result);
        Assert.Equal(3, list.Count);
    }
}
