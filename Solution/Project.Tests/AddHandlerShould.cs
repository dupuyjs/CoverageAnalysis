using NUnit.Framework;
using Project.Add;

namespace Project.Tests
{
    public class AddHandlerShould
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ResultIsEqualTo42()
        {
            var handler = new AddHandler();
            var result = handler.Run(30, 12);

            Assert.AreEqual(result, 42);
        }
    }
}