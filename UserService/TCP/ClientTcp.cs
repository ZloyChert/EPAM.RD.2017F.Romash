using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UserService.Events;
using UserService.Services;

namespace UserService.TCP
{
    [Serializable]
    public class ClientTcp : MarshalByRefObject
    {
        public event EventHandler<UserEventArgs> AddUser = delegate { };
        public event EventHandler<UserEventArgs> DeleteUser = delegate { };
        public bool IsRunning { get; private set; } = true;
        public void StopClient() => IsRunning = false;
        public void ConnectToSlaveService(IUserServiceSlave service)
        {
            if (service == null)
                throw new ArgumentNullException();
            service.ConnectToTcpClient(this);
        }
        public void RunClient()
        {
            var tcpclnt = new TcpClient();

            tcpclnt.Connect("127.0.0.1", 8001);
            Stream stm = tcpclnt.GetStream();
            while (IsRunning)
            {
                byte[] length = new byte[4];
                stm.Read(length, 0, 4);
                int leng = BitConverter.ToInt32(length, 0);
                byte[] bmsg = new byte[leng];
                stm.Read(bmsg, 0, bmsg.Length);
                MessageUsers message = Serializer.DeserializeUsers(bmsg);
                if ( message.Mess == "add")
                    OnAddUser(new UserEventArgs(message.ListUsers));
                if (message.Mess == "delete")
                    OnDeleteUser(new UserEventArgs(message.ListUsers));
            }
        }

        protected virtual void OnAddUser(UserEventArgs e)
        {
            EventHandler<UserEventArgs> temp = AddUser;
            temp?.Invoke(this, e);
        }

        protected virtual void OnDeleteUser(UserEventArgs e)
        {
            EventHandler<UserEventArgs> temp = DeleteUser;
            temp?.Invoke(this, e);
        }

    }
}
