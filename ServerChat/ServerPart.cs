using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerChat
{
    class ServerPart
    {
        static TcpListener tcpListener; // сервер для прослушивания
        List<ClientPart> clients = new List<ClientPart>(); // Все подключенные клиенты
        public MainWindow mainWindow;
        static object locker = new object();
        private int portHost = 7777;

        public ServerPart(MainWindow mainWindow, int portHost)
        {
            this.mainWindow = mainWindow;
            this.portHost = portHost;
        }

        protected internal void AddConnection(ClientPart ClientPart)
        {
            clients.Add(ClientPart);
        }
        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientPart client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null)
            {
                clients.Remove(client);
            }
        }
        // прослушивание входящих подключений
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, portHost);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");
                mainWindow.ChangeStatusServer(true);

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientPart ClientPart = new ClientPart(tcpClient, this);
                    Task clientThread = new Task(ClientPart.Process);
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        // трансляция сообщения подключенным клиентам
        protected internal void BroadcastMessage(string message, string id)
        {
            Monitor.Enter(locker);

            mainWindow.SendToListReciveMessage(message);

            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id) // если id клиента не равно id отправляющего
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                }
            }
            Monitor.Exit(locker);
        }
        // отключение всех клиентов
        protected internal void Disconnect()
        {
            mainWindow.ChangeStatusServer(false);
            BroadcastMessage("disconnect", "");
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }
    }
}
