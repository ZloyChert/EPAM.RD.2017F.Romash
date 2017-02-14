using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace UserService
{
    public interface ICounterId
    {
        /// <summary>
        /// Method that gives algorithm of counting id of user
        /// </summary>
        /// <param name="user">User to set id</param>
        void CountId(User user);
    }
}
