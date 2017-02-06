using ApplicationCore;
using ApplicationUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UtilityModuleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Main Application Entry Point
            // Check if the command line arguments exists or else move with console gui
            // LogTo.Information("Application Started..");
            // Log.Time(nameof(Main))
            Console.WriteLine("Starting the application..");

            if (args.Length > 0)
            {
                // Get parameters from args
                var paramArg = args[0];
            }
            else
            {
                string infoText = "Listing all the available modules..";
                Console.WriteLine(infoText);
                // If user inserts one, just display the available modules.
                // display, get list of all available modules
                var lists = AtkUtils.GetAllModules();
                foreach (var item in lists)
                    Console.WriteLine($"\t{item}");
                //StartModule:
                Console.WriteLine("Enter the module name (-- Optional list any parameters with | delimeter).\nEg. ApplicationBusinessLogic.UpdateIntercomUser|12345 WHERE 12345 is Account Id");
                var moduleName = Console.ReadLine();
                string[] parts = null;
                if (moduleName != null && moduleName.Contains("|"))
                {
                    parts = moduleName.Split('|');
                    // Pass the parameters and invoke the instance                    
                    Task.Run(() => ((IModule) AtkUtils.GetInstanceWithParameters(parts[0], parts.Skip(1).ToArray()))?.Run());
                }
                else
                {
                    Console.WriteLine($"Running the task from {moduleName} module..");
                    // LogTo.Information($"Running the {moduleName} module");
                    // Invoke the required module here
                    Task.Run(() => ((IModule) AtkUtils.GetInstance(moduleName))?.Run());
                }

                // var parameters = input?.Split('|');
                //if (parameters != null && parameters.Length > 3)
                //{
                //    Console.WriteLine("Parameters Invalid !!");
                //    goto StartModule;
                //}
                // Check if the given module exists
                // Console.WriteLine("Do you want to schedule this module's task, Y or N ?");
                // var readChar = Console.ReadKey();
                //if (readChar.KeyChar == 'Y')
                //{
                //    Console.WriteLine("This module's task is now scheduled to run every 1 minute !!!");
                //    JobScheduler.ScheduleJob(moduleName);
                //}
                //else
                //{
                //Console.WriteLine($"Running the task from {moduleName} module..");
                // LogTo.Information($"Running the {moduleName} module");
                // Invoke the required module here
                // Task.Run(() => ((IModule) AtkUtils.GetInstance(moduleName))?.Run());
                // }
                Console.ReadKey();
            }
        }
    }
}
