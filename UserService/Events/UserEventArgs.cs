using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Events
{
    [Serializable]
    public class UserEventArgs
    {
        public UserEventArgs(User user)
        {
            Users = new List<User> {user};
        }
        public UserEventArgs(IEnumerable<User> users)
        {
            Users = new List<User>(users);
        }

        public List<User> Users { get; }
    }
}
