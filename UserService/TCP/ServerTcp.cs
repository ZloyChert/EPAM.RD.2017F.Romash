using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        public bool IsRunning { get; private set; } = true;
        public void StopServer() => IsRunning = false;

        public ServerTcp()
        {
            tcpListener = new TcpListener(IPAddress.Parse(IpAddress), Port);
        }

        public void ConnectToMasterService(IUserServiceMaster userServiceMaster)
        {
            serviceMaster = userServiceMaster;
            RegisterAddUser();
            RegisterDeleteUser();
            RegisterOnCreating();
        }

        public void RunServer()
        {   
            tcpListener.Start();
            while (IsRunning) 
            {
                var tcpClient = tcpListener.AcceptTcpClient();
                listClients.Add(tcpClient);
            }
        }

        private void AddUser(object sender, UserEventArgs eventArgs)
        {
            SendMessageToClients("add", eventArgs);
        }

        private void AddUsersOnCreating(object sender, UserEventArgs eventArgs)
        {
            SendMessageToClients("add", eventArgs);
            UnregisterOnCreating();
        }

        private void DeleteUser(object sender, UserEventArgs eventArgs)
        {
            SendMessageToClients("delete", eventArgs);
        }

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
    }
}
