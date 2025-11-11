namespace Communicator.Controller.Tests;


public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Assert.True(1 + 1 == 2);
    }

    [Fact]
    public void Test2()
    {
        Assert.NotNull("test");
    }

    [Fact]
    public void Test3()
    {
        Assert.Equal(4, 2 * 2);
    }

    private static readonly int[] s_collection = new[] { 1, 2, 3 };
    private static readonly int[] s_collection_1 = new[] { 1, 2, 3 };
    private static readonly int[] s_collection_2 = new[] { 1 };

    [Fact]
    public void Test4()
    {
        Assert.NotEmpty(s_collection);
    }

    [Fact]
    public void Test5()
    {
        Assert.True(true);
    }

    [Fact]
    public void Test6()
    {
        Assert.False(false);
    }

    [Fact]
    public void Test7()
    {
        Assert.Contains(3, s_collection_1);
    }

    [Fact]
    public void Test8()
    {
        Assert.Single(s_collection_2);
    }

    [Fact]
    public void Test9()
    {
        Assert.Empty(Array.Empty<int>());
    }

    [Fact]
    public void Test10()
    {
        Assert.IsType<string>("test");
    }
}
