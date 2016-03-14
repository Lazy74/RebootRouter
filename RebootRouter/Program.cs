using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Management;
using System.Runtime.InteropServices;

//Убрать консоль
//Изменить логин, пароль, адрес
//Изменить логин, пароль, кому
class TcpClientSample
{
    public static void Main(string[] args)
    {
        if (args.Length != 0)
        {
            if (args[0] == "test")
            {
                ConsoleWrite("Тестовая перезагрузка\n");
                Reboot();
                ConsoleWrite("The end\n");
                ConsoleWrite();
                return;
            }
        }
        ConsoleWrite("Старт программы\tПауза на минуту\n");
        Thread.Sleep(60000);

        var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogRebootRouter.txt");        //Путь к файлу
        ConsoleWrite(string.Format("Получен путь к файлу: {0}\n", FilePath));
        int N = 1;      //Количество перезагрузок роутера с момента запуска программы
        bool Flag = false;

        string line = string.Format(">   {0}  Старт программы", DateTime.Now);

        if (File.Exists(FilePath))
        {
            ConsoleWrite("Log файл найден\n");
            Flag = true;
            File.AppendAllText(FilePath, Environment.NewLine);
            File.AppendAllText(FilePath, line + Environment.NewLine);
            ConsoleWrite(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));
        }
        else
        {
            Flag = false;
            File.AppendAllText(FilePath, line + Environment.NewLine);
            ConsoleWrite("Log файл не найден\n");
            ConsoleWrite(string.Format("Создан новый файл и сделана запись Log файла. Текст сообщения\n {0}\n", line));
        }

        ConsoleWrite(string.Format("Флаг наличия файла: {0}\n", Flag));

        ConsoleWrite("Запущена первоначальная проверка интернета\n");
        for (int i = 0; i < 3; i++)
        {
            if (MyPing("google.com") == 0)
            {   
                ConsoleWrite("Fail ping google.com\n");
                if (MyPing("ya.ru") == 0)
                {
                    ConsoleWrite("Fail ping ya.ru\n");
                    if (MyPing("mail.ru") == 0)
                    {
                        ConsoleWrite("Fail ping mail.ru\n");
                        Reboot();
                        //ConsoleWrite("Отправлена комана перезагрузки\n");
                        line = string.Format(">   {0}  Reboot! №{1}", DateTime.Now, N++);
                        File.AppendAllText(FilePath, line + Environment.NewLine);
                        ConsoleWrite(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));
                        Thread.Sleep(180000);     // 3 мин = 180000     1 мин = 60000
                        continue;
                    }
                }
            }

            string smtpServer = "smtp.yandex.ru";
            string from = "login@yandex.ru";
            string password = "pass";
            string mailto = "to@mail.ru";
            string caption = "Test LogReboot";
            string message = "";
            string attachFile = "";

            ConsoleWrite(string.Format("Количество входных аргументов равно: {0}\n", args.Length));
            if (args.Length == 1)
            {
                caption += "TT_" + args[0];
                ConsoleWrite(string.Format("Тема письма: {0}\n", caption));
            }
            else
            {
                string[] dirs = Directory.GetDirectories(@"C:\YandexDisk\", "TT_??");
                caption += Path.GetFileNameWithoutExtension(dirs[0]);
                message = "Отсутствует входной агрумент для формирования темы письма. Тема составлена на основе содержания папки обмена\n \t";
                ConsoleWrite(string.Format("Тема письма составленного на основе папки обмена: {0}\n", caption));
            }

            if (Flag)
            { 
                message +=  string.Format("Дата сообщения формирования письма {0}", DateTime.Now);
                attachFile = FilePath;
                
                SendMail(smtpServer, from, password, mailto, caption, message, attachFile);
                Thread.Sleep(5000);
                File.Delete(FilePath);
                ConsoleWrite(string.Format("Файл {0} был удален\n", FilePath));
            }
            else
            {
                message += String.Format("Log файл не обнаружен.\nПуть поиска: {0}", FilePath);
                
                SendMail(smtpServer, from, password, mailto, caption, message);
                Thread.Sleep(5000);
            }
            break;
        }
        

        line = string.Format(">   {0}  Старт основной части программы программы", DateTime.Now);
        File.AppendAllText(FilePath, line + Environment.NewLine);
        ConsoleWrite(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));

        Flag = false;      // Флаг перезагрузки.
        while (true)
        {
            if (Flag)
            {
                Flag = false;
                line = string.Format(">   {0}  Возобновление программы", DateTime.Now);
                File.AppendAllText(FilePath, line + Environment.NewLine);
                ConsoleWrite(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));
            }

            if (MyPing("google.com")==0)
            {
                line = string.Format(">   {0}  Fail ping google.com", DateTime.Now);
                File.AppendAllText(FilePath, line + Environment.NewLine);
                ConsoleWrite(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));

                if (MyPing("ya.ru") == 0)
                {
                    line = string.Format(">   {0}  Fail ping ya.ru", DateTime.Now);
                    File.AppendAllText(FilePath, line + Environment.NewLine);
                    ConsoleWrite(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));

                    if (MyPing("mail.ru") == 0)
                    {
                        line = string.Format(">   {0}  Fail ping mail.ru", DateTime.Now);
                        File.AppendAllText(FilePath, line + Environment.NewLine);
                        ConsoleWrite(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));
                        line = string.Format(">   {0}  Reboot! №{1}", DateTime.Now, N++);
                        File.AppendAllText(FilePath, line + Environment.NewLine);
                        ConsoleWrite(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));

                        Flag = true;
                        //ConsoleWrite("Отправлена комана перезагрузки\n");
                        Reboot();
                        ConsoleWrite("Пауза 3 минуты\n");
                        Thread.Sleep(180000);     // 3 мин = 180000     1 мин = 60000
                    }
                }
            }
            Thread.Sleep(2000);
        }
        //ConsoleReadKey(true);
    }

    static int MyPing(string addr)
    {
        Ping ping = new Ping();
        PingReply pingReply = null;
        try
        {
            pingReply = ping.Send(addr, 2000);
            ConsoleWrite(string.Format("ping({0})\t TimeOut: {1}\n", addr, pingReply.RoundtripTime));
        }
        catch (Exception e)
        {
            var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogRebootRouter.txt");        //Путь к файлу
            string line = string.Format("#   {0}  ERROR  Funtion MyPing({1}) Message: {2}", DateTime.Now, addr, e.Message);
            File.AppendAllText(FilePath, line + Environment.NewLine);
            ConsoleWrite(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));

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
            string ip = "192.168.0.1";
            int port = 23;
            ConsoleWrite(string.Format("Подключение к роутеру ip: {0} port {1}\n", ip, port));
            server = new TcpClient(ip, port);
            ConsoleWrite("<УСПЕШНО>\n");
        }
        //catch (SocketException)
        catch(Exception e)
        {

            var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogRebootRouter.txt");        //Путь к файлу
            string line = string.Format("#   {0}  ERROR  Funtion Reboot Message: {1}", DateTime.Now, e.Message);
            File.AppendAllText(FilePath, line + Environment.NewLine);
            ConsoleWrite(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));

            return;
        }
        Thread.Sleep(1000);
        ConsoleWrite("Авторизация на роутере\n");
        ConsoleWrite("Login: admin\n");
        SendCmd(server, "admin");
        ConsoleWrite("pass: admin\n");
        SendCmd(server, "pass");
        ConsoleWrite("send command: Reboot\n");
        SendCmd(server, "reboot");
        server.Close();
        ConsoleWrite("Соединение с роутером закрыто.\n");
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

    public static void ConsoleWrite(string text = null)
    {
        if (text==null)
        {
            Console.ReadKey();
        }
        else
        {
            Console.Write(text);
        }
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
        ConsoleWrite("Письмо для отправки сформированно!\n");
        ConsoleWrite(string.Format("\tsmtpServer: {0}\n", smtpServer));
        ConsoleWrite(string.Format("\tfrom: {0}\n", from));
        ConsoleWrite(string.Format("\tpassword: {0}\n", password));
        ConsoleWrite(string.Format("\tmailto: {0}\n", mailto));
        ConsoleWrite(string.Format("\tcaption: {0}\n", caption));
        ConsoleWrite(string.Format("\tmessage: {0}\n", message));
        ConsoleWrite(string.Format("\tattachFile: {0}\n", attachFile));
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
            ConsoleWrite("Письмо отправлено!\n");
        }
        catch (Exception e)
        {
            var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogRebootRouter.txt");        //Путь к файлу
            string line = string.Format("#   {0}  ERROR  Funtion SendMail Письмо не отправлено! Message: {1}", DateTime.Now, e.Message);
            File.AppendAllText(FilePath, line + Environment.NewLine);
            ConsoleWrite(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));
        }
    }
}

