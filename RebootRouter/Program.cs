﻿using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TcpClientSample
{
    public static void Main()
    {
        Thread.Sleep(60000);

        var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogRebootRouter.txt");        //Путь к файлу
        int N = 0;      //Количество перезагрузок роутера с момента запуска программы
        bool Flag = false;

        string line = string.Format(">   {0}  Старт программы", DateTime.Now);

        if (File.Exists(FilePath))
        {
            Flag = true;
            File.AppendAllText(FilePath, Environment.NewLine);
            File.AppendAllText(FilePath, line + Environment.NewLine);
        }
        else
        {
            Flag = false;
            File.AppendAllText(FilePath, line + Environment.NewLine);
        }

        for (int i = 0; i < 3; i++)
        {
            if (MyPing("google.com") == 0)
            {
                if (MyPing("ya.ru") == 0)
                {
                    if (MyPing("mail.ru") == 0)
                    {
                        Reboot();
                        line = string.Format(">   {0}  Reboot! №{1}", DateTime.Now, N++);
                        File.AppendAllText(FilePath, line + Environment.NewLine);
                        Thread.Sleep(180000);     // 3 мин = 180000     1 мин = 60000
                        continue;
                    }
                }
            }
            string smtpServer = "smtp.yandex.ru";
            string from = "login@yandex.ru";
            string password = "pass";
            string mailto = "to@mail.ru";
            string caption = "LogReboot";
            string message = "";
            string attachFile = "";

            if (Flag)
            {
                message = "";
                attachFile = FilePath;
                SendMail(smtpServer, from, password, mailto, caption, message, attachFile);
                Thread.Sleep(5000);
                File.Delete(FilePath);
            }
            else
            {
                message = String.Format("Log файл не обнаружен.\nПуть поиска: {0}", FilePath);
                SendMail(smtpServer, from, password, mailto, caption, message);
                Thread.Sleep(5000);
            }
            break;
        }
        

        line = string.Format(">   {0}  Старт основной части программы программы", DateTime.Now);
        Console.Write(line);
        File.AppendAllText(FilePath, line + Environment.NewLine);

        Flag = false;      // Флаг перезагрузки.
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

    public static void SendMail(string smtpServer, string from, string password, string mailto, string caption, string message, string attachFile = null)
    // <summary>
    // Отправка письма на почтовый ящик C# mail send
    // </summary>
    // <param name="smtpServer">Имя SMTP-сервера</param>
    // <param name="from">Адрес отправителя</param>
    // <param name="password">пароль к почтовому ящику отправителя</param>
    // <param name="mailto">Адрес получателя</param>
    // <param name="caption">Тема письма</param>
    // <param name="message">Сообщение</param>
    // <param name="attachFile">Присоединенный файл</param>
    {
        try
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(from);
            mail.To.Add(new MailAddress(mailto));
            mail.Subject = caption;
            mail.Body = message;
            if (!string.IsNullOrEmpty(attachFile))
                mail.Attachments.Add(new Attachment(attachFile));
            SmtpClient client = new SmtpClient();
            client.Host = smtpServer;
            client.Port = 587;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(from.Split('@')[0], password);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Send(mail);
            mail.Dispose();
        }
        catch (Exception e)
        {
            var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogRebootRouter.txt");        //Путь к файлу
            string line = string.Format("#   {0}  ERROR  Funtion SendMail Письмо не отправлено! Message: {1}", DateTime.Now, e.Message);
            File.AppendAllText(FilePath, line + Environment.NewLine);
        }
    }

}

