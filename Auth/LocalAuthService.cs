using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ButchersCashier.Auth
{
    public class LocalAuthService
    {
        private readonly string _usersFile; // Path to the user file
        private readonly List<User> _users;
        private string _currentUsername; // To track the logged-in user

        public LocalAuthService()
        {
            var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ButchersCashier", "UserData");
            Directory.CreateDirectory(directoryPath);
            _usersFile = Path.Combine(directoryPath, "users.txt");
            _users = LoadUsers();
        }

        private class User
        {
            public string Username { get; set; }
            public string PasswordHash { get; set; }
        }

        public async Task RegisterAsync(string username, string password)
        {
            if (_users.Exists(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("User already exists.");
            }

            var passwordHash = HashPassword(password);
            _users.Add(new User { Username = username, PasswordHash = passwordHash });
            await File.WriteAllTextAsync(_usersFile, SerializeUsers(_users));
        }

        private List<User> LoadUsers()
        {
            if (!File.Exists(_usersFile))
            {
                return new List<User>();
            }

            var fileContent = File.ReadAllText(_usersFile);
            return DeserializeUsers(fileContent);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private string SerializeUsers(List<User> users)
        {
            var sb = new StringBuilder();
            foreach (var user in users)
            {
                sb.AppendLine($"{user.Username},{user.PasswordHash}");
            }
            return sb.ToString();
        }

        private List<User> DeserializeUsers(string data)
        {
            var users = new List<User>();
            var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length == 2)
                {
                    users.Add(new User { Username = parts[0], PasswordHash = parts[1] });
                }
            }
            return users;
        }
        public void SetCurrentUser(string username)
        {
            _currentUsername = username; // Set the current user
        }
        public async Task<bool> LoginAsync(string username, string password)
        {
            var passwordHash = HashPassword(password);

            Console.WriteLine($"Attempting login for: {username}"); // Log the username being attempted

            // Check if the user exists and the password matches
            var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.PasswordHash == passwordHash);

            if (user != null)
            {
                _currentUsername = username; // Set the current user
                Console.WriteLine($"Logged in user: {_currentUsername}"); // Log the username
                return true;
            }

            Console.WriteLine("Login failed: user not found or password incorrect."); // Log failure
            return false;
        }

        public void Logout()
        {
            _currentUsername = null; // Clear the current user on logout
        }

        public string GetCurrentUsername()
        {
            return _currentUsername; // Return the current username
        }
    }
}
