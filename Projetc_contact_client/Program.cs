using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using ConsoleTables;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContactClient
{
    public class Program
    {
        public  static void Main(string[] args)
        {
            string serverIP = "192.168.3.232";
            int serverPort = 5555;

            using (TcpClient client = new TcpClient(serverIP, serverPort))
            {
                NetworkStream stream = client.GetStream();
                Console.WriteLine("Connect  server.");
                
                bool exitRequested = false;

                while (!exitRequested)
                {
                    Console.WriteLine("Command (insert / delete/ search/list/page/exit):");
                    string command = Console.ReadLine();

                    JObject request = new JObject(); //i comandi devono essere serializzati in json
                    request["Command"] = command;

                    switch (command)
                    {
                        case "insert":
                            Console.Write("Name: ");
                            string name = Console.ReadLine();
                            Console.Write("Surname: ");
                            string surname = Console.ReadLine();
                            Console.Write("PhoneNumber: ");
                            string phoneNumber = Console.ReadLine();
                            Console.Write("Note: ");
                            string note = Console.ReadLine();

                            Contact newContact = new Contact
                            {
                                Name = name,
                                Surname = surname,
                                PhoneNumber = phoneNumber,
                                Note = note
                               
                            };
                            request["Contact_insert"] = JObject.FromObject(newContact); 
                            // valore di ritorno. Un JObject con i valori dell'oggetto specificato.
                            break;
                        case "delete":
                            Console.Write("name of contact: ");
                            string contact = Console.ReadLine();
                            request["Name:"] = contact;
                            break;
                        case "search":
                            Console.Write("name of contact: ");
                            string contactId = Console.ReadLine();
                            request["Name:"] = contactId;
                            break;
                        case "list":
                           Console.WriteLine("Table of contact");
                            break;
                        case "exit":
                            exitRequested = true;
                            Console.WriteLine("Exiting the program.");
                            break;
                        default:
                            Console.WriteLine("Command not valid.");
                            continue;
                    }
                    if (exitRequested)
                        break;
                    //

                    byte[] requestBytes = Encoding.UTF8.GetBytes(request.ToString()); //UTF8 CODIFICA DEI CARATTERI
                    stream.Write(requestBytes, 0, requestBytes.Length);

                    byte[] responseBytes = new byte[1024];
                    int bytesRead = stream.Read(responseBytes, 0, responseBytes.Length);
                    string responseData = Encoding.UTF8.GetString(responseBytes, 0, bytesRead);
                    try
                    {


                        JObject response = JObject.Parse(responseData); //spiegazione metodo Parse
                        //il metodo parse serve ad
                        //analizzare il formato testuale (stringa) di JSON e
                        //a costruire il valore JavaScript o l'oggetto
                        //response.ContainsKey("message")
                        Console.WriteLine("Received data from server: " + responseData);
                      

                        if (response.ContainsKey("message"))
                        {
                            Console.WriteLine("Messagge: " + response["message"]);
                            Contact foundContact = response["Contact_search"].ToObject<Contact>();
                            Console.WriteLine("Contact found:");
                            Console.WriteLine($"Name: {foundContact.Name}");
                            Console.WriteLine($"Surname: {foundContact.Surname}");
                            Console.WriteLine($"PhoneNumber: {foundContact.PhoneNumber}");
                            Console.WriteLine($"Note: {foundContact.Note}");
                        }
                        else if (response.ContainsKey("error"))
                        {
                            Console.WriteLine("Error: " + response["error"]);
                        }
                        if (response.ContainsKey("Contacts_list"))//comando page
                        {
                            JArray contactArray = (JArray)response["Contacts_list"];
                            var contactList = contactArray.ToObject<List<Contact>>();
                            bool exitPagination = false;
                            int pageSize = 4; // Numero di elementi per pagina
                            int currentPage = 1;
                            while (!exitPagination) 
                            {

                                int totalPages = (int)Math.Ceiling((double)contactList.Count / pageSize);

                                ConsoleTable table = new ConsoleTable("Name", "Surname", "PhoneNumber", "Note");
                                int startIndex = (currentPage - 1) * pageSize;
                                int endIndex = Math.Min(startIndex + pageSize, contactList.Count);

                                for (int i = startIndex; i < endIndex; i++)
                                {
                                    Contact contact = contactList[i];
                                    table.AddRow(contact.Name, contact.Surname, contact.PhoneNumber, contact.Note);
                                }

                                table.Write();
                                Console.WriteLine($"Page {currentPage}/{totalPages}");

                              
                                     Console.WriteLine("Enter 'next' for next page, 'prev' for previous page, or 'exit' to quit:");
                                     string userInput = Console.ReadLine();
     
                                     switch (userInput.ToLower())
                                     {
                                         case "next":
                                             if (currentPage < totalPages)
                                                 currentPage++;
                                             break;
                                         case "prev":
                                             if (currentPage > 1)
                                                 currentPage--;
                                             break;
                                         case "exit":
                                             exitPagination = true;
                                             break;
                                         default:
                                             Console.WriteLine("Invalid command.");
                                             break;
                                     }
                                    
                            }

                        }
                    }
                    catch (JsonReaderException ex)
                    {
                        Console.WriteLine("Error parsing JSON response: " + ex.Message);
                      
                    }
                }
            }
        }
    }   

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

