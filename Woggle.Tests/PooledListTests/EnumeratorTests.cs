namespace Woggle.Tests.PooledListTests;

public class EnumeratorTests
{
    [Fact]
    public void GetEnumerator_IteratesThroughAllItemsInOrder()
    {
        // Arrange
        using var list = new PooledList<int> { 10, 20, 30 };
        var results = new List<int>();

        // Act
        foreach (var item in list)
        {
            results.Add(item);
        }

        // Assert
        Assert.Equal(new[] { 10, 20, 30 }, results);
    }

    [Fact]
    public void GetEnumerator_OnEmptyList_CompletesWithoutError()
    {
        // Arrange
        using var list = new PooledList<int>();

        // Act
        foreach (var _ in list)
        {
            // This block should not be entered
            Assert.Fail("Enumerator should not yield any items for an empty list.");
        }

        // Assert - success if loop completes without error
    }
}
