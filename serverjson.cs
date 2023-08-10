using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Projetc_contact_server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Server server = new Server("192.168.3.232", 5555);
            Console.WriteLine("Server started. Waiting for incoming connections...");
            // Ciclo di controllo dei thread dei client
            while (server.ClientThreads.Count > 0)
            {
                foreach (Thread clientThread in server.ClientThreads.ToList())
                {
                    if (!clientThread.IsAlive)
                    {
                        server.ClientThreads.Remove(clientThread);
                    }
                }
            }
        }
    }

    public class Server
    {
        public string ServerIp { get; set; }
        public int ServerPort { get; set; }
        public List<TcpClient> ConnectedClients { get; set; }
        public List<Thread> ClientThreads { get; set; }


        public Server(string serverIP, int serverPort)
        {
            ServerIp = serverIP;
            ServerPort = serverPort;
            ConnectedClients = new List<TcpClient>();
            ClientThreads = new List<Thread>();

            TcpListener listener = new TcpListener(IPAddress.Parse(serverIP), serverPort);
            listener.Start();

            // Inizia ad ascoltare le connessioni in entrata
            while (true)
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

                if (dataReceived.ToLower() == "exit")
                {
                    isRunning = false;
                }
                else
                {

                    // Verifica che l'oggetto JSON ricevuto sia valido
                    if (TryDeserializeJson<Contact>(dataReceived, out var contact))
                    {
                        Console.WriteLine(
                            $"Received Contact: name: {contact.Name}, surname: {contact.Surname}, phone number: {contact.PhoneNumber}, Note: {contact.Note}");

                        // Salva il contatto su un file JSON
                        SaveContactToFile(contact);

                        // Invia un messaggio di conferma al client
                        string confirmationMessage = "Data received successfully.";
                        byte[] confirmationBuffer = Encoding.ASCII.GetBytes(confirmationMessage);
                        stream.Write(confirmationBuffer, 0, confirmationBuffer.Length);

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
                        Console.WriteLine("Received data is not a valid Contact JSON.");

                        // Invia un messaggio di errore al client
                        string errorMessage = "Received data error is not a valid Contact JSON";
                        byte[] errorBuffer = Encoding.ASCII.GetBytes(errorMessage);
                        stream.Write(errorBuffer, 0, errorBuffer.Length);
                    }
                }
                if (dataReceived.ToLower() == "delete")
                {
                    string jsonToDelete = dataReceived.Substring(7);
                    if (TryDeserializeJson<Contact>(jsonToDelete, out var contactToDelete))
                    {
                        // Chiamare un metodo per eliminare il contatto dal file o dal database
                        ClearContactFile(contactToDelete);
                        string confirmationMessage = "Contact deleted successfully.";
                        byte[] confirmationBuffer = Encoding.ASCII.GetBytes(confirmationMessage);
                        stream.Write(confirmationBuffer, 0, confirmationBuffer.Length);
                    }
                    else
                    {
                        string errorMessage = "Invalid JSON format for deletion.";
                        byte[] errorBuffer = Encoding.ASCII.GetBytes(errorMessage);
                        stream.Write(errorBuffer, 0, errorBuffer.Length);
                    }
                    continue;
                }
           
                
              
                  
            }
            ConnectedClients.Remove(client);
            client.Close();
            Console.WriteLine("Client disconnected.");
            Thread clientThread = Thread.CurrentThread;
            ClientThreads.Remove(clientThread);

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

        private void SaveContactToFile(Contact contact)
        {
            string contactJson = JsonConvert.SerializeObject(contact);
            string filePath = "C:\\code\\ixla\\test\\Project\\Project_client\\Projetc_contact_server\\contact_server.json"; // Specifica il percorso del file in cui vuoi salvare i contatti
            File.AppendAllText(filePath, contactJson + Environment.NewLine);
        }
        private void ClearContactFile(Contact contactToDelete)
        {
            var contactToRemove = ConnectedClients.FirstOrDefault(c => contactToDelete.Name == contactToDelete.Name && contactToDelete.Surname == contactToDelete.Surname);
            if (contactToRemove != null)
            {
                ConnectedClients.Remove(contactToRemove);
            }
            
        }
        
        public class Contact
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string PhoneNumber { get; set; }
            public string Note { get; set; }
        }

       
    }
}
