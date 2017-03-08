using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.TCP
{
    [Serializable]
    public class MessageUsers
    {
        public List<User> ListUsers { get; }
        public string Mess { get; }

        public MessageUsers(List<User> listUsers, string mess)
        {
            ListUsers = listUsers;
            Mess = mess;
        }
    }
}
