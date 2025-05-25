using BlockArena.Common;
using FluentAssertions;

namespace BlockArena.UnitTests
{
    [TestFixture]
    public class ExtensionsTests
    {
        [Test]
        public void ConcatShouldAddAValue()
        {
            new List<int> { 22, 33 }.Concat(55).Should().BeEquivalentTo(new List<int> { 22, 33, 55 });
        }

        [Test]
        public void ConcatShouldCreateNewArrayWithAddedValueWhenArrayIsNull()
        {
            (null as List<int>).Concat(178).Should().BeEquivalentTo(new List<int> { 178 });
        }
    }
}