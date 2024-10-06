using System;
using System.IO;
using System.Threading;

class Program
{
    static Mutex mutexA = new Mutex();
    static Mutex mutexB = new Mutex();
    static Mutex mutexC = new Mutex();

    static void Main()
    {
        Thread threadA = new Thread(() => ReplaceCharactersInFiles("catA", 'A'));
        Thread threadB = new Thread(() => ReplaceCharactersInFiles("catB", 'B'));
        Thread threadC = new Thread(() => ReplaceCharactersInFiles("catC", 'C'));

        threadA.Start();
        threadB.Start();
        threadC.Start();

        threadA.Join();
        threadB.Join();
        threadC.Join();

        Console.WriteLine("Замена символов завершена.");
        Console.ReadLine();
    }

    static void ReplaceCharactersInFiles(string directoryPath, char character)
    {
        string[] files = Directory.GetFiles(directoryPath);

        foreach (string filePath in files)
        {
            if (File.Exists(filePath) && IsFileUnlocked(filePath))
            {
                if (character == 'A')
                    mutexA.WaitOne();
                else if (character == 'B')
                    mutexB.WaitOne();
                else if (character == 'C')
                    mutexC.WaitOne();

                try
                {
                    string fileName = Path.GetFileName(filePath);
                    string newFileName = fileName.Replace(character, '_');

                    string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);
                    File.Move(filePath, newFilePath);

                    Console.WriteLine($"Заменены символы в файле {filePath}");
                }
                finally
                {
                    if (character == 'A')
                        mutexA.ReleaseMutex();
                    else if (character == 'B')
                        mutexB.ReleaseMutex();
                    else if (character == 'C')
                        mutexC.ReleaseMutex();
                }
            }
        }
    }

    static bool IsFileUnlocked(string filePath)
    {
        try
        {
            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                stream.Close();
            }
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }
}