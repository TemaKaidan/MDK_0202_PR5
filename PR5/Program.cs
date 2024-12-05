﻿using System;
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
            while (true) SetCommand();
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
                Console.Write($"Please, provide the IP-address: ");
                ServerIPAddress = IPAddress.Parse(Console.ReadLine());
                Console.Write($"Please, specify the port: ");
                ServerPort = int.Parse(Console.ReadLine());
                Console.Write($"Please, specify the maximum number of clients: ");
                MaxClient = int.Parse(Console.ReadLine());
                Console.Write($"Please, specify the duration of the license: ");
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

        static void SetCommand()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            string Command = Console.ReadLine();
            switch (Command)
            {
                case "/config": File.Delete(Directory.GetCurrentDirectory() + "/.config"); OnSettings(); break;
                case "/status": GetStatus(); break;
                case "/help": Help(); break;
                case "/add_to_blacklist": AddToBlacklist(); break;
                case "/remove_from_blacklist": RemoveFromBlacklist(); break;
                case "/blacklist": dbContext.ShowBlacklist(); break;
                default: if (Command.Contains("/disconnect")) DisconnectServer(Command); break;
            }
        }

        static void GetStatus()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Count clients: {AllClients.Count}");
            foreach (var client in AllClients)
            {
                int Duration = (int)DateTime.Now.Subtract(client.DateConnect).TotalSeconds;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Client: {client.Token}, time connection: {client.DateConnect.ToString("HH:mm:ss dd.MM")}, duration: {Duration}");
            }
        }

        static void Help()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Command to the clients: ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/config");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  - set initial settings");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/disconnect");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" - disconnect users from server");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/status");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  - show list users");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/add_to_blacklist");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" - add user to blacklist");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/remove_from_blacklist");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" - remove user from blacklist");
        }

        static void AddToBlacklist()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter username to add to blacklist: ");
            string username = Console.ReadLine();
            dbContext.AddToBlacklist(username);
            var client = AllClients.FirstOrDefault(c => c.Username == username);
            if (client != null)
            {
                AllClients.Remove(client);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Client {client.Token} disconnected due to being added to blacklist.");
            }
        }
    }
}