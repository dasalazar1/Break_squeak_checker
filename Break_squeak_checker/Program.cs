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
            List<string> devices = new List<string>();
            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                devices.Add(WaveIn.GetCapabilities(n).ProductName);
            }

            Console.CursorVisible = false;

            VolCheck vc = new VolCheck(0,0);
            vc.displayVolume += DisplayWriter.WriteToConsole;

            VolCheck vc2 = new VolCheck(2, 1);
            vc2.displayVolume += DisplayWriter.WriteToConsole;

            Thread.Sleep(1000000);

        }
    }

    public class VolCheck
    {
        float maxValue = 0f;
        float minValue = 0f;
        int sampleCount = 0;
        int position = 0;
        WaveInEvent waveIn;

        public EventHandler<VolArgs> displayVolume;

        public VolCheck(int devNum, int pos)
        {
            waveIn = new WaveInEvent();
            waveIn.DeviceNumber = devNum;
            waveIn.WaveFormat = new WaveFormat(44100, 1);
            waveIn.DataAvailable += OnDataAvailable;
            position = pos;
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
                //sampleAggregator.Add(sample32);
                maxValue = Math.Max(maxValue, sample32);
                minValue = Math.Min(minValue, sample32);
                sampleCount++;
            }

            if (sampleCount > 100)
            {
                //Console.Write("\r{0}", Math.Max(maxValue, Math.Abs(minValue)) * 100);
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
        public static void WriteToConsole(object sender, VolArgs e)
        {
            Console.SetCursorPosition(0, e.Position);
            Console.Write(new String(' ', 10));
            Console.Write("\r" + e.Message);
        }

        public static string VolMeter(float rawVol)
        {
            int easyVol = (int)Math.Round(rawVol * 10, 0);
            return new string('#', easyVol);
        }
    }

    public class VolArgs : EventArgs
    {
        public VolArgs(string str, int pos)
        {
            msg = str;
            position = pos;
        }

        private string msg;
        private int position; 
        public string Message
        {
            get { return msg; }
        }
        public int Position
        {
            get { return position;  }
        }
    }

}
