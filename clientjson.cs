using System;
using System.Dynamic;
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
            string option;
           
            Console.WriteLine("enter a option insert or delete");
            option = Console.ReadLine();
            switch (option)
            {
                case "insert":
                    Thread sendThread = new Thread(SendMessage);
                    sendThread.Start();
                    break;
                case "delete":
                    // Invia un messaggio al server per indicare l'operazione di cancellazione
                    Thread delteThread = new Thread(Delete);
                    delteThread.Start();
                    break;
                case "print":
                    Print();
                    break;
                case "exit":
                    Console.WriteLine("exit");
                    break;
                default:
                    Console.WriteLine("Invalid option");
                    break;
            }

        }

        private static void SendMessage()
        {
            NetworkStream stream = client.GetStream();
            StreamWriter writer = new StreamWriter(stream);

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("enter name (or 'exit' to quit): ");
                string name = Console.ReadLine();

                if (name.ToLower() == "exit")
                {
                    exit = true;
                    continue;
                }

                Console.WriteLine("enter surname: ");
                string surname = Console.ReadLine();
                Console.WriteLine("enter phone number: ");
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
            }

            client.Close();
            Console.WriteLine("Connection closed.");
        }

        public static void Print()
        {
            // Invia un messaggio al server per indicare l'operazione di stampa
            StreamWriter writer = new StreamWriter(stream);
         
            string allContactsJson = File.ReadAllText("C:\\code\\ixla\\test\\Project\\Project_client\\Projetc_contact_server\\contact_server.json");
            Console.WriteLine(allContactsJson);
            writer.WriteLine("print");
            writer.Flush();
    
            // Ricevi la risposta dal server
            StreamReader reader = new StreamReader(stream);
            string response = reader.ReadToEnd();
    
            // Stampa i dati ricevuti dal server
            Console.WriteLine(response);
        }

       

        private static void Delete()
        {
            NetworkStream stream = client.GetStream();
            StreamWriter writer = new StreamWriter(stream);
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("enter name for delete (or 'exit' to quit): ");
                string name = Console.ReadLine();
               

                if (name.ToLower() == "exit")
                {
                    exit = true;
                    continue;
                }
                
                Console.WriteLine("enter surname: ");
                string surname = Console.ReadLine();
                Console.WriteLine("enter phone number: ");
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

                // Invia la stringa JSON al server, preceduta dalla parola "delete"
                try
                {
                    writer.WriteLine("delete " + json);
                    writer.Flush();
                    Console.WriteLine("Operazione di cancellazione inviata al server");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Si è verificato un errore: " + ex.Message);
                }
            }

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
