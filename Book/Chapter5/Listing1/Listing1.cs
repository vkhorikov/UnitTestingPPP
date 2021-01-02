using Moq;
using Xunit;

namespace Book.Chapter5.Listing1
{
    public class ControllerTests
    {
        [Fact]
        public void Sending_a_greetings_email()
        {
            var emailGatewayMock = new Mock<IEmailGateway>();
            var sut = new Controller(emailGatewayMock.Object);

            sut.GreetUser("user@email.com");

            emailGatewayMock.Verify(
                x => x.SendGreetingsEmail("user@email.com"),
                Times.Once);
        }

        [Fact]
        public void Creating_a_report()
        {
            var stub = new Mock<IDatabase>();
            stub.Setup(x => x.GetNumberOfUsers()).Returns(10);
            var sut = new Controller(stub.Object);

            Report report = sut.CreateReport();

            Assert.Equal(10, report.NumberOfUsers);
        }
    }

    public class Controller
    {
        private readonly IEmailGateway _emailGateway;
        private readonly IDatabase _database;

        public Controller(IEmailGateway emailGateway)
        {
            _emailGateway = emailGateway;
        }

        public Controller(IDatabase database)
        {
            _database = database;
        }

        public void GreetUser(string userEmail)
        {
            _emailGateway.SendGreetingsEmail(userEmail);
        }

        public Report CreateReport()
        {
            int numberOfUsers = _database.GetNumberOfUsers();
            return new Report(numberOfUsers);
        }
    }

    public class Report
    {
        public int NumberOfUsers { get; }

        public Report(int numberOfUsers)
        {
            NumberOfUsers = numberOfUsers;
        }
    }

    public interface IDatabase
    {
        int GetNumberOfUsers();
    }

    public interface IEmailGateway
    {
        void SendGreetingsEmail(string userEmail);
    }
}
