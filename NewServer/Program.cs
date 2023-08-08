
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Servidor_Chat
{
    class Server
    {
        IPAddress ipAddr;
        IPEndPoint endPoint;
        Socket s_Server;
        Socket s_Client;
        public Server()
        {
            ipAddr = IPAddress.Any;
            endPoint = new IPEndPoint(ipAddr, 5555);
            s_Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s_Server.Bind(endPoint);
            s_Server.Listen(1);
        } 

        public void Start()
        {
            Console.WriteLine("Esperando clientes...");
            s_Client = s_Server.Accept();
            Console.WriteLine("Un cliente se ha conectado.");
            IPEndPoint clientep = (IPEndPoint)s_Client.RemoteEndPoint;
            Console.WriteLine("Conectado con {0} en el puerto {1}", clientep.Address, clientep.Port);
        }

        public void Send(string msg)
        {
            string texto = "";
            byte[] textoAEnviar;
            texto = msg;
            textoAEnviar = Encoding.Default.GetBytes(texto);
            s_Client.Send(textoAEnviar, 0, textoAEnviar.Length, 0);
        } 

        public void Receive()
        {
            while (true)
            {
                Thread.Sleep(500);
                byte[] ByRec;
                string textoRecibido = "";
                ByRec = new byte[255];
                int a = s_Client.Receive(ByRec, 0, ByRec.Length, 0);
                Array.Resize(ref ByRec, a);
                textoRecibido = Encoding.Default.GetString(ByRec);
                Console.WriteLine("Client: " + textoRecibido);
                Console.Out.Flush();
            }
        }
    } 

    class Program
    {
        static void Main(string[] args)
        {
            Thread t;
            Server s = new Server();
            s.Start();
            t = new Thread(new ThreadStart(s.Receive));
            t.Start();
            while (true)
            {
                s.Send(Console.ReadLine());
            } 

            Console.WriteLine("Presione cualquier tecla para terminar");
            Console.ReadLine();
        }
    }
}
