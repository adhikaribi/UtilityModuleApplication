using ApplicationCore;
using System;
using System.Collections.Generic;

namespace ApplicationBusinessLogic
{
    public class IntercomUpdate : IModule
    {
        private static string _moduleName = "IntercomUpdate";
        private static string _description = "IntercomUpdate does all the updates";
        public string Name => _moduleName;

        // TODO : Write logic for Run method here.
        public void Run()
        {
            Console.WriteLine("IntercomUpdate Started....");
            Console.WriteLine("IntercomUpdate Ended");
        }

        public string Description => _description;
    }
}
