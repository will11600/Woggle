namespace Woggle.Tests.PooledListTests;

public class DisposeTests
{
    [Fact]
    public void AllPublicMembers_ThrowAfterDispose()
    {
        // Arrange
        var list = new PooledList<int> { 1, 2 };
        list.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => list.Add(3));
        Assert.Throws<ObjectDisposedException>(() => list.Add([4]));
        Assert.Throws<ObjectDisposedException>(() => list.Clear());
        Assert.Throws<ObjectDisposedException>(() => list.Contains(1));
        Assert.Throws<ObjectDisposedException>(() => list.CopyTo(new int[2], 0));
        Assert.Throws<ObjectDisposedException>(() => list.IndexOf(1));
        Assert.Throws<ObjectDisposedException>(() => list.Insert(0, 0));
        Assert.Throws<ObjectDisposedException>(() => list.Remove(1));
        Assert.Throws<ObjectDisposedException>(() => list.RemoveAt(0));
        Assert.Throws<ObjectDisposedException>(() => list[0] = 5);
        Assert.Throws<ObjectDisposedException>(() => _ = list[0]);
        Assert.Throws<ObjectDisposedException>(() => {
            foreach (var _ in list) { }
        });
        // Properties don't throw, they are just fields.
        // But we can check that Count doesn't change after dispose actions.
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes_WithoutThrowing()
    {
        // Arrange
        var list = new PooledList<int>();

        // Act
        list.Dispose();
        var exception = Record.Exception(() => list.Dispose());

        // Assert
        Assert.Null(exception);
    }
}