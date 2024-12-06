using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static IPAddress ServerIPAddress;
        static int ServerPort;

        static string ClientToken;
        static DateTime ClientDateConnection;

        static void Main(string[] args)
        {
            OnSettings();

            Thread tCheckToken = new Thread(CheckToken);
            tCheckToken.Start();
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
                sr.Close();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"IP-адрес сервера: {ServerIPAddress.ToString()}; \nПорт сервера: {ServerPort};");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Укажите свой IP-адрес: ");
                ServerIPAddress = IPAddress.Parse(Console.ReadLine());
                Console.Write($"Укажите порт: ");
                ServerPort = int.Parse(Console.ReadLine());
                StreamWriter sw = new StreamWriter(Path);
                sw.WriteLine(ServerIPAddress.ToString());
                sw.WriteLine(ServerPort);
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
                case "/connect": AuthenticateUser(); break;
                case "/status": GetStatus(); break;
                case "/help": Help(); break;
            }
        }

        static void AuthenticateUser()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Введите имя пользователя: ");
            string username = Console.ReadLine();
            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();
            ConnectServer(username, password);
        }

        static void ConnectServer(string username, string password)
        {
            IPEndPoint EndPoint = new IPEndPoint(ServerIPAddress, ServerPort);
            Socket Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Socket.Connect(EndPoint);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка: " + ex.Message);
                return;
            }
            if (Socket.Connected)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                
                string authCommand = $"/auth {username} {password}";
                Socket.Send(Encoding.UTF8.GetBytes(authCommand));
                byte[] bytes = new byte[10485760];
                int byteRec = Socket.Receive(bytes);
                string Response = Encoding.UTF8.GetString(bytes, 0, byteRec);
                if (Response == "/auth_failed")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Не удалось выполнить аутентификацию. Неверное имя пользователя или пароль");
                }
                else if (Response == "/blacklist")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Вы внесены в черный список. Невозможно подключиться.");
                }
                else
                {
                    ClientToken = Response;
                    ClientDateConnection = DateTime.Now;
                    Console.WriteLine("Подключение к серверу выполнено успешно!");
                    Console.WriteLine($"Токен подключения: {ClientToken}");
                }
            }
        }

        static void CheckToken()
        {
            while (true)
            {
                if (!String.IsNullOrEmpty(ClientToken))
                {
                    IPEndPoint EndPoint = new IPEndPoint(ServerIPAddress, ServerPort);
                    Socket Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        Socket.Connect(EndPoint);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ошибка: " + ex.Message);
                    }
                    if (Socket.Connected)
                    {
                        Socket.Send(Encoding.UTF8.GetBytes(ClientToken));
                        byte[] bytes = new byte[10485760];
                        int byteRec = Socket.Receive(bytes);
                        string Response = Encoding.UTF8.GetString(bytes, 0, byteRec);
                        if (Response == "/disconnect")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Клиент отключен от сервера");
                            ClientToken = String.Empty;
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        static void GetStatus()
        {
            int Duration = (int)DateTime.Now.Subtract(ClientDateConnection).TotalSeconds;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Клиент: {ClientToken}, время подключения: {ClientDateConnection.ToString("HH:mm:ss dd.MM")}, продолжительность: {Duration}");
        }

        static void Help()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Команда в сторону сервера: ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/config");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" - начальные настройки");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/connect");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" - подключение к серверу");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/status");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" - показать список пользователей");
        }
    }
}