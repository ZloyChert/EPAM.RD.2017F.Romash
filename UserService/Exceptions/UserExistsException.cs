using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Exceptions
{
    public class UserExistsException : System.Exception
    {
        public UserExistsException()
        {
        }

        public UserExistsException(string message)
            : base(message)
        {
        }

        public UserExistsException(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }
}
