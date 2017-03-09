using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UserService.Events;
using UserService.Exceptions;
using UserService.TCP;

namespace UserService.Services
{
    [Serializable]
    internal class UserServiceSlave : MarshalByRefObject, IUserService
    {
        private readonly ClientTcp tcpClient;
        public UserServiceSlave(ClientTcp client)
        {
            this.tcpClient = client;
            RegisterAddUser();
            RegisterDeleteUser(); 
        }

        private readonly List<User> userList = new List<User>();

        private void AddUser(object sender, UserEventArgs eventArgs)
        {
            foreach (var user in eventArgs.Users)
            {
                userList.Add(user);
            }     
        }

        private void DeleteUser(object sender, UserEventArgs eventArgs)
        {
            foreach (var user in eventArgs.Users)
            {
                userList.RemoveAll(u => u.Equals(user));
            }
        } 
        
        public void RegisterAddUser() => tcpClient.AddUser += AddUser;
        
        public void RegisterDeleteUser() => tcpClient.DeleteUser += DeleteUser;
        
        public void UnregisterAddUser() => tcpClient.AddUser -= AddUser;
        
        public void UnregisterDeleteUser() => tcpClient.DeleteUser -= DeleteUser;
        
        /// <summary>
        /// Searching elements by predicate
        /// </summary>
        /// <param name="searchPredicate">Func that checks on each element</param>
        /// <returns>Sequence off elements</returns>
        public IEnumerable<User> Search(Func<User, bool> searchPredicate)
        {
            if (searchPredicate == null)
                throw new ArgumentNullException();
            return userList.Where(searchPredicate).ToList();
        }
    }
}
