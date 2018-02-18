using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryTest
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Chunk> chunks = new List<Chunk>();
            DateTimeOffset globalstart = DateTimeOffset.Now.AddMinutes(-25);
            globalstart = globalstart.AddSeconds(-globalstart.Second);
            DateTimeOffset endTime = globalstart.AddMinutes(0).AddSeconds(20);
            DateTimeOffset startTime = globalstart.AddMinutes(0).AddSeconds(-15);
            for (int i = 0; i < 1; i++)
            {
                chunks.Add(new Chunk() { ID = i, StartTime = globalstart.AddMinutes(i) });
            }

            var times = from d in chunks
                        where d.StartTime < startTime
                        select d.StartTime;

            //var maxTime = (from d in chunks
            //               where d.StartTime < startTime
            //               select d.StartTime).Max();
            
            var mt = (from c in chunks
                     select c.StartTime).Min();

            //if (mt > startTime)
            //    startTime = mt;

            var samplesChunks = (from c in chunks
                                 where c.StartTime <= endTime &&
                                     ((from d in chunks
                                       where d.StartTime <= startTime
                                       select d.StartTime).DefaultIfEmpty(chunks.Min(t=>t.StartTime)).Max() <= c.StartTime)
                                 select c).OrderBy(c => c.StartTime);

            var samplesChunks1 = chunks
                .Where(c => c.StartTime <= endTime && c.StartTime >= chunks
                    .Where(d => d.StartTime <= startTime)
                    .Select(d => d.StartTime).Max());

            //var minTime = samplesChunks.Select(t => t.StartTime).Min();
            //var mintime1 = samplesChunks.Min(t => t.StartTime);

            Console.WriteLine(String.Format("startTime = {0}\nendTime = {1}", startTime.ToString(), endTime.ToString()));
            Console.WriteLine("From...Where...Select");
            foreach (var item in samplesChunks)
            {
                Console.WriteLine(String.Format("ID = {0}, Time = {1}", item.ID, item.StartTime.ToString()));
            }

            Console.WriteLine("Obj.Where.Select");
            foreach (var item in samplesChunks1)
            {
                Console.WriteLine(String.Format("ID = {0}, Time = {1}", item.ID, item.StartTime.ToString()));
            }

            //Console.WriteLine(String.Format("MinTime = {0}\nMinTime1 = {1}", minTime.ToString(), mintime1.ToString()));


            Console.ReadLine();
        }
    }

    public class Chunk
    {
        public DateTimeOffset StartTime { get; set; }
        public int ID { get; set; }
    }
}
