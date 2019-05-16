using System;
using Xunit;

namespace Namotion.Reflection.Tests
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void When_calling_has_property_then_result_is_correct()
        {
            //// Arrage
            var uri = new Uri("https://rsuter.com");

            //// Act & Assert
            Assert.True(uri.HasProperty(nameof(Uri.Host)));
            Assert.False(uri.HasProperty("Foo"));
        }

        [Fact]
        public void When_setting_a_property_then_it_is_set()
        {
            //// Arrange
            var uri = new Uri("https://rsuter.com");

            //// Act
            var host = uri.TryGetPropertyValue<string>(nameof(Uri.Host), null);

            //// Assert
            Assert.Equal(uri.Host, host);
        }
    }
}
