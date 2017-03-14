using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1Beta1;
using NAudio.Wave;
using System.IO;

namespace GoogleSpechAPI
{
    class Program
    {
        static WaveFileWriter waveFile;
        static void Main(string[] args)
        {
            //WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(0);
            Console.WriteLine("Now recording...");
            WaveInEvent waveSource = new WaveInEvent();
            //waveSource.DeviceNumber = 0;
            waveSource.WaveFormat = new WaveFormat(16000, 1);

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);

            MemoryStream tempFile = new MemoryStream();
            waveFile = new WaveFileWriter(tempFile, waveSource.WaveFormat);
            waveSource.StartRecording();

            Console.WriteLine("Press enter to stop");
            Console.ReadLine();
            waveSource.StopRecording();

            tempFile.Position = 0;


            var speech = SpeechClient.Create();



            //var response = speech.StreamingRecognize();

            // Console.ReadLine();

            var response = speech.SyncRecognize(new RecognitionConfig()
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRate = 16000,
                LanguageCode = "pt-BR",
            }, RecognitionAudio.FromStream(tempFile));
            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    Console.WriteLine(alternative.Transcript);
                }
            }

            waveFile.Dispose();
            Console.WriteLine("Terminou");
            Console.ReadKey();
        }

        static void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            waveFile.WriteData(e.Buffer, 0, e.BytesRecorded);
        }

    }
}
