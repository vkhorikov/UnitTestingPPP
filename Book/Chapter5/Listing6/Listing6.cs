namespace Book.Chapter5.Listing6
{
    public class User
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => _name = NormalizeName(value);
        }

        private string NormalizeName(string name)
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
            user.Name = newName;
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
