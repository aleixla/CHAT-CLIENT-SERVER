using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
public class Program
{
    public static void Main(string []args)
    {
        string serverIP = "192.168.3.232";
        int serverPort = 5555;

        try
        {
            TcpClient client = new TcpClient();
            client.Connect(serverIP, serverPort);
            Console.WriteLine("Connected to the server.");
            NetworkStream stream = client.GetStream();

            MessageSender sender = new MessageSender(stream);
            MessageReceiver receiver = new MessageReceiver(stream);

            sender.Start();
            receiver.Start();

            sender.WaitForCompletion();
            receiver.WaitForCompletion();

            client.Close();
            Console.WriteLine("Connection closed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
//classe invio messaggio
class MessageSender
{
    private NetworkStream _stream { get; set; }
    private bool _completed{ get; set; }

    
    public MessageSender(NetworkStream stream)
    {
        this._stream = stream;
        this._completed = false;
    }

    public void Start()
    {
        Console.WriteLine("Sender started.");
        Console.WriteLine("Enter messages to send to the server. Type 'exit' to quit.");

        while (!_completed)
        { 
            string message = Console.ReadLine();

            if (message.ToLower() == "exit")
            {
                break;
            }

            try
            {
                byte[] sendData = Encoding.ASCII.GetBytes(message);
                _stream.Write(sendData, 0, sendData.Length);
                Console.WriteLine("Message sent to the server");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        Console.WriteLine("Sender completed.");
    }

    public void WaitForCompletion()
    {
        _completed = true;
    }
}
// classe dove ricevo il messaggio
class MessageReceiver
{
    private NetworkStream _stream{ get; set; }
    private bool _completed{ get; set; }

    public MessageReceiver(NetworkStream stream)
    {
        this._stream = stream;
        this._completed = false;
    }

    public void Start()
    {
        Console.WriteLine("Receiver started.");

        while (!_completed)
        {
            try
            {
                byte[] receiveData = new byte[1024];
                int bytesRead = _stream.Read(receiveData, 0, receiveData.Length);

                if (bytesRead > 0)
                {
                    string serverResponse = Encoding.ASCII.GetString(receiveData, 0, bytesRead);
                    Console.WriteLine("Response from the server: " + serverResponse);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        Console.WriteLine("Receiver completed.");
    }

    public void WaitForCompletion()
    {
        _completed = true;
    }
}
