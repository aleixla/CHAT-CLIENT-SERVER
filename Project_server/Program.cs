using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Project_server
{
    public  class Program
    {
        public static async Task Main(string[] args)
        {
            string serverIP = "192.168.3.232";
            int serverPort = 5555;

            TcpListener listener = new TcpListener(IPAddress.Parse(serverIP), serverPort);
            listener.Start();
            Console.WriteLine("Server started. Waiting for incoming connections...");

            List<TcpClient> clients = new List<TcpClient>();

            while (true)
            {
                
                TcpClient client = listener.AcceptTcpClient();
                clients.Add(client);
                NetworkStream stream = client.GetStream();

                Console.WriteLine("New client connected.");

                while (true)
                {
                    byte[] receiveData = new byte[1024];
                    int bytesRead = stream.Read(receiveData, 0, receiveData.Length);
                    string clientMessage = Encoding.ASCII.GetString(receiveData, 0, bytesRead);

                    Console.WriteLine($"Message received from client: {clientMessage}");

                    if (clientMessage.ToLower() == "exit")
                    {
                        clients.Remove(client);
                        client.Close();
                        break; 
                    }

                    byte[] sendData = Encoding.ASCII.GetBytes(clientMessage);

                    foreach (TcpClient otherClient in clients)
                    {
                        if (otherClient != client)
                        {
                            otherClient.GetStream().Write(sendData, 0, sendData.Length);
                            Console.WriteLine("Message sent to the other client");
                        }
                    }
                    
                    
                }
                while (true)
                {
                    byte[] receiveData = new byte[1024];
                    int bytesRead = stream.Read(receiveData, 0, receiveData.Length);
                    string clientMessage = Encoding.ASCII.GetString(receiveData, 0, bytesRead);
                    
                    Console.WriteLine($"Message received from client: {clientMessage}");
                    
                    if (clientMessage.ToLower() == "exit")
                    {
                        clients.Remove(client);
                        client.Close();
                        break; 
                    }
                    
                    byte[] sendData = Encoding.ASCII.GetBytes(clientMessage);
                    foreach (TcpClient otherClient in clients)
                    {
                        if (otherClient != client)
                        {
                            otherClient.GetStream().Write(sendData, 0, sendData.Length);
                            Console.WriteLine("Message sent to the other client");
                        }
                    }

                }
            }
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }
    }
}

/* il server continuerà ad accettare nuove connessioni client all'interno del primo ciclo while,
 mentre all'interno del secondo ciclo while verranno gestiti i messaggi ricevuti dai client.
 Quando un client invia il messaggio "exit", 
 la connessione con quel client verrà chiusa e il server tornerà al primo ciclo while per accettare nuove connessioni.*/