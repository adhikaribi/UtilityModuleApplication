using ApplicationBusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ApplicationUtilities
{
    public static class AtkUtils
    {
        // TODO : May be later we Use Unity IoC for getting / creating instances
        public static object GetInstance(string moduleName)
        {
            if (!Regex.IsMatch(moduleName, "ApplicationBusinessLogic.*"))
                moduleName = $"ApplicationBusinessLogic.{moduleName}";
            var type = Type.GetType($"{moduleName}, ApplicationBusinessLogic", false, true);
            return type != null ? Activator.CreateInstance(type) : null;
        }

        public static IEnumerable<string> GetAllModules()
        {
            // Search for all class name under ApplicationBusinessLogic namespace
            // Find the Assembly with class, for eg. IntercomUpdate and list all class names inside that assembly
            var moduleAssembly = typeof(IntercomUpdate).Assembly;
            return moduleAssembly.GetTypes().Select(typ => typ.FullName).ToList();
        }
    }
}
