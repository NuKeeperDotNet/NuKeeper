using System;
using System.Collections.Generic;
using System.Text;

namespace NuKeeper.Abstract
{
    public class User
    {
        public User(string login, string name, string email)
        {
            Login = login;
            Name = name;
            Email = email;
        }

        public string Login { get; }
        public string Name { get; }
        public string Email { get; }
    }
}
