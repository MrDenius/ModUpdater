using ModUpdater.Jsons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ModUpdater
{
    class Program
    {
        static string Root = $"{Environment.CurrentDirectory}";

        static JavaScriptSerializer serializer = new JavaScriptSerializer();

        static void Main(string[] args)
        {
            Init();
            Logo();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    switch (args[i].Replace("--", string.Empty))
                    {
                        case "upload-mods":
                            Mods mods = new Mods(new List<Mod>());

                            foreach (string mod in GetModList())
                            {
                                mods.mods.Add(new Mod(mod));
                            }

                            //Zip.CompressZip("Mods\\", "Temp\\Mods.rar");


                            WriteAPI($"new-mods", JsonConvert.SerializeObject(mods));

                            //mods.UploadMods("Temp\\Mods.rar");
                            mods.UploadMods("Mods\\");


                            break;
                        case "upload-update":

                            ProgramConfig ProgC = new ProgramConfig(Config.BuildVersion.ToString());

                            //TODO: Файлы которые будут скачиватся при обновлении
                            List<FileInfo> filestodownload = new List<FileInfo>()
                            {
                                new FileInfo("ModUpdater.exe"),
                                new FileInfo("Newtonsoft.Json.dll"),
                                new FileInfo("Update.exe")
                            };

                            ProgC.NewFiles = filestodownload;

                            WriteAPI("new-config", JsonConvert.SerializeObject(ProgC));

                            if (Directory.Exists("Temp\\NewUpdate\\"))
                                Directory.Delete("Temp\\NewUpdate\\", true);
                            Directory.CreateDirectory("Temp\\NewUpdate\\");


                            foreach (FileInfo fi in filestodownload)
                            {
                                fi.CopyTo($"Temp\\NewUpdate\\{fi.Name}");
                            }

                            Zip.CompressZip("Temp\\NewUpdate\\", "Temp\\Update.zip");

                            ProgramConfig.UploadUpdate("Temp\\Update.zip");

                            Thread.Sleep(1500);

                            break;
                    }
                }
            }
            if (args.Length != 0)
                Exit();

            Update();
            UpdateMods();
            Exit();
        }

        static void Init()
        {
            //Exist Dir
            List<DirectoryInfo> Dirs = new List<DirectoryInfo>()
            {
                new DirectoryInfo("Temp\\"),
                new DirectoryInfo("Mods\\")
            };
            foreach (DirectoryInfo dir in Dirs)
            {
                if (!dir.Exists)
                    dir.Create();
            }

            Task.Run(BeautifulTitle);
        }

        static void Logo()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine($"ModUpdater by Denius|Supported by HornetAPI\nBuild {Config.BuildVersion}");

            Console.ForegroundColor = ConsoleColor.Green;

            Thread.Sleep(1000);
        }

        static void BeautifulTitle()
        {
            string prcName = Process.GetCurrentProcess().ProcessName;
            while (true)
            {
                var counter = new PerformanceCounter("Process", "Working Set - Private", prcName);

                Console.Title = $"ModUpdater Build:{Config.BuildVersion}. Mem: {Math.Round((double)counter.RawValue / 1024 / 1024, 2)} MB";

                Thread.Sleep(1000 / 1);
            }
        }

        static void UnZipModsAndSetup()
        {
            if (Directory.Exists("Mods\\"))
                Directory.Delete("Mods\\", true);

            Zip.UnZip($"Temp\\NewMods.rar", "Mods\\");
        }

        static void UpdateMods()
        {

            Mods mods;
            using (StreamReader sr = ReadAPI("type=mods"))
                mods = JsonConvert.DeserializeObject<Mods>(sr.ReadToEnd());

            List<FileInfo> mm = new List<FileInfo>();
            foreach (Mod m in mods.mods)
            {
                mm.Add(new FileInfo(m.name));
            }

            FileInfo[] modlistN = mm.ToArray();
            FileInfo[] modlist = GetAllFiles(new DirectoryInfo("Mods\\"));


            bool ModsAllRight = false;

            if (modlistN.Length == modlist.Length)
            {
                foreach (FileInfo modN in modlistN)
                {
                    foreach (FileInfo mod in modlist)
                    {
                        if (mod.Name == modN.Name)
                        {
                            ModsAllRight = true;
                            break;
                        }
                    }
                }
            }

            if (ModsAllRight)
            {
                Console.WriteLine("У вас все моды в порядке. Вы хлтите продолжить?\n" +
                                    "Press y or n.");
                while (true)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.N)
                        Exit();
                    else
                        if (key == ConsoleKey.Y)
                        break;
                }
            }





            Console.WriteLine("Скачивание модов");

            using (StreamWriter sw = new StreamWriter("Mods.log", false, Encoding.Default))
            {
                sw.WriteLine($"Mods Updater Build {Config.BuildVersion}");
                sw.WriteLine($"NewMods Length: {mods.mods.Count}");
                sw.WriteLine($"Mods Length: {modlist.Length}");
                sw.WriteLine($"========================================");
                foreach (Mod mod in mods.mods)
                {
                    sw.WriteLine(mod.name);
                }
            }

            if (modlistN.Length - modlist.Length < 0)
            {
                foreach (FileInfo mod in modlist)
                {
                    bool Remove = true;
                    foreach (FileInfo modN in modlistN)
                    {
                        if (mod.Name == modN.Name)
                        {
                            Remove = false;
                            break;
                        }
                    }
                    if (Remove)
                        mod.Delete();
                }
            }
            if (modlistN.Length - modlist.Length > 0)
            {
                foreach (FileInfo modN in modlistN)
                {
                    bool NewMod = true;
                    foreach (FileInfo mod in modlist)
                    {
                        if (mod.Name == modN.Name)
                        {
                            NewMod = false;
                            break;
                        }
                    }
                    if (NewMod)
                    {
                        using (StreamReader sr = ReadAPI($"file=Mods.rar&modname={modN}", false))
                        {
                            int bi;
                            using (Stream s = new FileStream($"Temp\\{modN}.tmp", FileMode.Create, FileAccess.Write))
                            {
                                while ((bi = sr.BaseStream.ReadByte()) != -1)
                                {
                                    s.WriteByte((byte)bi);
                                }
                            }
                        }
                        File.Move($"Temp\\{modN}.tmp", $"Mods\\{modN}");
                    }

                }
            }

            Console.WriteLine("Готово!");
            Thread.Sleep(2000);
            Exit();
        }

        static void Update()
        {
            ProgramConfig programConfig;
            using (StreamReader sr = ReadAPI("type=config"))
                programConfig = JsonConvert.DeserializeObject<ProgramConfig>(sr.ReadToEnd());

            if (Convert.ToInt32(programConfig.NewBuildVersion) != Config.BuildVersion)
            {
                Console.WriteLine("Обновление!!!");

                using (StreamReader sr = ReadAPI($"file=Update.zip", false))
                {
                    int bi;
                    using (Stream s = new FileStream($"Temp\\Update.zip", FileMode.Create, FileAccess.Write))
                    {
                        while ((bi = sr.BaseStream.ReadByte()) != -1)
                        {
                            s.WriteByte((byte)bi);
                        }
                    }
                }

                if (Directory.Exists("Temp\\Update\\"))
                    Directory.Delete("Temp\\Update\\", true);

                Zip.UnZip($"Temp\\Update.zip", $"Temp\\Update\\");

                Thread.Sleep(1500);

                Process.Start("Update.exe");

                Environment.Exit(0);
            }

        }

        static string[] GetModList()
        {
            List<string> ret = new List<string>();
            if (Directory.Exists("Mods\\"))
            {
                foreach (FileInfo f in GetAllFiles(new DirectoryInfo("Mods\\")))
                {
                    ret.Add(Path.GetFileName(f.Name));
                }

            }
            return ret.ToArray();
        }

        static FileInfo[] GetAllFiles(DirectoryInfo directoryInfo)
        {
            List<FileInfo> files = new List<FileInfo>();
            foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
            {
                files.AddRange(GetAllFiles(dir));
            }
            foreach (FileInfo f in directoryInfo.GetFiles())
            {
                files.Add(f);
            }

            return files.ToArray();
        }

        static StreamReader ReadAPI(string args, bool ToStr = true)
        {
            Debug.WriteLine($"Start Read::{args}");
            string pathToRet = null;
            Random rand = new Random();
            while (pathToRet == null || !Directory.Exists(Path.GetDirectoryName(pathToRet)) || File.Exists(pathToRet))
            {
                pathToRet = $"Temp\\{rand.Next(0, 100000)}.tmp";
            }




            WebRequest request;

            request = WebRequest.Create($"{Config.UrlToAPI}/?{args}");




            request.Timeout = 10000;
            WebResponse response = request.GetResponse();


            int i = 0;
            int bi = 0;
            bool SWStop = false;
            Task StatusWriter = new Task(new Action(() =>
            {
                Console.CursorVisible = false;
                while (!SWStop)
                {
                    Thread.Sleep(50);
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"Downloading [{args}] {Math.Round((double)i / 1024, 2)} KB /{Math.Round((double)response.ContentLength / 1024, 2)} KB{new string(' ', 10)}");
                }
                Console.WriteLine();
                Console.CursorVisible = true;
            }));

            StatusWriter.Start();

            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    using (Stream s = new FileStream(pathToRet, FileMode.Create, FileAccess.Write))
                    {
                        while ((bi = stream.ReadByte()) != -1)
                        {
                            s.WriteByte(Convert.ToByte(bi));
                            i++;
                        }
                        SWStop = true;
                        StatusWriter.Wait();
                    }
                }
            }
            response.Close();
            request.Abort();

            if (ToStr)
                File.WriteAllText(pathToRet, Encoding.UTF8.GetString(File.ReadAllBytes(pathToRet)));

            return new StreamReader(pathToRet, Encoding.Default);
        }

        static void WriteAPI(string type, string data)
        {
            NewData nd = new NewData(type, data);
            string dataw = JsonConvert.SerializeObject(nd);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{Config.UrlToAPI}");
            request.Method = "POST";
            byte[] byteArray = Encoding.UTF8.GetBytes(dataw);
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;


            int i = 0;
            bool SWStop = false;

            Task StatusWriter = new Task(new Action(() =>
            {
                Console.CursorVisible = false;
                while (!SWStop)
                {
                    Thread.Sleep(50);
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"Uploading [{type}] {Math.Round((double)i / 1024, 2)} KB /{Math.Round((double)byteArray.Length / 1024, 2)} KB{new string(' ', 10)}");
                }
                Console.WriteLine();
                Console.CursorVisible = true;
            }));

            using (Stream dataStream = request.GetRequestStream())
            {
                {
                    StatusWriter.Start();
                    while (i < byteArray.Length)
                    {
                        dataStream.WriteByte(byteArray[i]);
                        i++;
                    }
                    SWStop = true;
                    StatusWriter.Wait();
                }
            }
            request.Abort();
        }



        static void Exit()
        {
            foreach (FileInfo tf in new DirectoryInfo("Temp\\").GetFiles())
            {
                if (tf.Extension == ".tmp")
                    tf.Delete();
            }

            Environment.Exit(0);
        }
    }
}
