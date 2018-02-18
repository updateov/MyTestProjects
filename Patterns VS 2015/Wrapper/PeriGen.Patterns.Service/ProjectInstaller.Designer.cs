namespace PeriGen.Patterns.Service
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
			this.serviceProcessPeriGenPatternsServiceInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.PeriGenPatternsServiceInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// serviceProcessPeriGenPatternsServiceInstaller
			// 
			this.serviceProcessPeriGenPatternsServiceInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			this.serviceProcessPeriGenPatternsServiceInstaller.Password = null;
			this.serviceProcessPeriGenPatternsServiceInstaller.Username = null;
			// 
			// PeriGenPatternsServiceInstaller
			// 
			this.PeriGenPatternsServiceInstaller.DisplayName = "PeriGen Patterns Service";
			this.PeriGenPatternsServiceInstaller.ServiceName = "PeriGenPatternsService";
			this.PeriGenPatternsServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessPeriGenPatternsServiceInstaller,
            this.PeriGenPatternsServiceInstaller});

		}

		#endregion

		private System.ServiceProcess.ServiceProcessInstaller serviceProcessPeriGenPatternsServiceInstaller;
		private System.ServiceProcess.ServiceInstaller PeriGenPatternsServiceInstaller;
	}
}