using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CRIPlugin;
using RestSharp;
using System.Diagnostics;

namespace CRIPluginUnitTest
{
    [TestClass]
    public class TestCRIPliginTask
    {
        [TestMethod]
        public void CRIPluginDBManagerStartTest()
        {
            string criPluginURL = @"http://localhost:7803/PluginsDataFeed/CheckListPlugin";

            CRIPluginTask criPluginTask = new CRIPluginTask();
            criPluginTask.Init(@"http://localhost:7803/PluginsDataFeed");
            criPluginTask.Start();

            string name = criPluginTask.Name();
            Assert.IsTrue(name.Equals("CheckListPlugin"));

            var client = new RestClient(criPluginURL);
            var request = new RestRequest(Method.GET);                       
            request.Timeout = 3000;

            var response = client.Execute(request);

            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK);

            //RunBatchFile("notepad");
        }

        private void RunBatchFile(string batchPath)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + batchPath);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;

            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");

            process.Close();
        }
    }
}
