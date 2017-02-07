using ApplicationBusinessLogic;
using ApplicationCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationUtilities
{
    public static class AtkUtils
    {
        // TODO : We may need a better way for creating an instance, when the class library is involved
        private static readonly Type[] ModuleTypes;

        static AtkUtils()
        {
            // Search for all class names (modules) under ApplicationBusinessLogic namespace implementing IModule interface
            var moduleAssembly = typeof(UpdateIntercomUser).Assembly;
            ModuleTypes = moduleAssembly.GetTypes().Where(t => typeof(IModule).IsAssignableFrom(t)).ToArray();
        }

        public static object GetInstance(string moduleName)
        {
            return
            (from mtype in ModuleTypes
                where mtype.FullName == moduleName
                select Activator.CreateInstance(mtype)).FirstOrDefault();
        }

        // Create an instance with contructor parameters
        public static object GetInstanceWithParameters(string moduleName, object[] parameters)
        {
            return
            (from mtype in ModuleTypes
                where mtype.FullName == moduleName
                select Activator.CreateInstance(mtype, parameters)).FirstOrDefault();
        }

        public static IEnumerable<string> GetAllModules() => ModuleTypes.Select(typ => typ.FullName);
    }
}
