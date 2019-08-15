using Moq;

namespace Dafda.Tests.TestDoubles
{
    public static class Dummy
    {
        public static T Of<T>() where T : class
        {
            return new Mock<T>().Object;
        }
    }
}