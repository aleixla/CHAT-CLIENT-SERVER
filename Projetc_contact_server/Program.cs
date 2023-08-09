using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Projetc_contact_server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Server server = new Server("192.168.3.232", 5555);
            string start = Console.ReadLine();
            server.HandleClient(start);
            Console.WriteLine("Connected to the server.");
            
        }
    }
    public class Server
    {
            public string ServerIp { get; set; }
            public int ServerPort { get; set; }
            public List<TcpClient> ConnectedClients { get; set; }

            public Server(string serverIP, int serverPort)
            {
                ServerIp = serverIP;
                ServerPort = serverPort;
                ConnectedClients = new List<TcpClient>();

                TcpListener listener = new TcpListener(IPAddress.Parse(serverIP), serverPort);
                listener.Start();
                Console.WriteLine("Server started. Waiting for incoming connections...");
                
                // Inizia ad ascoltare le connessioni in entrata
                while(true)
                {
                    TcpClient client = listener.AcceptTcpClient();

                    // Aggiungi il client alla lista di client connessi
                    ConnectedClients.Add(client);

                    // Gestisci il client in un thread separato
                    Thread clientThread = new Thread(HandleClient);
                    clientThread.Start(client);
                }
            }
            public void HandleClient(object obj)
            {
                TcpClient client = (TcpClient)obj;
                NetworkStream stream = client.GetStream();

                bool isRunning = true;
                while (isRunning)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
        
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine(dataReceived);

                    if (dataReceived.ToLower() == "exit")
                    {
                        isRunning = false;
                    }
                    else
                    {
                        // Verifica che l'oggetto JSON ricevuto sia valido
                        MyObject myObject;
                        if (TryDeserializeJson<MyObject>(dataReceived, out myObject))
                        {
                            Console.WriteLine($"Received MyObject: {myObject.Name}, {myObject.Surname}, {myObject.PhoneNumber},{myObject.Note}");

                            // Invia il messaggio ricevuto a tutti i client connessi tranne quello che ha inviato il messaggio
                            foreach (TcpClient connectedClient in ConnectedClients)
                            {
                                if (connectedClient != client)
                                {
                                    NetworkStream connectedStream = connectedClient.GetStream();
                                    byte[] responseBuffer = Encoding.ASCII.GetBytes(dataReceived);
                                    connectedStream.Write(responseBuffer, 0, responseBuffer.Length);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Received data is not a valid MyObject JSON.");
                        }
                    }
                }
             
            }

            private bool TryDeserializeJson<T>(string json, out T result)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<T>(json);
                    return true;
                }
                catch
                {
                    result = default(T);
                    return false;
                }
            }
    }

    public class MyObject
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public string Note { get; set; }
    }


}


