using System;
using UserService.TCP;

namespace UserService.Services
{
    public interface IUserServiceSlave : IUserService
    {
        void ConnectToTcpClient(ClientTcp client);
    }
}
