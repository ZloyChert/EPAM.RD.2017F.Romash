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
using System.Reflection;

namespace EventsTester
{
    class Program
    {

        static void Main(string[] args)
        {
            ServiceManager sm = new ServiceManager();
            IUserServiceMaster usm;
            sm.TryGetMasterInstance(out usm);
            //UserServiceMaster usm = new UserServiceMaster(new CounterId());
            IUserService firstUss;
            sm.TryGetNextSlaveInstance(out firstUss);
            //firstUss = new UserServiceSlave(usm);
            //usm.Add(new User {Age = 8, FirstName = "b", LastName = "b"});
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
        }
    }
}
