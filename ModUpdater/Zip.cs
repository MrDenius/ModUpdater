using System;
using System.IO;
using System.IO.Compression;

namespace ModUpdater
{
    public static class Zip
    {
        public static void UnZip(string pathToFile, string pathToDirSave)
        {
            if (!File.Exists(pathToFile))
            {
                Console.WriteLine("Указанный файл не найден.");
                return;
            }

            Console.WriteLine("Начинаем распаковку...");

            ZipFile.ExtractToDirectory(pathToFile, pathToDirSave);

            Console.WriteLine("Готово!");
        }

        public static void CompressZip(string pathToDir, string pathToFile)
        {
            if (!Directory.Exists(pathToDir))
            {
                Console.WriteLine("Указанный файл не найден.");
                return;
            }

            Console.WriteLine("Начинаем запаковку...");

            if (File.Exists(pathToFile))
                File.Delete(pathToFile);

            ZipFile.CreateFromDirectory(pathToDir, pathToFile);

            Console.WriteLine("Готово!");
        }
    }
}
