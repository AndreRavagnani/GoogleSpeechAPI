using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1Beta1;
using NAudio.Wave;
using System.IO;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using Google.LongRunning;

namespace GoogleSpechAPI
{
    class Program
    {

        static WaveFileWriter waveFile;
        static void Main(string[] args)
        {

            String envName = "GOOGLE_APPLICATION_CREDENTIALS";
            String envValue = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Remove(0, 6) + "\\token.json";

            //Stream stream = new FileStream(envValue, FileMode.Open, FileAccess.Read, FileShare.Read);

            //var credential = GoogleCredential.FromStream(stream);

            // Determine whether the environment variable exists.
            //if (Environment.GetEnvironmentVariable(envName) == null)
            //    // If it doesn't exist, create it.
            //    Environment.SetEnvironmentVariable(envName, envValue);

            //WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(0);
            Console.WriteLine("Gravando...");
            WaveInEvent waveSource = new WaveInEvent();
            //waveSource.DeviceNumber = 0;
            waveSource.WaveFormat = new WaveFormat(16000, 1);

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);

            MemoryStream tempFile = new MemoryStream();
            waveFile = new WaveFileWriter(tempFile, waveSource.WaveFormat);
            waveSource.StartRecording();

            Console.WriteLine("Aperte Enter para parar de gravar e enviar para reconhecimento");
            Console.ReadLine();
            waveSource.StopRecording();

            tempFile.Position = 0;


            var speech = SpeechClient.Create();



            //var response = speech.StreamingRecognize();

            // Console.ReadLine();
            long controlLen = tempFile.Length;
            long position = 0;



            DateTime dt = DateTime.Now;

            //while ((DateTime.Now - dt).Minutes < 1)
            {
                position = tempFile.Length - controlLen;
                controlLen = tempFile.Length;
                tempFile.Position = position;
                try
                {
                    //Console.WriteLine("Reconhecendo...");
                    //var t = speech.AsyncRecognize(new RecognitionConfig()
                    //{
                    //    Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                    //    SampleRate = 16000,
                    //    LanguageCode = "pt-BR"
                    //}, RecognitionAudio.FromStream(tempFile));


                    //Console.WriteLine("{0}", t.PollUntilCompleted().Result);
                    Console.WriteLine("Reconhecendo...");
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
                }
                finally
                {
                    Console.WriteLine("Reconhecido em {0}", DateTime.Now - dt);
                    dt = DateTime.Now;

                }
            }



            waveFile.Dispose();
            // Console.WriteLine("Reconhecimento Completo. Tempo={0}", DateTime.Now - dt);
            Console.WriteLine("Terminou");
            Console.ReadKey();
        }

        static void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            waveFile.WriteData(e.Buffer, 0, e.BytesRecorded);
        }

    }
}
