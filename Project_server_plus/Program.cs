using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Project_server_plus
{
    
    public  class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Press enter for server start");
            string start = Console.ReadLine();
            Server server = new Server("192.168.3.232",5555);
            server.HandleClient(start);
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

            // Esempio di lettura dei dati inviati dal client
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine(dataReceived);

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

            // Chiudi la connessione con il client
            // client.Close();
        }
    }
}