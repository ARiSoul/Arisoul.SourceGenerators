using Arisoul.SourceGenerators.Extensions;

namespace Arisoul.SourceGenerators.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Fact]
        public void AppendTabsWithZeroShouldReturnStringEmpty()
        {
            // Arrange
            string expected = string.Empty;

            // Act
            string result = expected.AppendTabs(0);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void AppendTabsShouldReturnExpected()
        {
            // Arrange
            string initial = string.Empty;
            string expected = "\t\t";

            // Act
            string result = initial.AppendTabs(2);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CapitalizeWithNullStringShouldReturnNull()
        {
            // Arrange
            string? initial = null;

            // Act
            string? result = initial.Capitalize();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void CapitalizeWithLengthOneShouldReturnExpected()
        {
            // Arrange
            string initial = "s";
            string expected = "S";

            // Act
            string? result = initial.Capitalize();

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
