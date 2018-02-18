using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConcurrentDictionary
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<long, List<byte>> dict = new Dictionary<long, List<byte>>();
            for (int i = 0; i < 100000; i += 2000)
            {
                List<byte> lst = new List<byte>();
                for (int j = 0; j < 1500; j++)
                {
                    lst.Add((byte)(j % 120));
                }

                dict[i] = lst;
            }

            var lastBlockStart = (from c in dict
                                  where c.Key < 87000
                                  select c).Max(d => d.Key);

            var lastBlock = dict[lastBlockStart];

            var listBlockStarts = (from c in dict
                                   where c.Key < 87000
                                   orderby c.Key
                                   select c.Key).ToList();

            listBlockStarts.RemoveAt(listBlockStarts.Count - 1);

            foreach (var item in listBlockStarts)
            {
                dict.Remove(item);
            }

            Console.WriteLine("lastBlock = " + lastBlock.Count);
            Console.WriteLine("dict size  = " + dict.Count);

            //ConcurrentDictionary<String, List<String>> conDic = new ConcurrentDictionary<String, List<String>>();
            //for (int i = 0; i < 5; i++)
            //{
            //    String tKey= String.Format("666-{0}-", i.ToString());
            //    List<String> tValueList = new List<String>();
            //    for (int j = 0; j < 3; j++)
            //    {
            //        tValueList.Add(String.Format("666-{0}-{1}", i.ToString(), j.ToString()));
            //    }

            //    conDic.TryAdd(tKey, tValueList);
            //}

            //String toPrint = String.Empty;
            //foreach (var item in conDic)
            //{
            //    if (item.Value.Contains("666-3-2"))
            //    {
            //        toPrint = item.Key;
            //        break;
            //    }
            //}

            //Console.WriteLine("mother id = " + toPrint);
            Console.ReadLine();
        }
    }
}
