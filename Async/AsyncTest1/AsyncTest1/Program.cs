using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;

namespace AsyncTest1
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();
            Console.WriteLine("\n\nDone Main");
            Console.ReadLine();
        }

        private async static void Run()
        {
            var sc = new SomeClass();
            //sc.Go();
            await sc.GoLoop();
            //sc.GoTask();
            Console.WriteLine("\n\nDone Run!");
            //Console.ReadLine();
        }
    }

    public class SomeClass
    {
        public SomeClass()
        {
        }

        public void GoTask()
        {
            //Task.Delay(2000);
            Task.Factory.StartNew(() => BeginGoTask());
            Console.WriteLine("\n\nGoTask done");
        }

        private void BeginGoTask()
        {
            //Task.Delay(5000);
            Console.WriteLine("\n\nBeginGoTask done");
        }

        public void Go()
        {
            var res = GoAsync();
            Console.WriteLine("Done Go");
        }

        public async Task<XElement> GoAsync()
        {
            //var t1 = Task.Factory.StartNew(() => GetFirst());
            var t1 =  GetFirst();
            var t2 = GetSecond();
            //var t2 = Task.Factory.StartNew(() => GetSecond());
            await Task.WhenAll(t1, t2);
            var x1 = t1.Result;
            var x2 = t2.Result;
            XElement toPrint = new XElement("Tasks", x1, x2);
            Console.WriteLine(toPrint.ToString());
            return toPrint;
        }

        public async Task<XElement> GetFirst()
        {
            XElement created = new XElement("First");
            Console.WriteLine("GetFirst: " + DateTime.Now.ToString());
            await Task.Delay(3000);
            Console.WriteLine("GetFirst: " + DateTime.Now.ToString());
            return created;
        }

        public async Task<XElement> GetSecond()
        {
            XElement created = new XElement("Second");
            Console.WriteLine("GetSecond: " + DateTime.Now.ToString());
            await Task.Delay(1000);
            Console.WriteLine("GetSecond: " + DateTime.Now.ToString());
            return created;
        }

        public async Task<XElement> GoLoop()
        {
            List<Task<XElement>> loopedTasks = await GoLoopInternal();
            XElement toRet = new XElement("elements");
            foreach (var item in loopedTasks)
            {
                toRet.Add(item.Result);
                Console.WriteLine(item.Id);
            }


            Console.WriteLine("Finished GoLoop");
            Console.WriteLine(toRet.ToString());
            return toRet;
        }

        private async Task<List<Task<XElement>>> GoLoopInternal()
        {
            List<Task<XElement>> loopedTasks = new List<Task<XElement>>();
            for (int i = 0; i < 10; i++)
            {
                loopedTasks.Add(DoTask(i));
            }

            await Task.WhenAll(loopedTasks);
            return loopedTasks;
        }

        private async Task<XElement> DoTask(int i)
        {
            XElement toRet = new XElement("element", new XAttribute("value", i.ToString()));
            Console.WriteLine("Do Task {0}", i.ToString());
            await Task.Delay(10000 - (i * 100));
            Console.WriteLine("Do Task {0}", i.ToString());
            return toRet;
        }
    }
}
