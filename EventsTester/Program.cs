using System;
using System.Threading.Tasks;
using UserService;
using UserService.Services;
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
            ServerTcp st = new ServerTcp();
            st.ConnectToMasterService(usm);
            Task.Run(() => st.RunServer());

            ClientTcp ct = new ClientTcp();
            Task.Run(() => ct.RunClient());    

            IUserServiceSlave firstUss;
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
