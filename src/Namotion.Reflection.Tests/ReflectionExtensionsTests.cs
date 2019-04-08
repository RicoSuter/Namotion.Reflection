using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Namotion.Reflection.Tests
{
    public class ReflectionExtensionsTests
    {
        public class MyType : IEquatable<MyType>, IEnumerable<int>
        {
            public bool Equals(MyType other)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<int> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void When_type_implements_equatable_then_item_type_can_be_retrieved()
        {
            //// Act
            var itemType = typeof(MyType).GetEnumerableItemType();

            //// Assert
            Assert.Equal(typeof(int), itemType);
        }
    }
}