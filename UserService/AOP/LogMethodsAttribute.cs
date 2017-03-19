using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;
using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace UserService.AOP
{
    [Serializable]
    class LogMethodsAttribute : OnMethodBoundaryAspect
    {
        public static Logger Log = LogManager.GetCurrentClassLogger();
        private string methodName;
        private Type className;

        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            methodName = method.Name;
            className = method.DeclaringType;
        }

        public override void OnEntry(MethodExecutionArgs args)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["isLogged"]))
                Log.Trace( $"Сlass {className} call method {methodName}");
        }

        public override void OnException(MethodExecutionArgs args)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["isLogged"]))
                Log.Error($"Сlass {className} fall with exception {args.Exception}, in method {methodName}");
        }
    }
}
