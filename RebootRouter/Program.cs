﻿using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

class TcpClientSample
{
    public static void Main()
    {
        int n = 0;
        //Console.Write(MyPing("www.det.act.gov.au"));
        while (true)
        {
            if (MyPing("google.com")==0)
            {
                if (MyPing("ya.ru") == 0)
                {
                    if (MyPing("mail.ru") == 0)
                    {
                        Reboot();
                    }
                }
            }
            Console.Write("Ok\n");
        }
        //Console.ReadKey(true);
    }

    static int MyPing(string addr)
    {
        Ping ping = new Ping();
        PingReply pingReply = null;
        try
        {
            pingReply = ping.Send(addr, 2000);
        }
        catch (Exception)
        {
            pingReply = null;
        }
        if (pingReply != null && pingReply.Status == IPStatus.Success)
        {
            return Convert.ToInt32(pingReply.RoundtripTime); //время ответа
        }
        else
        {
            return 0;
        }
    }

    static void Reboot()
    {
        byte[] data = new byte[1024];
        TcpClient server;
        try
        {
            server = new TcpClient("192.168.0.1", 23);
        }
        catch (SocketException)
        {
            return;
        }
        Thread.Sleep(1000);
        SendCmd(server, "admin");
        SendCmd(server, "pass");
        SendCmd(server, "reboot");
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