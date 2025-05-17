using System.Collections.Generic;
using BlockArena.Common;
using FluentAssertions;
using Xunit;

namespace BlockArena.Tests
{
    public class ExtensionsTests
    {
        [Fact]
        public void ConcatShouldAddAValue()
        {
            new List<int> { 1, 2 }.Concat(3).Should().BeEquivalentTo(new List<int> { 1, 2, 3 });
        }

        [Fact]
        public void ConcatShouldCreateNewArrayWithAddedValueWhenArrayIsNull()
        {
            (null as List<int>).Concat(1).Should().BeEquivalentTo(new List<int> { 1 });
        }
    }
}
