using System;
using System.Collections.Generic;
using UserService.Events;

namespace UserService.Services
{
    public interface IUserServiceMaster : IUserService
    {
        void Add(User user);
        void Delete(Func<User, bool> deletePredicate);
        event EventHandler<UserEventArgs> AddUser;
        event EventHandler<UserEventArgs> DeleteUser;
        event EventHandler<UserEventArgs> AddUserOnSlaveCreating;
    }
}
