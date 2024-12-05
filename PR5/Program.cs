using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace PR5
{
    internal class Program
    {
        static IPAddress ServerIPAddress;
        static int ServerPort;
        static int MaxClient;
        static int Duration;
        static List<Client> AllClients = new List<Client>();
        static Context dbContext;

        static void Main(string[] args)
        {
            OnSettings();
        }

        static void OnSettings()
        {
            string Path = Directory.GetCurrentDirectory() + "/.config";
            if (File.Exists(Path))
            {
                StreamReader sr = new StreamReader(Path);
                ServerIPAddress = IPAddress.Parse(sr.ReadLine());
                ServerPort = int.Parse(sr.ReadLine());
                MaxClient = int.Parse(sr.ReadLine());
                Duration = int.Parse(sr.ReadLine());
                sr.Close();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Server IP-address: {ServerIPAddress.ToString()};\nServer port: {ServerPort};\nMax client: {MaxClient};\nDuration: {Duration};");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Укажите ip-адрес: ");
                ServerIPAddress = IPAddress.Parse(Console.ReadLine());
                Console.Write($"Укажите порт: ");
                ServerPort = int.Parse(Console.ReadLine());
                Console.Write($"Укажите максимальное количество клиентов: ");
                MaxClient = int.Parse(Console.ReadLine());
                Console.Write($"Укажите срок действия лицензии: ");
                Duration = int.Parse(Console.ReadLine());
                StreamWriter sw = new StreamWriter(Path);
                sw.WriteLine(ServerIPAddress.ToString());
                sw.WriteLine(ServerPort);
                sw.WriteLine(MaxClient);
                sw.WriteLine(Duration);
                sw.Close();
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Чтобы изменить, введите команду: /config");
        }
    }
}