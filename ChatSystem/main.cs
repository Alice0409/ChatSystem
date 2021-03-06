using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ChatSystem;

namespace ChatSystem
{
    class main
    {
        static ChatSystem chatSystem;
        const Int32 portNo = 11000;
        const string EOF = "<EOF>";
        static readonly int maxLength = 200 + EOF.Length;
        static ChatSystem.ConnectMode connectMode;
        enum FunctionMode { chat, bot, janken };
        static FunctionMode functionMode = FunctionMode.chat;

        static void Main(string[] args)
        {
            chatSystem = new ChatSystem(maxLength);
            Console.WriteLine($"this hostName is {chatSystem.hostName}.");
            functionMode = SelectFunction();
            connectMode = SelectMode();
            switch (functionMode)
            {
                case FunctionMode.chat:
                    InChat();
                    break;
                case FunctionMode.bot:
                    InChatBot();
                    break;
                case FunctionMode.janken:
                    InChatJanken();
                    break;
                default:
                    Console.WriteLine("not suported");
                    break;
            }
        }
        static FunctionMode SelectFunction()
        {
            Console.WriteLine("Select Function\n0 = chat\n1 = bot\n2 = janken");
            int select = int.Parse(Console.ReadLine());
            FunctionMode[] function = { FunctionMode.chat, FunctionMode.bot, FunctionMode.janken };
            return function[select];
        }
        static ChatSystem.ConnectMode SelectMode()
        {
            ChatSystem.ConnectMode connectMode = ChatSystem.ConnectMode.host;
            Console.Write("Select Mode：0 = Host, 1 = Client\n");
            int select = int.Parse(Console.ReadLine());
            switch (select)
            {
                case 0: // Host
                    Console.WriteLine("Running Host mode");
                    InitializeHost();
                    connectMode = ChatSystem.ConnectMode.host;
                    break;
                case 1: // Client
                    Console.WriteLine("Running Client mode");
                    InitializeClient();
                    connectMode = ChatSystem.ConnectMode.client;
                    break;
                default:
                    Console.WriteLine("ERROR undefind");
                    break;
            }
            return connectMode;
        }
        static void InitializeHost()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(chatSystem.hostName);
            foreach (var addresslist in ipHostInfo.AddressList)
            {
                Console.WriteLine($"found own address：{addresslist.ToString()}");
            }
            Console.Write($"Select address to listen（0 - {ipHostInfo.AddressList.Length - 1})：");
            IPAddress ipAddress = ipHostInfo.AddressList[int.Parse(Console.ReadLine())];
            ChatSystem.EResult re = chatSystem.InitializeHost(ipAddress, portNo);
            if (re != ChatSystem.EResult.success)
            {
                Console.WriteLine($"failed to initialize, ERROR = {re.ToString()}");
            }
        }
        static void InitializeClient()
        {
            Console.Write("Input IP address to connect：");
            var ipAddress = IPAddress.Parse(Console.ReadLine());
            ChatSystem.EResult re = chatSystem.InitializeClient(ipAddress, 11000);
            if (re == ChatSystem.EResult.success)
            {
                Console.WriteLine($"Connected host {ipAddress.ToString()}");
            }
            else
            {
                Console.WriteLine($"failed to connect to host, ERROR = {chatSystem.resultMessage}");
            }
        }
        static void InChat()
        {
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            while (true)
            {
                if (turn)
                { // 受信
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        string received = Encoding.UTF8.GetString(buffer.content).Replace(EOF, "");
                        int l = received.Length;
                        if (received[0] != '\0')
                        { // 正常にメッセージを受信
                            Console.WriteLine($"受信メッセージ：{received}");
                        }
                        else
                        { // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    { // 受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                }
                else
                { // 送信
                    Console.Write("送るメッセージ：");
                    string inputSt = Console.ReadLine();
                    if (inputSt.Length > maxLength)
                    {
                        inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                    }
                    inputSt += EOF;
                    buffer.content = Encoding.UTF8.GetBytes(inputSt);
                    buffer.length = buffer.content.Length;
                    ChatSystem.EResult re = chatSystem.Send(buffer);
                    if (re != ChatSystem.EResult.success)
                    {
                        Console.WriteLine($"送信エラー：{re.ToString()} Error code： {chatSystem.resultMessage}");
                        break;
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();
        }
        static void InChatBot()
        {
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            string received = string.Empty;
            while (true)
            {
                if (turn)
                { // 受信
                    received = string.Empty;
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        received = Encoding.UTF8.GetString(buffer.content).Replace(EOF, "");
                        int l = received.Length;
                        if (received[0] != '\0')
                        { // 正常にメッセージを受信
                            Console.WriteLine($"受信メッセージ：{received}");
                        }
                        else
                        { // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    { // 受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                }
                else
                { // 送信
                    string inputSt = string.Empty;
                    Console.Write("送るメッセージ：");
                    if (connectMode == ChatSystem.ConnectMode.host)
                    { // Host
                        Random rand = new Random();
                        int num = rand.Next(0, 5);
                        string[] response = { "へぇ～", "うんうん", "ふむふむ", "なるほど", "そうなんだ" };
                        Console.WriteLine(inputSt = response[num]);
                    }
                    else
                    { // Client
                        inputSt = Console.ReadLine(); // 入力文字で送信
                        if (inputSt.Length > maxLength)
                        {
                            inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                        }
                    }
                    inputSt += EOF;
                    buffer.content = Encoding.UTF8.GetBytes(inputSt);
                    buffer.length = buffer.content.Length;
                    ChatSystem.EResult re = chatSystem.Send(buffer);
                    if (re != ChatSystem.EResult.success)
                    {
                        Console.WriteLine($"送信エラー：{re.ToString()} Error code：{chatSystem.resultMessage}");
                        break;
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();
        }
        static void InChatJanken()
        {
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            string received = string.Empty;
            string inputSt = string.Empty;
            while (true)
            {
                if (turn)
                { // 受信
                    received = string.Empty;
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        received = Encoding.UTF8.GetString(buffer.content).Replace(EOF, "");
                        int l = received.Length;
                        if (received[0] != '\0')
                        { // 正常にメッセージを受信
                            Console.WriteLine($"結果：{received}");
                        }
                        else
                        { // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    { // 受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                }
                else
                { // 送信
                    if (connectMode == ChatSystem.ConnectMode.host)
                    { // Host
                        inputSt = received;
                    }
                    else
                    { // Client
                        Console.Write("何を出す？(1 = グー, 2 = チョキ, 3 = パー)：");
                        int select = int.Parse(Console.ReadLine());
                        Random rand = new Random();
                        int num = rand.Next(1, 4);
                        string[] hand = { "グー", "チョキ", "パー" };
                        if (select == 1 && num == 1 || select == 2 && num == 2 || select == 3 && num == 3)
                        {
                            inputSt = "お互い" + hand[num - 1] + "で引き分け";
                        }
                        else if (select == 1 && num == 2 || select == 2 && num == 3 || select == 3 && num == 1)
                        {
                            inputSt = "ホストは" + hand[num - 1] + "、クライアントは" + hand[select - 1] + "でクライアントの勝ち";
                        }
                        else if (select == 1 && num == 3 || select == 2 && num == 1 || select == 3 && num == 2)
                        {
                            inputSt = "ホストは" + hand[num - 1] + "、クライアントは" + hand[select - 1] + "でクライアントの負け";
                        }
                        else
                        {
                            inputSt = "入力エラー";
                        }

                        if (inputSt.Length > maxLength)
                        {
                            inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                        }
                    }
                    inputSt += EOF;
                    buffer.content = Encoding.UTF8.GetBytes(inputSt);
                    buffer.length = buffer.content.Length;
                    ChatSystem.EResult re = chatSystem.Send(buffer);
                    if (re != ChatSystem.EResult.success)
                    {
                        Console.WriteLine($"送信エラー：{re.ToString()} Error code：{chatSystem.resultMessage}");
                        break;
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();
        }
    }
}