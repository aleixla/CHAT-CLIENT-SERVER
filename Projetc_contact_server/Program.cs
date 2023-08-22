using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContactServer
{
    public class Program
    {
       
        static ConcurrentDictionary<string, Contact> contacts = new ConcurrentDictionary<string, Contact>();
        static string dataFilePath = "C:\\code\\ixla\\test\\Project\\Project_client\\Projetc_contact_server\\contact_server.json"; // Path to the JSON data file

        public static void Main(string[] args)
        {
          

            int port = 5555;
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Server start. Loading connection...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("accept new connection.");

                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }

        public  static void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;
            try
            {

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine(data);
                    JObject request;
                    
                    try
                    {
                        request = JObject.Parse(data);
                    }
                    catch (JsonReaderException)
                    {
                        // Handle JSON parsing error
                        JObject errorResponse = new JObject();
                        errorResponse["error"] = "Invalid JSON format.";
                        byte[] errorBytes = Encoding.UTF8.GetBytes(errorResponse.ToString());
                        stream.Write(errorBytes, 0, errorBytes.Length);
                        stream.Flush();// Cancella i buffer del flusso e fa si che i dati memorizzati nel buffer vengano scritti nel file
                        continue; // Skip processing invalid JSON
                    }

                    string command = request["Command"].ToString();
                    JObject response = new JObject();

                    switch (command)
                    {
                        case "insert":
                            string contact = Guid.NewGuid().ToString();
                            // Restituisce una rappresentazione di stringa del valore di questa istanza della classe Guid
                            Contact newContact = request["Contact_insert"].ToObject<Contact>();
                           // contacts = new ConcurrentDictionary<string, Contact>();
                            LoadContactsFromFile();
                            contacts.TryAdd(contact, newContact);
                            SaveContactsToFile(); // Save contacts after adding a new one
                            response["message"] = "Contact create.";
                            break;

                        case "delete":
                            string deleteContactId = request["Contact"].ToString();
                            if (contacts.TryRemove(deleteContactId, out _))
                            {
                                LoadContactsFromFile();
                                SaveContactsToFile(); // Save contacts after removal
                                response["message"] = "Contact delete successfully.";
                            }
                            else
                            {
                                response["error"] = "Contact not found.";
                            }

                            break;

                        case "search":
                            string searchContactId = request["Name"].ToString();
                            if (contacts.TryGetValue(searchContactId, out Contact foundContact))
                            {
                                LoadContactsFromFile();
                                response["Contact_search"] = JObject.FromObject(foundContact);
                                Console.WriteLine("contact found:");
                               // Console.WriteLine($"Command:{foundContact.Command}");
                                Console.WriteLine($"Name: {foundContact.Name}");
                                Console.WriteLine($"Surname: {foundContact.Surname}");
                                Console.WriteLine($"Phone Number: {foundContact.PhoneNumber}");
                                Console.WriteLine($"Note: {foundContact.Note}");
                            }
                            else
                            {
                                response["error"] = "Contact not found.";
                            }
                            break;
                        case "list":
                            LoadContactsFromFile();
                           var contactList = contacts.Values.ToList();
                            response["Contacts_list"] = JArray.FromObject(contactList); //metodo per creare un array
                            break;
                    
                        default:
                            response["error"] = "Command not valid.";
                            break;
                    }

                    byte[] responseBytes = Encoding.UTF8.GetBytes(response.ToString());
                    stream.Write(responseBytes, 0, responseBytes.Length);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine("Error when reading from the stream: " + ex.Message + "\n" + ex.StackTrace);
            }

            Console.WriteLine("Client disconnected.");
            client.Close();
        }

        public  static void LoadContactsFromFile()
        {
            if (File.Exists(dataFilePath))
            {
                try
                {
                    string jsonData = File.ReadAllText(dataFilePath);
                 
                    contacts = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Contact>>(jsonData);
                   
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading contacts: " + ex.Message);
                }
            }
           
        }

        public static void SaveContactsToFile()
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(contacts);
                File.WriteAllText(dataFilePath, jsonData);
             
            }
            catch (Exception ex)
            {
                Console.WriteLine("error save"+ex.Message);
            }
           
        }
    }
//
    public class Contact
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Surname")]
        public string Surname { get; set; }

        [JsonProperty("PhoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("Note")]
        public string Note { get; set; }
        
    }
}



