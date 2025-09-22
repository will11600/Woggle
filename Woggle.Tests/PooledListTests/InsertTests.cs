namespace Woggle.Tests.PooledListTests;

public class InsertTests
{
    [Fact]
    public void Insert_InMiddle_ShiftsElementsAndInsertsItem()
    {
        // Arrange
        using var list = new PooledList<int> { 10, 30 };

        // Act
        list.Insert(1, 20);

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(10, list[0]);
        Assert.Equal(20, list[1]);
        Assert.Equal(30, list[2]);
    }

    [Fact]
    public void Insert_AtEnd_BehavesLikeAdd()
    {
        // Arrange
        using var list = new PooledList<int> { 10, 20 };

        // Act
        list.Insert(2, 30);

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(30, list[2]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)] // Greater than Count
    public void Insert_WithInvalidIndex_ThrowsArgumentOutOfRangeException(int index)
    {
        // Arrange
        using var list = new PooledList<int> { 10, 20, 30 };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(index, 99));
    }
}
