using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace BuildRunnerManager
{
    public class ProjectToBuild
    {
        public String Name { get; set; }
    }

    [CollectionDataContract(Name = "Projects")]
    public class ProjectsList : List<ProjectToBuild>
    {
        private ProjectsList()
        {
            var dict = BuildManager.Instance.BuildName2Path;
            foreach (var item in dict)
            {
                Add(new ProjectToBuild() { Name = item.Key });
            }
        }

        public static ProjectsList GetProjectsList()
        {
            lock (s_lock)
            {
                AllProjects = new ProjectsList();
                return AllProjects;
            }
        }

        private static ProjectsList AllProjects = null;
        private static object s_lock = new object();
    }

    public class BuildRequest
    {
        public String Name { get; set; }
        public DateTime RequestTime { get; set; }
    }

    [CollectionDataContract(Name = "BuildRequests")]
    public class BuildRequestsList : List<BuildRequest>
    {
        private BuildRequestsList()
        {
            var queue = BuildRequestsQueue.Queue;
            foreach (var item in queue.QueueList)
            {
                Add(new BuildRequest() { Name = item.Name, RequestTime = item.RequestTime });
            }
        }

        public static BuildRequestsList GetList()
        {
            lock (s_lock)
            {
                AllRequests = new BuildRequestsList();
                return AllRequests;
            }
        }

        private static BuildRequestsList AllRequests = null;
        private static object s_lock = new object();
    }

    public class CompletedBuild
    {
        public String Project { get; set; }
        public String BuildLog { get; set; }
    }

    [CollectionDataContract(Name = "CompletedBuilds")]
    public class CompletedBuildsList : List<CompletedBuild>
    {
        private CompletedBuildsList()
        {
            var dict = BuildManager.Instance.BuildName2Path;
            foreach (var item in dict)
            {
                String folder = item.Value.Remove(item.Value.LastIndexOf("\\") + 1);
                folder += "Log";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var files = Directory.EnumerateFiles(folder);
                foreach (var file in files)
                {
                    Add(new CompletedBuild() { Project = item.Key, BuildLog = file.Remove(0, file.LastIndexOf("\\") + 1).Trim() });
                }
            }
        }

        public static CompletedBuildsList GetCompletedBuildsList()
        {
            lock (s_lock)
            {
                AllBuilds = new CompletedBuildsList();
                return AllBuilds;
            }
        }

        private static CompletedBuildsList AllBuilds = null;
        private static object s_lock = new object();
    }

    public class LogFile
    {
        public String LogStr { get; set; }
    }
}
