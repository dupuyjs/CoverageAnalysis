using NUnit.Framework;
using Project.Substract;

namespace Project.Tests
{
    public class SubstractHandlerShould
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ResultIsEqualTo42()
        {
            var handler = new SubstractHandler();
            var result = handler.Run(52, 10);

            Assert.AreEqual(result, 42);
        }
    }
}
