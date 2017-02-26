using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UserService.Events;
using UserService.Exceptions;

namespace UserService.Services
{
    // осталось рефлекией
    [Serializable]
    internal class UserServiceSlave : MarshalByRefObject, IUserService
    {
        private readonly IUserServiceMaster serviceMaster;
        public UserServiceSlave(IUserServiceMaster serviceMaster)
        {
            this.serviceMaster = serviceMaster;
            RegisterAddUser();
            RegisterDeleteUser();
            RegisterOnCreating();  
        }

        private readonly List<User> userList = new List<User>();

        private void AddUser(object sender, AddUserEventArgs eventArgs)
        {
            foreach (var user in eventArgs.Users)
            {
                userList.Add(user);
            }
            
        }

        private void AddUsersOnCreating(object sender, AddUserEventArgs eventArgs)
        {
            foreach (var user in eventArgs.Users)
            {
                userList.Add(user);
            }
            UnregisterOnCreating();
        }

        private void DeleteUser(object sender, DeleteUserEventArgs eventArgs)
        {
            foreach (var user in Search(eventArgs.DeletePredicate))
            {
                userList.Remove(user);
            }
        }

        public void RegisterAddUser()
        {
            serviceMaster.AddUser += AddUser;
        }

        public void RegisterDeleteUser()
        {
            serviceMaster.DeleteUser += DeleteUser;
        }

        public void UnregisterAddUser()
        {
            serviceMaster.AddUser -= AddUser;
        }

        public void UnregisterDeleteUser()
        {
            serviceMaster.DeleteUser -= DeleteUser;
        }


        public void RegisterOnCreating()
        {
            serviceMaster.AddUserOnSlaveCreating += AddUsersOnCreating;
        }

        public void UnregisterOnCreating()
        {
            serviceMaster.AddUserOnSlaveCreating -= AddUsersOnCreating;
        }

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
