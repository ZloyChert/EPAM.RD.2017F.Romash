using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Exceptions
{
    public class EmptyUserException : System.Exception
    {
        public EmptyUserException()
        {
        }

        public EmptyUserException(string message)
            : base(message)
        {
        }

        public EmptyUserException(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }
}
