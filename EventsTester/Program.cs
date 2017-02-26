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
            AppDomain firstDomain = AppDomain.CreateDomain($"Domain_Slave");
            var instance = firstDomain.CreateInstanceAndUnwrap("UserService",
                "UserService.Services.UserServiceMaster", true, BindingFlags.Default, null, new[] { (object)new CounterId() },
                CultureInfo.CurrentCulture, null);
            UserServiceMaster usm = (UserServiceMaster)instance;
            //UserServiceMaster usm = new UserServiceMaster(new CounterId());
            IUserService firstUss;
            usm.TryGetNextSlaveInstance(out firstUss);
            //firstUss = new UserServiceSlave(usm);
            //usm.Add(new User {Age = 8, FirstName = "a", LastName = "a"});
            Console.WriteLine($"{usm.Search(n => true).FirstOrDefault().FirstName}");
            Console.WriteLine($"{firstUss.Search(n => true).FirstOrDefault().FirstName}");
            Console.ReadLine();
        }
    }
}
