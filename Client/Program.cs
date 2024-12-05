﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
                Console.WriteLine($"Server IP-address: {ServerIPAddress.ToString()}; \nServer port: {ServerPort};");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Please, provide the IP-address: ");
                ServerIPAddress = IPAddress.Parse(Console.ReadLine());
                Console.Write($"Please, specify the port: ");
                ServerPort = int.Parse(Console.ReadLine());
                StreamWriter sw = new StreamWriter(Path);
                sw.WriteLine(ServerIPAddress.ToString());
                sw.WriteLine(ServerPort);
                sw.Close();
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("To change, write the command: /config");
        }

        static void AuthenticateUser()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter your username: ");
            string username = Console.ReadLine();
            Console.Write("Enter your password: ");
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
                Console.WriteLine("Error: " + ex.Message);
                return;
            }
            if (Socket.Connected)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Connection to server successful");
                string authCommand = $"/auth {username} {password}";
                Socket.Send(Encoding.UTF8.GetBytes(authCommand));
                byte[] bytes = new byte[10485760];
                int byteRec = Socket.Receive(bytes);
                string Response = Encoding.UTF8.GetString(bytes, 0, byteRec);
                if (Response == "/auth_failed")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Authentication failed. Invalid username or password.");
                }
                else if (Response == "/blacklist")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("You are in the blacklist. Connection denied.");
                }
                else
                {
                    ClientToken = Response;
                    ClientDateConnection = DateTime.Now;
                    Console.WriteLine($"Received connection token: {ClientToken}");
                }
            }
        }


    }
}