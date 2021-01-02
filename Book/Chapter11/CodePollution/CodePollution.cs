using Book.Chapter11.LeakingKnowledge;
using Xunit;

namespace Book.Chapter11.CodePollution
{
    public class Logger
    {
        private readonly bool _isTestEnvironment;

        public Logger(bool isTestEnvironment)
        {
            _isTestEnvironment = isTestEnvironment;
        }

        public void Log(string text)
        {
            if (_isTestEnvironment)
                return;

            /* Log the text */
        }
    }

    public class Controller
    {
        public void SomeMethod(Logger logger)
        {
            logger.Log("SomeMethod is called");
        }
    }

    public class Tests
    {
        [Fact]
        public void Some_test()
        {
            var logger = new Logger(true);
            var sut = new Controller();

            sut.SomeMethod(logger);

            /* assert */
        }
    }

    public interface ILogger
    {
        void Log(string text);
    }

    public class Logger2 : ILogger
    {
        public void Log(string text)
        {
            /* Log the text */
        }
    }

    public class FakeLogger : ILogger
    {
        public void Log(string text)
        {
            /* Do nothing */
        }
    }

    public class Controller2
    {
        public void SomeMethod(ILogger logger)
        {
            logger.Log("SomeMethod is called");
        }
    }
}
