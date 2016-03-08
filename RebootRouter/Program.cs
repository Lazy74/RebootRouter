using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TcpClientSample
{
    public static void Main()
    {
        int N = 0;      //Количество перезагрузок роутера с момента запуска программы
        var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogRebootRouter.txt");        //Путь к файлу

        string line = string.Format("\n>   {0}  Старт программы", DateTime.Now);
        try
        {
            File.AppendAllText(FilePath, Environment.NewLine);
            File.AppendAllText(FilePath, line + Environment.NewLine);
        }
        catch (Exception)
        {
            File.Create(FilePath);
            File.AppendAllText(FilePath, line + Environment.NewLine);
        }
        bool Flag = false;      // Флаг перезагрузки.
        while (true)
        {
            if (Flag)
            {
                Flag = false;
                line = string.Format(">   {0}  Возобновление программы", DateTime.Now);
                File.AppendAllText(FilePath, line + Environment.NewLine);
            }

            if (MyPing("google.com")==0)
            {
                line = string.Format(">   {0}  Fail ping google.com", DateTime.Now);
                File.AppendAllText(FilePath, line + Environment.NewLine);

                if (MyPing("ya.ru") == 0)
                {
                    line = string.Format(">   {0}  Fail ping ya.ru", DateTime.Now);
                    File.AppendAllText(FilePath, line + Environment.NewLine);

                    if (MyPing("mail.ru") == 0)
                    {
                        line = string.Format(">   {0}  Fail ping mail.ru", DateTime.Now);
                        File.AppendAllText(FilePath, line + Environment.NewLine);
                        line = string.Format(">   {0}  Reboot! №{1}", DateTime.Now, N++);
                        File.AppendAllText(FilePath, line + Environment.NewLine);

                        Flag = true;
                        //Console.Write("Reboot\n");
                        Reboot();

                        Thread.Sleep(180000);     // 3 мин = 180000     1 мин = 60000
                    }
                }
            }
            //Console.Write("ok\n");
            Thread.Sleep(2000);
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
        catch (Exception e)
        {
            var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogRebootRouter.txt");        //Путь к файлу
            string line = string.Format("#   {0}  ERROR  Funtion MyPing({1}) Message: {2}", DateTime.Now, addr, e.Message);
            File.AppendAllText(FilePath, line + Environment.NewLine);

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
        //catch (SocketException)
        catch(Exception e)
        {

            var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogRebootRouter.txt");        //Путь к файлу
            string line = string.Format("#   {0}  ERROR  Funtion Reboot Message: {1}", DateTime.Now, e.Message);
            File.AppendAllText(FilePath, line + Environment.NewLine);

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