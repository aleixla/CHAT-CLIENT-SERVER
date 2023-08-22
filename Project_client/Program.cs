// See https://aka.ms/new-console-template for more information

using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Client
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

        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();

        Thread sendThread = new Thread(SendMessage);
        sendThread.Start();
        
       
    }

    private static void ReceiveMessages()
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
         
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
               
                Console.WriteLine("response of the client: " + message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
        Console.WriteLine("-----------------------------------------");
    }

    private static void SendMessage()
    {
       try
       {
           while (true)
           {
               Console.WriteLine("write message...");
               string message = Console.ReadLine();
               byte[] buffer = Encoding.ASCII.GetBytes(message);
               stream.Write(buffer, 0, buffer.Length);
           }
       }
       catch (Exception ex)
       {
           Console.WriteLine("An error occurred: " + ex.Message);
       }

      
    }
}





