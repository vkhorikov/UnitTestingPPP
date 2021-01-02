using System;

namespace Book.Chapter7.Refactored_2
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

        public int ChangeEmail(string newEmail,
            string companyDomainName, int numberOfEmployees)
        {
            if (Email == newEmail)
                return numberOfEmployees;

            string emailDomain = newEmail.Split('@')[1];
            bool isEmailCorporate = emailDomain == companyDomainName;
            UserType newType = isEmailCorporate
                ? UserType.Employee
                : UserType.Customer;

            if (Type != newType)
            {
                int delta = newType == UserType.Employee ? 1 : -1;
                int newNumber = numberOfEmployees + delta;
                numberOfEmployees = newNumber;
            }

            Email = newEmail;
            Type = newType;

            return numberOfEmployees;
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
            string companyDomainName = (string)companyData[0];
            int numberOfEmployees = (int)companyData[1];

            int newNumberOfEmployees = user.ChangeEmail(
                newEmail, companyDomainName, numberOfEmployees);

            _database.SaveCompany(newNumberOfEmployees);
            _database.SaveUser(user);
            _messageBus.SendEmailChangedMessage(userId, newEmail);
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

        public void SaveCompany(int newNumber)
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
