using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UserService.AOP;
using UserService.Events;
using UserService.Exceptions;
using UserService.TCP;

namespace UserService.Services
{
    [Serializable]
    internal class UserServiceSlave : MarshalByRefObject, IUserServiceSlave
    {
        private ClientTcp tcpClient;
        private static readonly ReaderWriterLock Rwl = new ReaderWriterLock();
        private static readonly int time = 10000;

        [LogMethods]
        public void ConnectToTcpClient(ClientTcp client)
        {
            tcpClient = client;
            RegisterAddUser();
            RegisterDeleteUser(); 
        }

        private readonly List<User> userList = new List<User>();

        [LogMethods]
        private void AddUser(object sender, UserEventArgs eventArgs)
        {
            Rwl.AcquireWriterLock(time);
            foreach (var user in eventArgs.Users)
            {
                userList.Add(user);
            }     
            Rwl.ReleaseWriterLock();
        }

        [LogMethods]
        private void DeleteUser(object sender, UserEventArgs eventArgs)
        {
            Rwl.AcquireWriterLock(time);
            foreach (var user in eventArgs.Users)
            {
                userList.RemoveAll(u => u.Equals(user));
            }
            Rwl.ReleaseWriterLock();
        }

        [LogMethods]
        public void RegisterAddUser() => tcpClient.AddUser += AddUser;

        [LogMethods]
        public void RegisterDeleteUser() => tcpClient.DeleteUser += DeleteUser;

        [LogMethods]
        public void UnregisterAddUser() => tcpClient.AddUser -= AddUser;

        [LogMethods]
        public void UnregisterDeleteUser() => tcpClient.DeleteUser -= DeleteUser;

        /// <summary>
        /// Searching elements by predicate
        /// </summary>
        /// <param name="searchPredicate">Func that checks on each element</param>
        /// <returns>Sequence off elements</returns>
        [LogMethods]
        public IEnumerable<User> Search(Func<User, bool> searchPredicate)
        { 
            if (searchPredicate == null)
                throw new ArgumentNullException();
            Rwl.AcquireReaderLock(time);
            var templist = userList.Where(searchPredicate).ToList();
            Rwl.ReleaseWriterLock();
            return templist;

        }
    }
}
