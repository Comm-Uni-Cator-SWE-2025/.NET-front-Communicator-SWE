/*
 * Unit tests for ClientNode record
 * Target: 100% line and branch coverage
 */
using System;
using Communicator.App.ViewModels;
using Xunit;

namespace Communicator.App.Tests.Unit.ViewModels;

public sealed class ClientNodeTests
{
    [Fact]
    public void ConstructorSetsHostNameAndPort()
    {
        ClientNode node = new ClientNode("192.168.1.1", 8080);

        Assert.Equal("192.168.1.1", node.HostName);
        Assert.Equal(8080, node.Port);
    }

    [Fact]
    public void GetHashCodeReturnsSameValueForSameHostNameAndPort()
    {
        ClientNode node1 = new ClientNode("localhost", 3000);
        ClientNode node2 = new ClientNode("localhost", 3000);

        Assert.Equal(node1.GetHashCode(), node2.GetHashCode());
    }

    [Fact]
    public void GetHashCodeReturnsDifferentValueForDifferentHostNames()
    {
        ClientNode node1 = new ClientNode("localhost", 3000);
        ClientNode node2 = new ClientNode("127.0.0.1", 3000);

        Assert.NotEqual(node1.GetHashCode(), node2.GetHashCode());
    }

    [Fact]
    public void GetHashCodeReturnsDifferentValueForDifferentPorts()
    {
        ClientNode node1 = new ClientNode("localhost", 3000);
        ClientNode node2 = new ClientNode("localhost", 4000);

        Assert.NotEqual(node1.GetHashCode(), node2.GetHashCode());
    }

    [Fact]
    public void EqualsReturnsTrueForSameValues()
    {
        ClientNode node1 = new ClientNode("192.168.1.1", 8080);
        ClientNode node2 = new ClientNode("192.168.1.1", 8080);

        Assert.Equal(node1, node2);
    }

    [Fact]
    public void EqualsReturnsFalseForDifferentValues()
    {
        ClientNode node1 = new ClientNode("192.168.1.1", 8080);
        ClientNode node2 = new ClientNode("192.168.1.2", 8080);

        Assert.NotEqual(node1, node2);
    }

    [Fact]
    public void RecordDeconstructionWorks()
    {
        ClientNode node = new ClientNode("myhost", 9000);

        (string hostName, int port) = node;

        Assert.Equal("myhost", hostName);
        Assert.Equal(9000, port);
    }

    [Fact]
    public void WithExpressionCreatesNewInstanceWithModifiedValue()
    {
        ClientNode original = new ClientNode("original", 1000);
        ClientNode modified = original with { Port = 2000 };

        Assert.Equal("original", modified.HostName);
        Assert.Equal(2000, modified.Port);
        Assert.NotSame(original, modified);
    }

    [Fact]
    public void ToStringReturnsExpectedFormat()
    {
        ClientNode node = new ClientNode("testhost", 5000);

        string result = node.ToString();

        Assert.Contains("testhost", result, StringComparison.Ordinal);
        Assert.Contains("5000", result, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptyHostNameIsAllowed()
    {
        ClientNode node = new ClientNode("", 8080);

        Assert.Equal("", node.HostName);
        Assert.Equal(8080, node.Port);
    }

    [Fact]
    public void ZeroPortIsAllowed()
    {
        ClientNode node = new ClientNode("localhost", 0);

        Assert.Equal("localhost", node.HostName);
        Assert.Equal(0, node.Port);
    }

    [Fact]
    public void NegativePortIsAllowed()
    {
        ClientNode node = new ClientNode("localhost", -1);

        Assert.Equal(-1, node.Port);
    }
}
