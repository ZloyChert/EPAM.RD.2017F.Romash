using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UserService;
using UserService.IdCounters;
using UserService.Services;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Threading;
using UserService.TCP;

namespace EventsTester
{
    class Program
    {

        static void Main(string[] args)
        {
            ServiceManager sm = new ServiceManager();
            IUserServiceMaster usm;
            sm.TryGetMasterInstance(out usm);           
            
            ServerTcp st = new ServerTcp(usm);
            //Thread serverThread = new Thread(st.RunServer);
            //serverThread.Start();
            Task.Run(() => st.RunServer());
            ClientTcp ct = new ClientTcp();
            //Thread clientThread = new Thread(ct.RunClient);
            //clientThread.Start();
            Task.Run(() => ct.RunClient());
            IUserService firstUss;
            sm.TryGetNextSlaveInstance(out firstUss, ct);
            usm.Add(new User { Age = 12, FirstName = "qqwwwqq", LastName = "w" });
            Console.WriteLine("Master");
            foreach (var user in usm.Search(n => true))
            {
                Console.Write($"{user.FirstName}/{user.LastName};");
            }
            Console.WriteLine();
            Console.WriteLine("Slave");
            foreach (var user in firstUss.Search(n => true))
            {
                Console.Write($"{user.FirstName}/{user.LastName};");
            }
            /*******/
            usm.Delete(n => n.LastName == "w");
            Console.WriteLine();
            Console.WriteLine("Master");
            foreach (var user in usm.Search(n => true))
            {
                Console.Write($"{user.FirstName}/{user.LastName};");
            }
            Console.WriteLine();
            Console.WriteLine("Slave");
            foreach (var user in firstUss.Search(n => true))
            {
                Console.Write($"{user.FirstName}/{user.LastName};");
            }

            Console.ReadLine();
            
            st.StopServer();
            ct.StopClient();
            
        }
    }
}
