using System;
using System.Collections.Generic;
using UserService.Events;

namespace UserService.Services
{
    public interface IUserServiceMaster : IUserService
    {
        void Add(User user);
        void Delete(Func<User, bool> deletePredicate);
        event EventHandler<AddUserEventArgs> AddUser;
        event EventHandler<DeleteUserEventArgs> DeleteUser;
        event EventHandler<AddUserEventArgs> AddUserOnSlaveCreating;
    }
}
