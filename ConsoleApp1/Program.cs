using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Data;
using Microsoft.ML.Legacy.Models;
using Microsoft.ML.Legacy.Trainers;
using Microsoft.ML.Legacy.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor;
using OpenHardwareMonitor.Hardware;
using System.Threading;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        static public List<float[]> data;
        static public List<float[]> data2;
        static bool isFirst;
        static CancellationTokenSource cts = new CancellationTokenSource();
        static CancellationTokenSource cts2 = new CancellationTokenSource();
        static Task task2;
        static Task task1;
        static Stopwatch sw = new Stopwatch();
        static TimeSpan m1Time;
        static TimeSpan m2Time;

        

        static void Main(string[] args)
        {
            isFirst = true;
            data = new List<float[]>();            
            data2 = new List<float[]>();

            Console.WriteLine("Press ENTER to START");
            if (Console.ReadLine() == "")
            {
                task1 = Task.Run(() => Measure1(cts.Token));
                sw.Start();
            }
            
            if (Console.ReadLine() == "")
            {                     
                cts.Cancel();
                task1.Wait();
                isFirst = false;
                sw.Stop();
                m1Time = sw.Elapsed;
            }
            Console.WriteLine("\n ---1. Measure--- \n  Time: "+m1Time.Minutes+" min "+m1Time.Seconds+" sec");
            Console.WriteLine(" Count: "+ data.Count+"db");
            Console.WriteLine("\n-------PAUSED-------");
            Console.WriteLine("Press ENTER CONTUNIE");
            sw = new Stopwatch();
            if (Console.ReadLine()=="")
            {
                task2 = Task.Run(() => Measure2(cts2.Token));
                sw.Start();                
            }
            if (Console.ReadLine() == "")
            {
                cts2.Cancel();
                task2.Wait();
                sw.Stop();
                m2Time = sw.Elapsed;
                WriteOutAverages();
            }

            Console.ReadLine();
        }

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
        static void GetSystemInfo()
        {
            float[] temps = new float[5];
            int idx = 0;
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;

            computer.Accept(updateVisitor);

            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                        {
                            Console.WriteLine(computer.Hardware[i].Sensors[j].Name + ": " + computer.Hardware[i].Sensors[j].Value.ToString() + "°\r");
                            temps[idx] = (float)computer.Hardware[i].Sensors[j].Value;
                            idx++;

                        }

                    }
                }
            }
            if (isFirst)
            {
                data.Add(temps);
            }
            else
            {
                data2.Add(temps);
            }
            computer.Close();
        }
        static double[] Avarage(List<float[]> data)
        {
            double[] avarageArray = new double[5];

            float one = 0;
            float two = 0;
            float three = 0;
            float four = 0;
            float five = 0;

            foreach (float[] item in data)
            {
                one += item[0];
                two += item[1];
                three += item[2];
                four += item[3];
                five += item[4];
            }

            avarageArray[0] = Math.Round(one / data.Count, 1);
            avarageArray[1] = Math.Round(two / data.Count, 1);
            avarageArray[2] = Math.Round(three/data.Count, 1);
            avarageArray[3] = Math.Round(four/ data.Count,1);
            avarageArray[4] = Math.Round(five/ data.Count,1);
            return avarageArray;
        }

        private static void WriteOutAverages()
        {
            Console.Clear();
            double[] av1 = Avarage(data);
            double[] av2 = Avarage(data2);            
            Console.WriteLine("----1. Measure----      Time: " + m1Time.Minutes + " min " + m1Time.Seconds + " sec");
            Console.WriteLine("                         Count: " + data.Count + "db");

            int db = 1;
            foreach (double item in av1)
            {
                Console.WriteLine("CPU Core #" + db + ": " + item + "°");
                db++;
            }

            Console.WriteLine("\n ----2. Measure----      Time: " + m2Time.Minutes + " min " + m2Time.Seconds + " sec");
            Console.WriteLine("                         Count: " + data2.Count + "db");
            db = 1;
            foreach (double item in av2)
            {
                Console.WriteLine("CPU Core #" + db + ": " + item + "°");
                db++;
            }
            Console.WriteLine("\n------------------------");
            Console.WriteLine("Diffes:");
            for (int i = 0; i < av1.Length; i++)
            {
                Console.WriteLine("CPU Core #" + (i+1) + ": " + Math.Round(av2[i]-av1[i],2) + "°");
            }
        }

        static void Measure1(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Console.Clear();
                Console.WriteLine("------ 1. Measure ------" );
                Console.WriteLine("\nActuals:");
                GetSystemInfo();

                Console.WriteLine();
                Console.WriteLine("Averages:");
                double[] avarageArray = Avarage(data);
                int db = 1;
                foreach (double item in avarageArray)
                {
                    Console.WriteLine("CPU Core #" + db + ": " + item + "°");
                    db++;
                }
                Console.WriteLine("\n--> Press ENTER to PAUSE");
                Thread.Sleep(2000);
            }
        }        
        private static void Measure2(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {                
                Console.Clear();
                Console.WriteLine("------ 2. Measure ------");
                Console.WriteLine("\nActuals:");
                GetSystemInfo();
                
                Console.WriteLine();
                Console.WriteLine("Averages:");
                double[] avarageArray = Avarage(data2);
                int db = 1;
                foreach (double item in avarageArray)
                {
                    Console.WriteLine("CPU Core #" + db + ": " + item + "°");
                    db++;
                }
                Console.WriteLine("\n--> Press ENTER to finish.");
                Thread.Sleep(2000);
            }
        }
    }
}
