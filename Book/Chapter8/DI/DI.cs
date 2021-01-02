using System;
using System.Collections.Generic;

namespace Book.Chapter8.DI
{
    public class User
    {
        private static readonly ILogger _logger =
            LogManager.GetLogger(typeof(User));

        private IDomainLogger _domainLogger;
        public int UserId { get; set; }
        public string Email { get; private set; }
        public UserType Type { get; private set; }
        public bool IsEmailConfirmed { get; }
        public List<EmailChangedEvent> DomainEvents { get; }

        public User(int userId, string email, UserType type, bool isEmailConfirmed)
        {
            UserId = userId;
            Email = email;
            Type = type;
            IsEmailConfirmed = isEmailConfirmed;
            DomainEvents = new List<EmailChangedEvent>();
        }

        public string CanChangeEmail()
        {
            if (IsEmailConfirmed)
                return "Can't change email after it's confirmed";

            return null;
        }

        public void ChangeEmail(
            string newEmail, Company company, ILogger logger)
        {
            logger.Info(
                $"Changing email for user {UserId} to {newEmail}");

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
                AddDomainEvent(
                    new UserTypeChangedEvent(UserId, Type, newType));
            }

            Email = newEmail;
            Type = newType;
            AddDomainEvent(new EmailChangedEvent(UserId, newEmail));

            logger.Info($"Email is changed for user {UserId}");
        }

        private void AddDomainEvent(EmailChangedEvent userTypeChangedEvent)
        {
        }

        private void AddDomainEvent(UserTypeChangedEvent userTypeChangedEvent)
        {
            
        }
    }

    internal class LogManager
    {
        public static ILogger GetLogger(Type type)
        {
            throw new NotImplementedException();
        }
    }

    public class UserController
    {
        private readonly Database _database = new Database();
        private readonly MessageBus _messageBus = new MessageBus();

        public string ChangeEmail(int userId, string newEmail)
        {
            object[] userData = _database.GetUserById(userId);
            User user = UserFactory.Create(userData);

            string error = user.CanChangeEmail();
            if (error != null)
                return error;

            object[] companyData = _database.GetCompany();
            Company company = CompanyFactory.Create(companyData);

            user.ChangeEmail(newEmail, company, null);

            _database.SaveCompany(company);
            _database.SaveUser(user);
            EventDispatcher.Dispatch(user.DomainEvents);

            return "OK";
        }
    }

    public class EventDispatcher
    {
        public static void Dispatch(List<EmailChangedEvent> userDomainEvents)
        {
            //foreach (EmailChangedEvent ev in user.DomainEvents)
            //{
            //    _messageBus.SendEmailChangedMessage(ev.UserId, ev.NewEmail);
            //}
        }
    }

    public class UserFactory
    {
        public static User Create(object[] data)
        {
            return null;
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

    internal interface IDomainLogger
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

    public class UserTypeChangedEvent
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

    public class EmailChangedEvent
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
