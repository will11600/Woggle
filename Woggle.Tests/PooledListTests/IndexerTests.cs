namespace Woggle.Tests.PooledListTests;

public class IndexerTests
{
    [Fact]
    public void Indexer_Get_ReturnsCorrectItem()
    {
        // Arrange
        using var list = new PooledList<int> { 10, 20, 30 };

        // Act & Assert
        Assert.Equal(20, list[1]);
    }

    [Fact]
    public void Indexer_Set_UpdatesItemValue()
    {
        // Arrange
        using var list = new PooledList<int> { 10, 20, 30 };

        // Act
        list[1] = 99;

        // Assert
        Assert.Equal(99, list[1]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)] // Equal to Count
    public void Indexer_Get_WithInvalidIndex_ThrowsArgumentOutOfRangeException(int index)
    {
        // Arrange
        using var list = new PooledList<int> { 10, 20, 30 };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list[index]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)] // Equal to Count
    public void Indexer_Set_WithInvalidIndex_ThrowsArgumentOutOfRangeException(int index)
    {
        // Arrange
        using var list = new PooledList<int> { 10, 20, 30 };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list[index] = 99);
    }
}
