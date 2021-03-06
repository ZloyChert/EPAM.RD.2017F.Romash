﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UserService.AOP;
using UserService.Events;
using UserService.Services;
using UserService.TCP;

namespace UserService
{
    public class ServiceManager
    {
        public int CountOfRemainingSlaves { get; private set; }
        public int CountOfRemainingMasters { get; private set; }
        private IUserServiceMaster userServiceMaster;

        [LogMethods]
        public ServiceManager()
        {
            int tempCount;
            if (!int.TryParse(ConfigurationManager.AppSettings["CountOfSlaves"], out tempCount))
                tempCount = 1;
            CountOfRemainingSlaves = tempCount;
            CountOfRemainingMasters = 1;
        }

        [LogMethods]
        public bool TryGetMasterInstance(out IUserServiceMaster masterService)
        {
            if (CountOfRemainingMasters > 0)
            {
                AppDomain firstDomain = AppDomain.CreateDomain($"Domain_Master");
                var instance = firstDomain.CreateInstanceAndUnwrap("UserService",
                    "UserService.Services.UserServiceMaster", true, BindingFlags.Default, null, new[] { (object)new IdCounters.CounterId() },
                    CultureInfo.CurrentCulture, null);
                masterService = (IUserServiceMaster)instance;
                CountOfRemainingMasters--;
                userServiceMaster = masterService;
                return true;
            }
            masterService = null;
            return false;
        }

        [LogMethods]
        public bool TryGetNextSlaveInstance(out IUserServiceSlave slaveService, ClientTcp t)
        {
            int countOfSlaves = int.Parse(ConfigurationManager.AppSettings["CountOfSlaves"]);
            if (CountOfRemainingSlaves > 0 && CountOfRemainingMasters == 0)
            {
                AppDomain firstDomain = AppDomain.CreateDomain($"Domain_{countOfSlaves - CountOfRemainingSlaves + 1}");
                var instance = firstDomain.CreateInstanceAndUnwrap("UserService",
                    "UserService.Services.UserServiceSlave", true, BindingFlags.Default, null, new object[] {},
                    CultureInfo.CurrentCulture, null);
                slaveService = (IUserServiceSlave)instance;
                CountOfRemainingSlaves--;
                slaveService.ConnectToTcpClient(t);
                MethodInfo method = userServiceMaster.GetType()
                    .GetMethod("OnAddUserOnCreatingSlave", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(userServiceMaster, new object[] {});
                return true;
            }
            slaveService = null;
            return false;
        }
    }
}
