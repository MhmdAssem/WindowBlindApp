using POS_Service.Extra;
using System;
using System.ServiceProcess;
using System.Timers;

namespace POS_Service
{
    public partial class Service1 : ServiceBase
    {
        // this is the service it self

        // it has a timer that run every 2 minutes
        private Timer _timer = null;

        // this is table i will get this value form an text file on the c partition 
        string table = "";
        string mongoDbCon = "";


        public Service1()
        {
            InitializeComponent();
        }

        // this method i use to test 
        public void onDebug()
        {
            OnStart(null);
            Timer_tick(null, null);
        }

        // this method that run we the service start

        protected override void OnStart(string[] args)
        {
            try
            {
                table = Reader.TableReader()[0];
                mongoDbCon = Reader.TableReader()[1];
            }
            catch (Exception e)
            {
            
            }

            _timer = new Timer();
            _timer.Start();
            _timer.Interval = 60000;
            _timer.Enabled = true;
            _timer.Elapsed += new ElapsedEventHandler(Timer_tick);

        }



        private void Timer_tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                _timer.Stop();
                ServiceBusiness service = new ServiceBusiness();
                service.SendToComPort();
                _timer.Start();

            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnStop()
        {
            //stop the timer if the service stopped
            _timer.Enabled = false;
        }


    }
}
