using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UserService.AOP;
using UserService.Events;
using UserService.Services;

namespace UserService.TCP
{
    [Serializable]
    public class ServerTcp : MarshalByRefObject
    {
        private readonly TcpListener tcpListener;
        private readonly List<TcpClient> listClients = new List<TcpClient>();
        private IUserServiceMaster serviceMaster;
        private static readonly string IpAddress = "127.0.0.1";
        private static readonly int Port = 8001;

        [LogMethods]
        public bool IsRunning { get; private set; } = true;

        [LogMethods]
        public void StopServer() => IsRunning = false;

        [LogMethods]
        public ServerTcp()
        {
            tcpListener = new TcpListener(IPAddress.Parse(IpAddress), Port);
        }

        [LogMethods]
        public void ConnectToMasterService(IUserServiceMaster userServiceMaster)
        {
            serviceMaster = userServiceMaster;
            RegisterAddUser();
            RegisterDeleteUser();
            RegisterOnCreating();
        }

        [LogMethods]
        public void RunServer()
        {   
            tcpListener.Start();
            while (IsRunning) 
            {
                var tcpClient = tcpListener.AcceptTcpClient();
                listClients.Add(tcpClient);
            }
        }

        [LogMethods]
        private void AddUser(object sender, UserEventArgs eventArgs)
        {
            SendMessageToClients("add", eventArgs);
        }

        [LogMethods]
        private void AddUsersOnCreating(object sender, UserEventArgs eventArgs)
        {
            SendMessageToClients("add", eventArgs);
            UnregisterOnCreating();
        }

        [LogMethods]
        private void DeleteUser(object sender, UserEventArgs eventArgs)
        {
            SendMessageToClients("delete", eventArgs);
        }

        [LogMethods]
        private void SendMessageToClients(string mode, UserEventArgs eventArgs)
        {
            foreach (var client in listClients)
            {
                var strem = client.GetStream();
                MessageUsers msg = new MessageUsers(eventArgs.Users, mode);
                byte[] bmsg = Serializer.Serialize(msg);
                strem.Write(BitConverter.GetBytes(bmsg.Length), 0, 4);
                strem.Write(bmsg, 0, bmsg.Length);
            }
        }

        [LogMethods]
        public void RegisterAddUser()
        {
            serviceMaster.AddUser += AddUser;
        }

        [LogMethods]
        public void RegisterDeleteUser()
        {
            serviceMaster.DeleteUser += DeleteUser;
        }

        [LogMethods]
        public void UnregisterAddUser()
        {
            serviceMaster.AddUser -= AddUser;
        }

        [LogMethods]
        public void UnregisterDeleteUser()
        {
            serviceMaster.DeleteUser -= DeleteUser;
        }

        [LogMethods]
        public void RegisterOnCreating()
        {
            serviceMaster.AddUserOnSlaveCreating += AddUsersOnCreating;
        }

        [LogMethods]
        public void UnregisterOnCreating()
        {
            serviceMaster.AddUserOnSlaveCreating -= AddUsersOnCreating;
        }
    }
}
