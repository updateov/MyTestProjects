using System.ServiceProcess;

namespace PatternsAddOnService
{
    partial class AddOnWinService : ServiceBase
    {
        public AddOnWinService()
        {
            InitializeComponent();
            Task = new AddOnTask();
        }

        protected override void OnStart(string[] args)
        {
            Task.StartTask();
        }

        protected override void OnStop()
        {
            Task.StopTask();
        }

        private AddOnTask Task { get; set; }
    }
}
