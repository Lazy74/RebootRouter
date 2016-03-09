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
                Console.Write("Тестовая перезагрузка\n");
                Reboot();
                Console.Write("The end\n");
                Console.ReadKey();
                return;
            }
        }
        Console.Write("Старт программы\tПауза на минуту\n");
        Thread.Sleep(60000);

        var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogRebootRouter.txt");        //Путь к файлу
        Console.Write(string.Format("Получен путь к файлу: {0}\n", FilePath));
        int N = 0;      //Количество перезагрузок роутера с момента запуска программы
        bool Flag = false;

        string line = string.Format(">   {0}  Старт программы", DateTime.Now);

        if (File.Exists(FilePath))
        {
            Console.Write("Log файл найден\n");
            Flag = true;
            File.AppendAllText(FilePath, Environment.NewLine);
            File.AppendAllText(FilePath, line + Environment.NewLine);
            Console.Write(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));
        }
        else
        {
            Flag = false;
            File.AppendAllText(FilePath, line + Environment.NewLine);
            Console.Write("Log файл не найден\n");
            Console.Write(string.Format("Создан новый файл и сделана запись Log файла. Текст сообщения\n {0}\n", line));
        }

        Console.Write(string.Format("Флаг наличия файла: {0}\n", Flag));

        Console.Write("Запущена первоначальная проверка интернета\n");
        for (int i = 0; i < 3; i++)
        {
            if (MyPing("google.com") == 0)
            {   
                Console.Write("Fail ping google.com\n");
                if (MyPing("ya.ru") == 0)
                {
                    Console.Write("Fail ping ya.ru\n");
                    if (MyPing("mail.ru") == 0)
                    {
                        Console.Write("Fail ping mail.ru\n");
                        Reboot();
                        //Console.Write("Отправлена комана перезагрузки\n");
                        line = string.Format(">   {0}  Reboot! №{1}", DateTime.Now, N++);
                        File.AppendAllText(FilePath, line + Environment.NewLine);
                        Console.Write(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));
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

            Console.Write(string.Format("Количество входных аргументов равно: {0}\n", args.Length));
            if (args.Length == 1)
            {
                caption += "TT_" + args[0];
                Console.Write(string.Format("Тема письма: {0}\n", caption));
            }
            else
            {
                string[] dirs = Directory.GetDirectories(@"C:\YandexDisk\", "TT_??");
                caption += Path.GetFileNameWithoutExtension(dirs[0]);
                message = "Отсутствует входной агрумент для формирования темы письма. Тема составлена на основе содержания папки обмена\n \t";
                Console.Write(string.Format("Тема письма составленного на основе папки обмена: {0}\n", caption));
            }

            if (Flag)
            { 
                message +=  string.Format("Дата сообщения формирования письма {0}", DateTime.Now);
                attachFile = FilePath;
                
                SendMail(smtpServer, from, password, mailto, caption, message, attachFile);
                Thread.Sleep(5000);
                File.Delete(FilePath);
                Console.Write(string.Format("Файл {0} был удален\n", FilePath));
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
        Console.Write(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));

        Flag = false;      // Флаг перезагрузки.
        while (true)
        {
            if (Flag)
            {
                Flag = false;
                line = string.Format(">   {0}  Возобновление программы", DateTime.Now);
                File.AppendAllText(FilePath, line + Environment.NewLine);
                Console.Write(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));
            }

            if (MyPing("google.com")==0)
            {
                line = string.Format(">   {0}  Fail ping google.com", DateTime.Now);
                File.AppendAllText(FilePath, line + Environment.NewLine);
                Console.Write(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));

                if (MyPing("ya.ru") == 0)
                {
                    line = string.Format(">   {0}  Fail ping ya.ru", DateTime.Now);
                    File.AppendAllText(FilePath, line + Environment.NewLine);
                    Console.Write(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));

                    if (MyPing("mail.ru") == 0)
                    {
                        line = string.Format(">   {0}  Fail ping mail.ru", DateTime.Now);
                        File.AppendAllText(FilePath, line + Environment.NewLine);
                        Console.Write(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));
                        line = string.Format(">   {0}  Reboot! №{1}", DateTime.Now, N++);
                        File.AppendAllText(FilePath, line + Environment.NewLine);
                        Console.Write(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));

                        Flag = true;
                        //Console.Write("Отправлена комана перезагрузки\n");
                        Reboot();
                        Console.Write("Пауза 3 минуты\n");
                        Thread.Sleep(180000);     // 3 мин = 180000     1 мин = 60000
                    }
                }
            }
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
            Console.Write(string.Format("ping({0})\t TimeOut: {1}\n", addr, pingReply.RoundtripTime));
        }
        catch (Exception e)
        {
            var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogRebootRouter.txt");        //Путь к файлу
            string line = string.Format("#   {0}  ERROR  Funtion MyPing({1}) Message: {2}", DateTime.Now, addr, e.Message);
            File.AppendAllText(FilePath, line + Environment.NewLine);
            Console.Write(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));

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
            Console.Write("Подключение к роутеру ip: {0} port {1}\n", ip, port);
            server = new TcpClient(ip, port);
            Console.Write("<УСПЕШНО>\n");
        }
        //catch (SocketException)
        catch(Exception e)
        {

            var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogRebootRouter.txt");        //Путь к файлу
            string line = string.Format("#   {0}  ERROR  Funtion Reboot Message: {1}", DateTime.Now, e.Message);
            File.AppendAllText(FilePath, line + Environment.NewLine);
            Console.Write(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));

            return;
        }
        Thread.Sleep(1000);
        Console.Write("Авторизация на роутере\n");
        Console.Write("Login: admin\n");
        SendCmd(server, "admin");
        Console.Write("pass: admin\n");
        SendCmd(server, "pass");
        Console.Write("send command: Reboot\n");
        SendCmd(server, "reboot");
        server.Close();
        Console.Write("Соединение с роутером закрыто.\n");
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
        Console.Write("Письмо для отправки сформированно!\n");
        Console.Write(string.Format("\tsmtpServer: {0}\n", smtpServer));
        Console.Write(string.Format("\tfrom: {0}\n", from));
        Console.Write(string.Format("\tpassword: {0}\n", password));
        Console.Write(string.Format("\tmailto: {0}\n", mailto));
        Console.Write(string.Format("\tcaption: {0}\n", caption));
        Console.Write(string.Format("\tmessage: {0}\n", message));
        Console.Write(string.Format("\tattachFile: {0}\n", attachFile));
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
            Console.Write("Письмо отправлено!\n");
        }
        catch (Exception e)
        {
            var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogRebootRouter.txt");        //Путь к файлу
            string line = string.Format("#   {0}  ERROR  Funtion SendMail Письмо не отправлено! Message: {1}", DateTime.Now, e.Message);
            File.AppendAllText(FilePath, line + Environment.NewLine);
            Console.Write(string.Format("Запись Log файла. Текст сообщения\n {0}\n", line));
        }
    }

}

