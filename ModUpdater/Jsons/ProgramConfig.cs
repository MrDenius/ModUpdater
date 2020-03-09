using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ModUpdater.Jsons
{
    public class ProgramConfig
    {
        public string NewBuildVersion { get; set; }
        public List<FileInfo> NewFiles { get; set; }

        public ProgramConfig(string newBuildVersion)
        {
            NewBuildVersion = newBuildVersion;
        }

        public static void UploadUpdate(string PathToZip)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{Config.UrlToAPI}?name=Update.zip");
            request.Method = "POST";
            byte[] byteArray = File.ReadAllBytes(PathToZip);
            request.ContentType = "application/octet-stream";
            request.ContentLength = byteArray.Length;

            request.Timeout = 240000;

            int i = 0;
            bool SWStop = false;

            Task StatusWriter = new Task(new Action(() =>
            {
                while (!SWStop)
                {
                    Thread.Sleep(50);
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"Uploading [] {Math.Round((double)i / 1024, 2)} KB /{Math.Round((double)byteArray.Length / 1024, 2)} KB{new string(' ', 10)}");
                }
            }));

            using (Stream dataStream = request.GetRequestStream())
            {
                {
                    StatusWriter.Start();
                    while (i < byteArray.Length)
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        break;
                        dataStream.WriteByte(byteArray[i]);
                        i++;
                    }
                    Console.WriteLine();
                    SWStop = true;
                    StatusWriter.Wait();
                }
            }
            //request.GetResponse();
            request.Abort();
        }
    }
}
