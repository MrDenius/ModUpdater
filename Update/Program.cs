using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Update
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Обновление");

            bool Hren = true;
            Task.Run(new Action(() =>
            {
                while (Hren)
                {
                    Console.Write(".");
                    Task.Delay(1000).Wait();
                }
            }));

            if (Directory.Exists("Temp\\"))
            {
                Directory.CreateDirectory("Temp\\");
            }
            if (args.Length != 0)
            {
                foreach (FileInfo fi in new DirectoryInfo($"{args[0]}\\Temp\\Update\\").GetFiles())
                {
                    File.WriteAllBytes($"{args[0]}\\{fi.Name}", File.ReadAllBytes(fi.FullName));
                }
                Directory.Delete("Temp\\Update\\", true);
            }
            else
            {
                if (Directory.Exists("Temp\\Update\\"))
                {
                    if (File.Exists("Temp\\Update.exe"))
                        File.Delete("Temp\\Update.exe");
                    File.Copy("Update.exe", "Temp\\Update.exe");
                    Process.Start("Temp\\Update.exe", $"\"{new DirectoryInfo(Directory.GetCurrentDirectory()).FullName}\"");
                }
                else
                {
                    Hren = false;
                    Console.WriteLine("Проблем!! Дениска оказывается дурачёк((. Кривое обновление, Темп\\Апдейт нету!");
                    Console.ReadKey();
                }
            }
            Environment.Exit(0);
        }
    }
}
