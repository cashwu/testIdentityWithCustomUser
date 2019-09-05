using System.Collections.Generic;
using System.Linq;

namespace testIdentity.Models
{
    public class UserRepo : IUserRepo
    {
        private readonly List<User> _users = new List<User>
        {
            new User
            {
                Id = "1",
                UserName = "cash",
                Password = "cash",
                SystemCode = "AB001"
            },
            new User
            {
                Id = "2",
                UserName = "cash2",
                Password = "cash2",
                SystemCode = "AB002"
            }
        };

        public bool ValidateCredentials(string username, string password)
        {
            var user = FindByUsername(username);

            if (user == null)
            {
                return false;
            }

            return user.Password.Equals(password);
        }

        public User FindById(string id)
        {
            return _users.FirstOrDefault(a => a.Id == id);
        }

        public User FindByUsername(string username)
        {
            return _users.FirstOrDefault(a => a.UserName.ToLower() == username.ToLower());
        }
    }
}