using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Mapper.Initialize(c =>
            {
                c.CreateMap<FromBase, ToBase>().Include<FromDerived1, ToDerived1>().Include<FromDerived2, ToDerived2>().Include<FromDerived3, ToDerived3>();
                c.CreateMap<FromDerived1, ToDerived1>();
                c.CreateMap<FromDerived2, ToDerived2>();
                c.CreateMap<FromDerived3, ToDerived3>();
            });

            double value = 14f;
            var val = String.Format("{0:#.0}", value);
            Console.Out.WriteLine(val);
            var val2 = value.ToString();
            int ind = val2.IndexOf(".");
            if (ind > 0)
                val2 = val2.Remove(ind + 2);
            Console.Out.WriteLine(val2);

            double x = 4f;
            for (int i = 0; i < 100; i++)
            {
                double y = x + i / 100f;
                Console.Out.WriteLine("y = " + y + ", rounded y = " + (long)Math.Round(y));
            }

            double z = 4.50001;
            Console.Out.WriteLine("z = " + z + ", rounded z = " + (long)Math.Round(z));

            Console.WriteLine("\n\nRandom:\n");
            Random rand = new Random();
            for (int i = 0; i < 10; i++)
            {
                int num = rand.Next(82, 134);
                Console.Write("{0, 3}    ", num);
            }

            Console.WriteLine();

            FromBase fb = new FromBase() { BaseStr = "From Base" };
            FromDerived1 fd1 = new FromDerived1() { BaseStr = "From base 1", DerivedStr1 = "From Derived 1" };
            FromBase fd2 = new FromDerived2() { BaseStr = "From base 2", DerivedStr2 = "From Derived 2" };
            FromBase fd3 = new FromDerived3() { BaseStr = "From base 3", DerivedStr3 = "From Derived 3" };

            var tb = Mapper.Map<ToBase>(fb);
            var td1 = Mapper.Map<ToDerived1>(fd1);
            var td2 = Mapper.Map<ToBase>(fd2);
            var td3 = Mapper.Map<ToBase>(fd3);

            Console.WriteLine("fb = {0} ======> tb = {1}", fb.ToString(), tb.ToString());
            Console.WriteLine("fd1 = {0} ======> td1 = {1}", fd1.ToString(), td1.ToString());
            Console.WriteLine("fd2 = {0} ======> td2 = {1}", fd2.ToString(), td2.ToString());
            Console.WriteLine("fd3 = {0} ======> td3 = {1}", fd3.ToString(), td3.ToString());

            var str = Console.In.ReadLine();
        }
    }
}
