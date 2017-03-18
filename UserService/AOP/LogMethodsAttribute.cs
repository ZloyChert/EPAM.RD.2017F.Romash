using System;
using System.Collections.Generic;
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
        private string methodName;
        private Type className;
        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            methodName = method.Name;
            className = method.DeclaringType;
        }

        public override void OnEntry(MethodExecutionArgs args)
        {
            Console.WriteLine($"{methodName},{className}");
        }
    }
}
