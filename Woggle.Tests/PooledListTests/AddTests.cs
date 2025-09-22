namespace Woggle.Tests.PooledListTests;

public class AddTests
{
    [Fact]
    public void Add_SingleItem_IncrementsCountAndAddsItem()
    {
        // Arrange
        using var list = new PooledList<string>();

        // Act
        list.Add("hello");

        // Assert
        Assert.Single(list);
        Assert.Equal("hello", list[0]);
    }

    [Fact]
    public void Add_BeyondInitialCapacity_ResizesAndSucceeds()
    {
        // Arrange
        // Default capacity is 8
        using var list = new PooledList<int>();
        for (int i = 0; i < 8; i++)
        {
            list.Add(i);
        }

        // Act
        // This should trigger a resize
        list.Add(8);

        // Assert
        Assert.Equal(9, list.Count);
        Assert.Equal(8, list[8]);
        // Verify old items are still there
        Assert.Equal(0, list[0]);
        Assert.Equal(7, list[7]);
    }

    [Fact]
    public void Add_Span_AddsAllItemsAndIncrementsCount()
    {
        // Arrange
        using var list = new PooledList<int>
            {
                100
            };
        var itemsToAdd = new[] { 200, 300, 400 };

        // Act
        list.Add(itemsToAdd.AsSpan());

        // Assert
        Assert.Equal(4, list.Count);
        Assert.Equal(100, list[0]);
        Assert.Equal(200, list[1]);
        Assert.Equal(300, list[2]);
        Assert.Equal(400, list[3]);
    }
}
