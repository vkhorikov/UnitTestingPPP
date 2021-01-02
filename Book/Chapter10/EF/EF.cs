using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Book.Chapter10.EF
{
    public class CrmContext : DbContext
    {
        public CrmContext(DbContextOptions<CrmContext> options)
            : base(options)
        {
        }

        public CrmContext(string connectionString)
            : base (new DbContextOptionsBuilder<CrmContext>().UseSqlServer(connectionString).Options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(x =>
            {
                x.ToTable("User").HasKey(k => k.UserId);
                x.Property(k => k.Email);
                x.Property(k => k.Type);
                x.Property(k => k.IsEmailConfirmed);
                x.Ignore(k => k.DomainEvents);
            });

            modelBuilder.Entity<Company>(x =>
            {
                x.ToTable("Company").HasKey(k => k.DomainName);
                x.Property(p => p.DomainName);
                x.Property(p => p.NumberOfEmployees);
            });
        }
    }

    public class User
    {
        public int UserId { get; set; }
        public string Email { get; private set; }
        public UserType Type { get; private set; }
        public bool IsEmailConfirmed { get; }
        public List<IDomainEvent> DomainEvents { get; }

        public User(int userId, string email, UserType type, bool isEmailConfirmed)
        {
            UserId = userId;
            Email = email;
            Type = type;
            IsEmailConfirmed = isEmailConfirmed;
            DomainEvents = new List<IDomainEvent>();
        }

        public string CanChangeEmail()
        {
            if (IsEmailConfirmed)
                return "Can't change email after it's confirmed";

            return null;
        }

        public void ChangeEmail(string newEmail, Company company)
        {
            Precondition.Requires(CanChangeEmail() == null);

            if (Email == newEmail)
                return;

            UserType newType = company.IsEmailCorporate(newEmail)
                ? UserType.Employee
                : UserType.Customer;

            if (Type != newType)
            {
                int delta = newType == UserType.Employee ? 1 : -1;
                company.ChangeNumberOfEmployees(delta);
                AddDomainEvent(new UserTypeChangedEvent(UserId, Type, newType));
            }

            Email = newEmail;
            Type = newType;
            AddDomainEvent(new EmailChangedEvent(UserId, newEmail));
        }

        private void AddDomainEvent(IDomainEvent domainEvent)
        {
            DomainEvents.Add(domainEvent);
        }
    }

    public class UserController
    {
        private readonly CrmContext _context;
        private readonly UserRepository _userRepository;
        private readonly CompanyRepository _companyRepository;
        private readonly EventDispatcher _eventDispatcher;

        public UserController(
            CrmContext context,
            MessageBus messageBus,
            IDomainLogger domainLogger)
        {
            _context = context;
            _userRepository = new UserRepository(context);
            _companyRepository = new CompanyRepository(context);
            _eventDispatcher = new EventDispatcher(
                messageBus, domainLogger);
        }

        public string ChangeEmail(int userId, string newEmail)
        {
            User user = _userRepository.GetUserById(userId);

            string error = user.CanChangeEmail();
            if (error != null)
                return error;

            Company company = _companyRepository.GetCompany();

            user.ChangeEmail(newEmail, company);

            _companyRepository.SaveCompany(company);
            _userRepository.SaveUser(user);
            _eventDispatcher.Dispatch(user.DomainEvents);

            _context.SaveChanges();
            return "OK";
        }
    }

    public class EventDispatcher
    {
        private readonly MessageBus _messageBus;
        private readonly IDomainLogger _domainLogger;

        public EventDispatcher(
            MessageBus messageBus,
            IDomainLogger domainLogger)
        {
            _domainLogger = domainLogger;
            _messageBus = messageBus;
        }

        public void Dispatch(List<IDomainEvent> events)
        {
            foreach (IDomainEvent ev in events)
            {
                Dispatch(ev);
            }
        }

        private void Dispatch(IDomainEvent ev)
        {
            switch (ev)
            {
                case EmailChangedEvent emailChangedEvent:
                    _messageBus.SendEmailChangedMessage(
                        emailChangedEvent.UserId,
                        emailChangedEvent.NewEmail);
                    break;

                case UserTypeChangedEvent userTypeChangedEvent:
                    _domainLogger.UserTypeHasChanged(
                        userTypeChangedEvent.UserId,
                        userTypeChangedEvent.OldType,
                        userTypeChangedEvent.NewType);
                    break;
            }
        }
    }

    public class UserFactory
    {
        public static User Create(object[] data)
        {
            Precondition.Requires(data.Length >= 3);

            int id = (int)data[0];
            string email = (string)data[1];
            UserType type = (UserType)data[2];
            bool isEmailConfirmed = (bool)data[3];

            return new User(id, email, type, isEmailConfirmed);
        }
    }

    public class CompanyFactory
    {
        public static Company Create(object[] data)
        {
            Precondition.Requires(data.Length >= 2);

            string domainName = (string)data[0];
            int numberOfEmployees = (int)data[1];

            return new Company(domainName, numberOfEmployees);
        }
    }

    public interface IDomainLogger
    {
        void UserTypeHasChanged(int userId, UserType oldType, UserType newType);
    }

    public class DomainLogger : IDomainLogger
    {
        private readonly ILogger _logger;

        public DomainLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void UserTypeHasChanged(
            int userId, UserType oldType, UserType newType)
        {
            _logger.Info(
                $"User {userId} changed type " +
                $"from {oldType} to {newType}");
        }
    }

    public interface ILogger
    {
        void Info(string s);
    }

    public class UserTypeChangedEvent : IDomainEvent
    {
        public int UserId { get; }
        public UserType OldType { get; }
        public UserType NewType { get; }

        public UserTypeChangedEvent(int userId, UserType oldType, UserType newType)
        {
            UserId = userId;
            OldType = oldType;
            NewType = newType;
        }

        protected bool Equals(UserTypeChangedEvent other)
        {
            return UserId == other.UserId && string.Equals(OldType, other.OldType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((EmailChangedEvent)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (UserId * 397) ^ OldType.GetHashCode();
            }
        }
    }

    public class EmailChangedEvent : IDomainEvent
    {
        public int UserId { get; }
        public string NewEmail { get; }

        public EmailChangedEvent(int userId, string newEmail)
        {
            UserId = userId;
            NewEmail = newEmail;
        }

        protected bool Equals(EmailChangedEvent other)
        {
            return UserId == other.UserId && string.Equals(NewEmail, other.NewEmail);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((EmailChangedEvent)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (UserId * 397) ^ (NewEmail != null ? NewEmail.GetHashCode() : 0);
            }
        }
    }

    public interface IDomainEvent
    {
    }

    public class Company
    {
        public string DomainName { get; }
        public int NumberOfEmployees { get; private set; }

        public Company(string domainName, int numberOfEmployees)
        {
            DomainName = domainName;
            NumberOfEmployees = numberOfEmployees;
        }

        public void ChangeNumberOfEmployees(int delta)
        {
            Precondition.Requires(NumberOfEmployees + delta >= 0);

            NumberOfEmployees += delta;
        }

        public bool IsEmailCorporate(string email)
        {
            string emailDomain = email.Split('@')[1];
            return emailDomain == DomainName;
        }
    }

    public enum UserType
    {
        Customer = 1,
        Employee = 2
    }

    public static class Precondition
    {
        public static void Requires(bool precondition, string message = null)
        {
            if (precondition == false)
                throw new Exception(message);
        }
    }

    public class UserControllerTestsBad
    {
        private const string ConnectionString = @"Server=.\Sql;Database=IntegrationTests;Trusted_Connection=true;";

        [Fact]
        public void Changing_email_from_corporate_to_non_corporate()
        {
            var optionsBuilder = new DbContextOptionsBuilder<CrmContext>()
                .UseSqlServer(ConnectionString);

            using (var context = new CrmContext(optionsBuilder.Options))
            {
                // Arrange
                var userRepository = new UserRepository(context);
                var companyRepository = new CompanyRepository(context);
                var user = new User(0, "user@mycorp.com",
                    UserType.Employee, false);
                userRepository.SaveUser(user);
                var company = new Company("mycorp.com", 1);
                companyRepository.SaveCompany(company);
                context.SaveChanges();

                var busSpy = new BusSpy();
                var messageBus = new MessageBus(busSpy);
                var loggerMock = new Mock<IDomainLogger>();
                var sut = new UserController(
                    context, messageBus, loggerMock.Object);

                // Act
                string result = sut.ChangeEmail(user.UserId, "new@gmail.com");

                // Assert
                Assert.Equal("OK", result);

                User userFromDb = userRepository.GetUserById(user.UserId);
                Assert.Equal("new@gmail.com", userFromDb.Email);
                Assert.Equal(UserType.Customer, userFromDb.Type);

                Company companyFromDb = companyRepository.GetCompany();
                Assert.Equal(0, companyFromDb.NumberOfEmployees);

                busSpy.ShouldSendNumberOfMessages(1)
                    .WithEmailChangedMessage(user.UserId, "new@gmail.com");
                loggerMock.Verify(
                    x => x.UserTypeHasChanged(
                        user.UserId, UserType.Employee, UserType.Customer),
                    Times.Once);
            }
        }
    }

    public class UserControllerTests : IntegrationTests
    {
        [Fact]
        public void Changing_email_from_corporate_to_non_corporate()
        {
            // Arrange
            User user = CreateUser("user@mycorp.com", UserType.Employee);
            CreateCompany("mycorp.com", 1);

            var busSpy = new BusSpy();
            var messageBus = new MessageBus(busSpy);
            var loggerMock = new Mock<IDomainLogger>();

            // Act
            string result = Execute(
                x => x.ChangeEmail(user.UserId, "new@gmail.com"),
                messageBus, loggerMock.Object);

            // Assert
            Assert.Equal("OK", result);

            User userFromDb = QueryUser(user.UserId);
            userFromDb
                .ShouldExist()
                .WithEmail("new@gmail.com")
                .WithType(UserType.Customer);
            Company companyFromDb = QueryCompany();
            Assert.Equal(0, companyFromDb.NumberOfEmployees);

            busSpy.ShouldSendNumberOfMessages(1)
                .WithEmailChangedMessage(user.UserId, "new@gmail.com");
            loggerMock.Verify(
                x => x.UserTypeHasChanged(
                    user.UserId, UserType.Employee, UserType.Customer),
                Times.Once);
        }

        private string Execute(Func<UserController, string> func, MessageBus messageBus, IDomainLogger logger)
        {
            using (var context = new CrmContext(ConnectionString))
            {
                var controller = new UserController(context, messageBus, logger);
                return func(controller);
            }
        }

        private Company QueryCompany()
        {
            using (var context = new CrmContext(ConnectionString))
            {
                var repository = new CompanyRepository(context);
                return repository.GetCompany();
            }
        }

        private User QueryUser(int userId)
        {
            using (var context = new CrmContext(ConnectionString))
            {
                var repository = new UserRepository(context);
                return repository.GetUserById(userId);
            }
        }

        private User CreateUser(
            string email = "user@mycorp.com",
            UserType type = UserType.Employee,
            bool isEmailConfirmed = false)
        {
            using (var context = new CrmContext(ConnectionString))
            {
                var user = new User(0, email, type, isEmailConfirmed);
                var repository = new UserRepository(context);
                repository.SaveUser(user);

                context.SaveChanges();

                return user;
            }
        }

        private Company CreateCompany(string domainName, int numberOfEmployees)
        {
            using (var context = new CrmContext(ConnectionString))
            {
                var company = new Company(domainName, numberOfEmployees);
                var repository = new CompanyRepository(context);
                repository.AddCompany(company);

                context.SaveChanges();

                return company;
            }
        }
    }

    public static class UserExternsions
    {
        public static User ShouldExist(this User user)
        {
            Assert.NotNull(user);
            return user;
        }

        public static User WithEmail(this User user, string email)
        {
            Assert.Equal(email, user.Email);
            return user;
        }

        public static User WithType(this User user, UserType type)
        {
            Assert.Equal(type, user.Type);
            return user;
        }
    }

    public abstract class IntegrationTests
    {
        protected const string ConnectionString = @"Server=.\Sql;Database=IntegrationTests;Trusted_Connection=true;";

        protected IntegrationTests()
        {
            ClearDatabase();
        }

        private void ClearDatabase()
        {
            string query = 
                "DELETE FROM dbo.[User];" +
                "DELETE FROM dbo.Company;";

            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand(query, connection)
                {
                    CommandType = CommandType.Text
                };

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }

    public class UserRepository
    {
        private readonly CrmContext _context;

        public UserRepository(CrmContext context)
        {
            _context = context;
        }

        public User GetUserById(int userId)
        {
            return _context.Users
                .SingleOrDefault(x => x.UserId == userId);
        }

        public void SaveUser(User user)
        {
            _context.Users.Update(user);
        }
    }

    public class CompanyRepository
    {
        private readonly CrmContext _context;

        public CompanyRepository(CrmContext context)
        {
            _context = context;
        }

        public Company GetCompany()
        {
            return _context.Companies
                .SingleOrDefault();
        }

        public void SaveCompany(Company company)
        {
            _context.Companies.Update(company);
        }

        public void AddCompany(Company company)
        {
            _context.Companies.Add(company);
        }
    }

    public class Transaction : IDisposable
    {
        private readonly TransactionScope _transaction;
        public readonly string ConnectionString;

        public Transaction(string connectionString)
        {
            _transaction = new TransactionScope();
            ConnectionString = connectionString;
        }

        public void Commit()
        {
            _transaction.Complete();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
        }
    }

    public class MessageBus
    {
        private readonly IBus _bus;

        public MessageBus(IBus bus)
        {
            _bus = bus;
        }

        public void SendEmailChangedMessage(int userId, string newEmail)
        {
            _bus.Send("Type: USER EMAIL CHANGED; " +
                $"Id: {userId}; " +
                $"NewEmail: {newEmail}");
        }
    }

    public interface IBus
    {
        void Send(string message);
    }

    public class BusSpy : IBus
    {
        private List<string> _sentMessages = new List<string>();

        public void Send(string message)
        {
            _sentMessages.Add(message);
        }

        public BusSpy ShouldSendNumberOfMessages(int number)
        {
            Assert.Equal(number, _sentMessages.Count);
            return this;
        }

        public BusSpy WithEmailChangedMessage(int userId, string newEmail)
        {
            string message = "Type: USER EMAIL CHANGED; " +
                $"Id: {userId}; " +
                $"NewEmail: {newEmail}";
            Assert.Contains(_sentMessages, x => x == message);

            return this;
        }
    }
}
