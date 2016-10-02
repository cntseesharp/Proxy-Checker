using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;

namespace ProxyChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            int counter = 0;
            int total = 0;
            int good = 0;
            Console.WriteLine("Grabbing proxies from proxies.txt file");
            List<string> goodProxies = new List<string>();
            FileStream fileStream = new FileStream(@"proxies.txt", FileMode.Open, FileAccess.Read);

            Console.WriteLine("Checking proxies at 1000 threads");
            StreamReader sr = new StreamReader(fileStream);

            DateTime date = DateTime.Now;

            int threads = 0;
            while (!sr.EndOfStream)
            {
                while (threads >= 1000) Thread.Sleep(100);
                counter++;
                string temp = sr.ReadLine();
                new Thread(() =>
                {
                    threads++;
                    if (CheckProxy(temp))
                    {
                        good++;
                        using(StreamWriter sw = new StreamWriter(@"goodProxies.txt"))
                            sw.WriteLine(temp);
                    }
                    threads--;
                    Console.Write("\rTested: {0}, threads: {2}, for {1}", ++total, (DateTime.Now - date).ToString(), threads);
                }).Start();
            }

            while (total != counter)
                Thread.Sleep(1000);
            TimeSpan took = DateTime.Now - date;
            sr.Close();
            fileStream.Close();
            //System.IO.File.WriteAllLines(@"goodProxies.txt", goodProxies.ToArray());
            Console.WriteLine("\nDone. Good proxies: " + good + " in total of " + total + ", took " + (DateTime.Now - date).ToString());
            Console.Read();
        }

        static bool CheckProxy(string proxy)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.google.com/");
                httpWebRequest.Proxy = (IWebProxy)new WebProxy(proxy);
                httpWebRequest.Method = "GET";
                httpWebRequest.Timeout = 2000;
                httpWebRequest.ReadWriteTimeout = 5000;
                DateTime now1 = DateTime.Now;
                Task<WebResponse> responseAsync = httpWebRequest.GetResponseAsync();
                while ((responseAsync.Status == TaskStatus.WaitingForActivation || responseAsync.Status == TaskStatus.WaitingToRun) && (now1.AddSeconds(10.0) > DateTime.Now))
                    Thread.Sleep(500);
                DateTime now2 = DateTime.Now;
                while (responseAsync.Status == TaskStatus.Running && (now2.AddSeconds(2.0) > DateTime.Now))
                    Thread.Sleep(500);
				return responseAsync.Status == TaskStatus.RanToCompletion;
            }
            catch { }
            return false;
        }   
    }
}