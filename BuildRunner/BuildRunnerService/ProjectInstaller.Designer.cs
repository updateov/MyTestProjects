namespace BuildRunnerService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BuildServiceProcessIntaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.BuildServiceIstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // BuildServiceProcessIntaller
            // 
            this.BuildServiceProcessIntaller.Password = null;
            this.BuildServiceProcessIntaller.Username = null;
            // 
            // BuildServiceIstaller
            // 
            this.BuildServiceIstaller.Description = "Available builds service";
            this.BuildServiceIstaller.DisplayName = "Perigen Build Service";
            this.BuildServiceIstaller.ServiceName = "Service1";
            this.BuildServiceIstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.BuildServiceProcessIntaller,
            this.BuildServiceIstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller BuildServiceProcessIntaller;
        private System.ServiceProcess.ServiceInstaller BuildServiceIstaller;
    }
}