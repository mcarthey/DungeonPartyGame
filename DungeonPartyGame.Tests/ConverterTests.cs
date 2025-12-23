using DungeonPartyGame.UI.Converters;
using Xunit;

namespace DungeonPartyGame.Tests;

public class ConverterTests
{
    [Fact]
    public void IsNotNullConverter_ReturnsTrue_ForNonNullValue()
    {
        // Arrange
        var converter = new IsNotNullConverter();
        var testObject = new object();

        // Act
        var result = converter.Convert(testObject, typeof(bool), null, null);

        // Assert
        Assert.True((bool)result);
    }

    [Fact]
    public void IsNotNullConverter_ReturnsFalse_ForNullValue()
    {
        // Arrange
        var converter = new IsNotNullConverter();

        // Act
        var result = converter.Convert(null, typeof(bool), null, null);

        // Assert
        Assert.False((bool)result);
    }

    [Fact]
    public void IsNotNullConverter_ConvertBack_ThrowsNotImplementedException()
    {
        // Arrange
        var converter = new IsNotNullConverter();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack(true, typeof(object), null, null));
    }
}