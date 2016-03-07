using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TcpClientSample
{
    public static void Main()
    {
        Reboot();
    }

    static void Reboot()
    {
        byte[] data = new byte[1024];
        string input, stringData;
        TcpClient server;
        try
        {
            server = new TcpClient("192.168.0.1", 23);
        }
        catch (SocketException)
        {
            //Console.WriteLine("Unable to connect to server");
            return;
        }
        Thread.Sleep(1000);

        //NetworkStream ns = server.GetStream();
        //int recv = ns.Read(data, 0, data.Length);
        //stringData = Encoding.ASCII.GetString(data, 0, recv);
        //Console.WriteLine(stringData);
        SendCmd(server, "admin");
        SendCmd(server, "pass");
        SendCmd(server, "reboot");
        //Console.WriteLine("Disconnecting from server...");
        //Thread.Sleep(1000);

        //ns.Close();
        server.Close();
    }

    static void SendCmd(TcpClient tc, string cmd)
    {
        NetworkStream netStream = tc.GetStream();
        if (netStream.CanWrite)
        {
            Byte[] sendBytes = Encoding.UTF8.GetBytes(cmd + "\r");
            netStream.Write(sendBytes, 0, sendBytes.Length);
        }
        Thread.Sleep(1000);
    }

}