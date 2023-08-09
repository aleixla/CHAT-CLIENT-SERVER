using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;

namespace Projetc_contact_client
{
    internal class Program
    {
        private static TcpClient client { get; set; }
        private static NetworkStream stream { get; set; }

        public static void Main(string[] args)
        {
            string serverIP = "192.168.3.232";
            int serverPort = 5555;
            client = new TcpClient();
            client.Connect(serverIP, serverPort);
            Console.WriteLine("Connected to the server.");
            stream = client.GetStream();
            Thread sendThread = new Thread(SendMessage);
            sendThread.Start();
        }

        private static void SendMessage()
        { 
            NetworkStream stream = client.GetStream();
            
            StreamWriter writer = new StreamWriter(stream); 
            Console.WriteLine("enter name: ");
            string name = Console.ReadLine();
            Console.WriteLine("enter surname: ");
            string surname = Console.ReadLine();
             Console.WriteLine("enter number phone: ");
            string phoneNumber = Console.ReadLine();
            Console.WriteLine("enter note: ");
            string note = Console.ReadLine();
            var contact = new Contact()
            {
                Name = name,
                Surname = surname,
                PhoneNumber = phoneNumber,
                Note = note
           
            };

          // Convertire l'oggetto in una stringa JSON
            string json = JsonConvert.SerializeObject(contact);

          // Invia la stringa JSON al server
            try
            {
              writer.WriteLine(json);
              writer.Flush();
              Console.WriteLine("Messaggio inviato al server");
            }
            catch (Exception ex)
            {
              Console.WriteLine("Si è verificato un errore: " + ex.Message);
            }
            client.Close();
            Console.WriteLine("Connection closed.");
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