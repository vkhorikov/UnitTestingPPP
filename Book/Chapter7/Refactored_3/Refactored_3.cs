using System;
using Xunit;

namespace Book.Chapter7.Refactored_3
{
    public class User
    {
        public int UserId { get; private set; }
        public string Email { get; private set; }
        public UserType Type { get; private set; }

        public User(int userId, string email, UserType type)
        {
            UserId = userId;
            Email = email;
            Type = type;
        }

        public void ChangeEmail(string newEmail, Company company)
        {
            if (Email == newEmail)
                return;

            UserType newType = company.IsEmailCorporate(newEmail)
                ? UserType.Employee
                : UserType.Customer;

            if (Type != newType)
            {
                int delta = newType == UserType.Employee ? 1 : -1;
                company.ChangeNumberOfEmployees(delta);
            }

            Email = newEmail;
            Type = newType;
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

            return new User(id, email, type);
        }
    }

    public class UserController
    {
        private readonly Database _database = new Database();
        private readonly MessageBus _messageBus = new MessageBus();

        public void ChangeEmail(int userId, string newEmail)
        {
            object[] userData = _database.GetUserById(userId);
            User user = UserFactory.Create(userData);

            object[] companyData = _database.GetCompany();
            Company company = CompanyFactory.Create(companyData);

            user.ChangeEmail(newEmail, company);

            _database.SaveCompany(company);
            _database.SaveUser(user);
            _messageBus.SendEmailChangedMessage(userId, newEmail);
        }
    }

    public class Company
    {
        public string DomainName { get; private set; }
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

    public enum UserType
    {
        Customer = 1,
        Employee = 2
    }

    public class Tests
    {
        [Fact]
        public void Changing_email_without_changing_user_type()
        {
            var company = new Company("mycorp.com", 1);
            var sut = new User(1, "user@mycorp.com", UserType.Employee);

            sut.ChangeEmail("new@mycorp.com", company);

            Assert.Equal(1, company.NumberOfEmployees);
            Assert.Equal("new@mycorp.com", sut.Email);
            Assert.Equal(UserType.Employee, sut.Type);
        }

        [Fact]
        public void Changing_email_from_corporate_to_non_corporate()
        {
            var company = new Company("mycorp.com", 1);
            var sut = new User(1, "user@mycorp.com", UserType.Employee);

            sut.ChangeEmail("new@gmail.com", company);

            Assert.Equal(0, company.NumberOfEmployees);
            Assert.Equal("new@gmail.com", sut.Email);
            Assert.Equal(UserType.Customer, sut.Type);
        }

        [Fact]
        public void Changing_email_from_non_corporate_to_corporate()
        {
            var company = new Company("mycorp.com", 1);
            var sut = new User(1, "user@gmail.com", UserType.Customer);

            sut.ChangeEmail("new@mycorp.com", company);

            Assert.Equal(2, company.NumberOfEmployees);
            Assert.Equal("new@mycorp.com", sut.Email);
            Assert.Equal(UserType.Employee, sut.Type);
        }

        [Fact]
        public void Changing_email_to_the_same_one()
        {
            var company = new Company("mycorp.com", 1);
            var sut = new User(1, "user@gmail.com", UserType.Customer);

            sut.ChangeEmail("user@gmail.com", company);

            Assert.Equal(1, company.NumberOfEmployees);
            Assert.Equal("user@gmail.com", sut.Email);
            Assert.Equal(UserType.Customer, sut.Type);
        }

        [InlineData("mycorp.com", "email@mycorp.com", true)]
        [InlineData("mycorp.com", "email@gmail.com", false)]
        [Theory]
        public void Differentiates_a_corporate_email_from_non_corporate(
            string domain, string email, bool expectedResult)
        {
            var sut = new Company(domain, 0);

            bool isEmailCorporate = sut.IsEmailCorporate(email);

            Assert.Equal(expectedResult, isEmailCorporate);
        }
    }

    public static class Precondition
    {
        public static void Requires(bool precondition, string message = null)
        {
            if (precondition == false)
                throw new Exception(message);
        }
    }

    public class Database
    {
        public object[] GetUserById(int userId)
        {
            return null;
        }

        public User GetUserByEmail(string email)
        {
            return null;
        }

        public void SaveUser(User user)
        {
        }

        public object[] GetCompany()
        {
            return null;
        }

        public void SaveCompany(Company company)
        {
        }
    }

    public class MessageBus
    {
        private IBus _bus;

        public void SendEmailChangedMessage(int userId, string newEmail)
        {
            _bus.Send($"Subject: USER; Type: EMAIL CHANGED; Id: {userId}; NewEmail: {newEmail}");
        }
    }

    internal interface IBus
    {
        void Send(string message);
    }
}
