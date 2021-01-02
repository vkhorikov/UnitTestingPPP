using Xunit;

namespace Book.Chapter5.Listing5
{
    public class User
    {
        public string Name { get; set; }

        public string NormalizeName(string name)
        {
            string result = (name ?? "").Trim();

            if (result.Length > 50)
                return result.Substring(0, 50);

            return result;
        }
    }

    public class UserController
    {
        public void RenameUser(int userId, string newName)
        {
            User user = GetUserFromDatabase(userId);

            string normalizedName = user.NormalizeName(newName);
            user.Name = normalizedName;

            SaveUserToDatabase(user);
        }

        private void SaveUserToDatabase(User user)
        {
        }

        private User GetUserFromDatabase(int userId)
        {
            return new User();
        }
    }
}
