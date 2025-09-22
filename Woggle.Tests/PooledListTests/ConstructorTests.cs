namespace Woggle.Tests.PooledListTests;

public class ConstructorTests
{
    [Fact]
    public void DefaultConstructor_CreatesEmptyList()
    {
        // Arrange & Act
        using var list = new PooledList<int>();

        // Assert
        Assert.Empty(list);
        Assert.False(list.IsReadOnly);
    }

    [Fact]
    public void CapacityConstructor_CreatesEmptyList()
    {
        // Arrange & Act
        using var list = new PooledList<int>(16);

        // Assert
        Assert.Empty(list);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CapacityConstructor_WithInvalidCapacity_ThrowsArgumentOutOfRangeException(int invalidCapacity)
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new PooledList<int>(invalidCapacity));
    }
}
