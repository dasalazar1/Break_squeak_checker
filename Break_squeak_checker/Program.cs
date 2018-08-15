using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NAudio.Wave;
using System.Threading;


namespace Break_squeak_checker
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            List<string> devices = new List<string>();
            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                devices.Add(WaveIn.GetCapabilities(n).ProductName);
            }

            for (int n = 0; n < devices.Count; n++)
            {
                Console.WriteLine(n + " : " + devices[n]);
            }

            WheelWells ww = new WheelWells();

            DisplayWriter.DrawScreen();
            Thread.Sleep(1000000);
        }
    }

    public class WheelWells
    {
        VolCheck driverFront;
        VolCheck driverRear;
        VolCheck passengerFront;
        VolCheck passengerRear;
        int deviceCount = WaveIn.DeviceCount;

        public WheelWells()
        {
            assignMics();
            driverFront.StartMetering();
            driverRear.StartMetering();
            passengerFront.StartMetering();
            passengerRear.StartMetering();
        }

        private void assignMics()
        {
            string userInput = string.Empty;
            int deviceInput = -1;

            Console.WriteLine("Mic for driver front well: ");
            userInput = Console.ReadLine();
            // Additional logic needed. Parse fail will be 0 but there is a mic
            // in this laptop so 0 is always valid.
            int.TryParse(userInput, out deviceInput); 
            driverFront = new VolCheck(deviceInput, "DF");

            Console.WriteLine("Mic for driver rear well: ");
            userInput = Console.ReadLine();
            int.TryParse(userInput, out deviceInput);
            driverRear = new VolCheck(deviceInput, "DR");

            Console.WriteLine("Mic for passenger front well: ");
            userInput = Console.ReadLine();
            int.TryParse(userInput, out deviceInput);
            passengerFront = new VolCheck(deviceInput, "PF");

            Console.WriteLine("Mic for passenger rear well: ");
            userInput = Console.ReadLine();
            int.TryParse(userInput, out deviceInput);
            passengerRear = new VolCheck(deviceInput, "PR");

            //wire up the writing
            driverFront.displayVolume += DisplayWriter.WriteToConsole;
            driverRear.displayVolume += DisplayWriter.WriteToConsole;
            passengerFront.displayVolume += DisplayWriter.WriteToConsole;
            passengerRear.displayVolume += DisplayWriter.WriteToConsole;

        }
    }

    public class VolCheck
    {
        float maxValue = 0f;
        float minValue = 0f;
        int sampleCount = 0;
        string position;
        WaveInEvent waveIn;

        public EventHandler<VolArgs> displayVolume;

        public VolCheck(int devNum, string pos)
        {
            waveIn = new WaveInEvent();
            waveIn.DeviceNumber = devNum;
            waveIn.WaveFormat = new WaveFormat(44100, 1);
            waveIn.DataAvailable += OnDataAvailable;
            position = pos;
            //waveIn.StartRecording();
        }

        public void StartMetering()
        {
            waveIn.StartRecording();
        }

        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;

            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((buffer[index + 1] << 8) |
                                        buffer[index + 0]);
                float sample32 = sample / 32768f;
                maxValue = Math.Max(maxValue, sample32);
                minValue = Math.Min(minValue, sample32);
                sampleCount++;
            }

            if (sampleCount > 100)
            {
                VolArgs va = new VolArgs(DisplayWriter.VolMeter(Math.Max(maxValue, Math.Abs(minValue))), position);
                sampleCount = 0;
                maxValue = 0f;
                minValue = 0f;

                displayVolume(this, va);
            }
        }


    }

    static public class DisplayWriter
    {

        static readonly Dictionary<string, int> wells = new Dictionary<string, int>
        {
            {"DF", 1 },
            {"DR", 3 },
            {"PF", 5 },
            {"PR", 7 }
        };

        public static void WriteToConsole(object sender, VolArgs e)
        {
            Console.SetCursorPosition(0, wells[e.Position]);
            Console.Write(new String(' ', 10));
            Console.SetCursorPosition(0, wells[e.Position]);
            Console.Write("\r" + e.Message);
        }

        public static void DrawScreen()
        {
            Console.Clear();
            //Console.WriteLine("DF:           PF: ");
            //Console.WriteLine("DR:           PR: ");
            Console.WriteLine("DF:");
            Console.WriteLine("");
            Console.WriteLine("DR:");
            Console.WriteLine("");
            Console.WriteLine("PF:");
            Console.WriteLine("");
            Console.WriteLine("PR:");
        }


        public static string VolMeter(float rawVol)
        {
            int easyVol = (int)Math.Round(rawVol * 10, 0);
            return new string('#', easyVol);
        }
    }

    public class VolArgs : EventArgs
    {
        public VolArgs(string str, string pos)
        {
            msg = str;
            position = pos;
        }

        private string msg;
        private string position; 
        public string Message
        {
            get { return msg; }
        }
        public string Position
        {
            get { return position;  }
        }
    }

}
