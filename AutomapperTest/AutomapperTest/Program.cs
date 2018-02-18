using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomapperTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var map = Mapper.CreateMap<MyClass, MyClassDB>();
            map.ForMember(cv => cv.Str34, m => m.MapFrom(s => String.Format("{0} {1}", s.Str3, s.Str4)));
            map.ForMember(c => c.NowTimeOffset, d => d.MapFrom(s => new DateTimeOffset(s.NowTime, TimeZoneInfo.Local.BaseUtcOffset)));
            Run();
        }

        private static void Run()
        {
            MyClass mc = new MyClass();
            mc.InitMyClass();
            MyClassDB mcDB = Mapper.Map<MyClass, MyClassDB>(mc);
            Console.WriteLine(mcDB.ToString());
            Console.ReadLine();
        }
    }

    public class MyClass
    {
        public String Str1 { get; set; }
        public String Str2 { get; set; }
        public String Str3 { get; set; }
        public String Str4 { get; set; }
        public DateTime NowTime { get; set; }

        internal void InitMyClass()
        {
            Str1 = "afbba1";
            Str2 = "afbba2";
            Str3 = "afbba3";
            Str4 = "afbba4";
            NowTime = DateTime.Now;
        }
    }

    public class MyClassDB
    {
        public String Str1 { get; set; }
        public String Str2 { get; set; }
        public String Str3 { get; set; }
        public String Str4 { get; set; }
        public String Str34 { get; set; }
        public DateTimeOffset NowTimeOffset { get; set; }

        public override string ToString()
        {
            return String.Format("Str1 = {0}\nStr2 = {1}\nStr3 = {2}\nStr4 = {3}\nStr34 = {4}\nNowTimeOffset = {5}", Str1, Str2, Str3, Str4, Str34, NowTimeOffset.ToString());
        }
    }
}
