using System.ServiceProcess;

namespace POS_Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // here we set up the service start mode
            // if you using debug active the below code


            //Service1 myService = new Service1();
            //myService.onDebug();


            // if you using release active the below code


            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
