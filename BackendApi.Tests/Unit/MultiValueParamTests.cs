using Poq.BackendApi.Models;

namespace Poq.BackendApi.Tests.Unit;

public class MultiValueParamTests
{
    [Fact]
    public void Parse_NoSource_DoesNotClear()
    {
        // Arrange
        var list = new List<string> { "1", "2", "3" };
        IMultiValueParam service = new MultiValueParam(list);

        // Act
        service.Parse(string.Empty, ',');

        // Assert
        Assert.Equal(list.Count, service.Count);
        Assert.False(service.Contains("4"));
        list.ForEach(
            expectedValue => Assert.True(service.Contains(expectedValue))
        );
    }

    [Fact]
    public void Parse_NoSeparator_ReturnsUnparsedSource()
    {
        // Arrange
        string source = "1 2 3";
        IEnumerable<string> emptyList = Enumerable.Empty<string>();
        IMultiValueParam service = new MultiValueParam(emptyList);

        // Act
        service.Parse(source, ',');

        // Assert
        Assert.Equal(1, service.Count);
        Assert.True(service.Contains(source));
    }

    [Fact]
    public void Parse_ValidSourceAndSeparator_Success()
    {
        // Arrange
        IEnumerable<string> emptyList = Enumerable.Empty<string>();
        IMultiValueParam service = new MultiValueParam(emptyList);

        // Act
        service.Parse("1,2,3", ',');

        // Assert
        Assert.Equal(3, service.Count);
        Assert.True(service.Contains("1"));
        Assert.True(service.Contains("2"));
        Assert.True(service.Contains("3"));
    }
}
