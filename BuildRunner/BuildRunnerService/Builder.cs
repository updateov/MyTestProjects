using BuildRunnerManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Linq;

namespace BuildRunnerService
{
    public class Builder : IBuilder
    {
        private static object s_lock = new Object();

        public ProjectsList GetCompatibleBuilds()
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            return ProjectsList.GetProjectsList();
        }

        public BuildRequestsList GetRunningBuilds()
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            return BuildRequestsList.GetList();
        }

        public BuildRequestsList RequestBuild(String BuildName)
        {
            try
            {
                BuildRequestsQueue.Queue.AddToQueue(BuildName);
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotAcceptable;
                return null;
            }

            return BuildRequestsList.GetList();
        }

        public BuildRequestsList DeleteRequest(BuildRequest param)
        {
            try
            {
                var queue = BuildRequestsQueue.Queue;
                var elem = (from c in queue.QueueList
                            where c.Name.Equals(param.Name) && c.RequestTime == param.RequestTime
                            select c).FirstOrDefault();

                if (elem != null)
                    queue.QueueList.Remove(elem);

                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotAcceptable;
            }

            return BuildRequestsList.GetList();
        }


        public CompletedBuildsList GetCompletedBuilds()
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            return CompletedBuildsList.GetCompletedBuildsList();
        }


        public LogFile GetBuildLog(String ProjectName, String BuildLog)
        {
            var dict = BuildManager.Instance.BuildName2Path;
            var path = dict[ProjectName];
            var log = path.Remove(path.LastIndexOf("\\") + 1) + "Log\\" + BuildLog;
            var bytes = File.ReadAllBytes(log);
            String fileStr = Convert.ToBase64String(bytes);
            LogFile toRet = new LogFile() { LogStr = fileStr };
            return toRet;
        }


        public LogFile GetCurrentBuildingLog()
        {
            lock (s_lock)
            {
                String str = BuildRequestsQueue.Queue.CurrentBuildingLog;
                using (StreamWriter sw = new StreamWriter("tmp.log"))
                {
                    sw.Write(str);
                }

                var bytes = File.ReadAllBytes("tmp.log");
                String fileStr = Convert.ToBase64String(bytes);
                LogFile toRet = new LogFile() { LogStr = fileStr };
                return toRet;
            }
        }
    }
}
