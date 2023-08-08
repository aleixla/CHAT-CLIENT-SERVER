// See https://aka.ms/new-console-template for more information

using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Program
{
   public static async Task Main(string[] args)
   {
      
      string serverIP = "192.168.3.232";
      int serverPort = 5555;

      TcpClient client = new TcpClient();
      client.Connect(serverIP, serverPort);

      Console.WriteLine("Connected to the server.");

      NetworkStream stream = client.GetStream();

      string message;
      while (true)
      {
         Console.WriteLine("enter the message: ");
         message = Console.ReadLine();

         if (message.ToLower() == "exit")
         {
            break; 
         }

         byte[] sendData = Encoding.ASCII.GetBytes(message);

         stream.Write(sendData, 0, sendData.Length);
         Console.WriteLine("Message sent to the server");

         try
         {
            byte[] receiveData = new byte[1024];
            int bytes = stream.Read(receiveData, 0, receiveData.Length);
            string serverResponse = Encoding.ASCII.GetString(receiveData, 0, bytes);
            Console.WriteLine("Response from the client: " + serverResponse);
         }
         catch (Exception ex)
         {
            Console.WriteLine("An error occurred: " + ex.Message);
         }  
      }

     
      Console.WriteLine("Connection closed.");
   }
   
}





