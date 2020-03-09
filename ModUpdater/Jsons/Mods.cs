using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ModUpdater.Jsons
{
    public struct Mods
    {
        public List<Mod> mods;

        public Mods(List<Mod> mods)
        {
            this.mods = mods;
        }

        public void UploadMods(string PathToDir)
        {
            HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create($"{Config.UrlToAPI}?dir=Mods");
            request1.Method = "DELETE";
            //request1.ContentType = "application/octet-stream";
            //request1.ContentLength = 0;
            HttpWebResponse response1 = (HttpWebResponse)request1.GetResponse();
            if (response1.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("OK del");
            }
            else
            {
                Console.WriteLine($"Error del: {response1.StatusCode}");
            }
            request1.Abort();

            foreach (FileInfo fi in new DirectoryInfo(PathToDir).GetFiles())
            {
                string PathToFile = fi.FullName;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{Config.UrlToAPI}?dir=Mods&file={Path.GetFileName(PathToFile)}");
                request.Method = "POST";
                byte[] byteArray = File.ReadAllBytes(PathToFile);
                switch (fi.Extension)
                {
                    case ".zip": request.ContentType = "application/x-zip-compressed"; request.MediaType = "application/x-zip-compressed"; request.Accept = "application/x-zip-compressed"; break;
                    case ".jar": request.ContentType = "application/java-archive"; request.MediaType = "application/java-archive"; request.Accept = "application/java-archive"; break;
                    default: request.ContentType = "application/x-www-form-urlencoded"; request.MediaType = "application/x-www-form-urlencoded"; request.Accept = "application/x-www-form-urlencoded"; break;
                }
                request.ContentLength = byteArray.Length;

                request.Timeout = 240000;


                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    Console.WriteLine($"Uploading [{Path.GetFileName(PathToFile)}]{Math.Round((double)byteArray.Length / 1024, 2)} KB");

                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("OK");
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
                request.Abort();
            }
        }
    }

    public struct Mod
    {
        public string name;

        public Mod(string name)
        {
            this.name = name;
        }
    }
}
