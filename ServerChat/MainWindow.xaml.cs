using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ServerChat
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int portHost = 5555; // порт для приема входящих запросов
        private ServerPart server; // сервер
        private Thread listenThread; // потока для прослушивания

        private static ObservableCollection<string> _listClients;
        private ObservableCollection<Message> _listMessage;
        public MainWindow()
        {
            InitializeComponent();

            _listClients = new ObservableCollection<string>();
            listBoxListClients.ItemsSource = _listClients;

            _listMessage = new ObservableCollection<Message>();
            listBoxistReciveMessage.ItemsSource = _listMessage;

            textBoxPortServer.Text = portHost.ToString();
        }


        public void RunServer(int portHost)
        {
            try
            {
                server = new ServerPart(this, portHost);
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start(); //старт потока
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }

        }

        public void AddClientInList(string nameClient)
        {
            // Получить диспетчер от текущего окна и использовать его для вызова кода обновления
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    _listClients.Add(nameClient);
                }
            );
        }

        public void RemoveClientWithList(string nameClient)
        {
            // Получить диспетчер от текущего окна и использовать его для вызова кода обновления
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    _listClients.Remove(nameClient);
                }
            );
        }

        private void RunServer_Click(object sender, RoutedEventArgs e)
        {
            RunServer(portHost);
        }

        //Вывод сообщения на экран
        public void SendToListReciveMessage(string message)
        {
            // Получить диспетчер от текущего окна и использовать его для вызова кода обновления
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    _listMessage.Add(new Message { Text = message });
                }
              );

        }

        private void ChangePortServer_Click(object sender, RoutedEventArgs e)
        {
            string patternPort = @"\d{4}\d?$";
            if(Regex.IsMatch(textBoxPortServer.Text, patternPort))
            {
                ((MainWindow)this.Owner).portHost = Convert.ToInt32(textBoxPortServer.Text);
            }
            else
            {
                textBoxPortServer.Text = ((MainWindow)this.Owner).portHost.ToString();
            }
        }

       

        public void ChangeStatusServer(bool IsConnect)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    if (IsConnect)
                        ellipseStatusConnection.Fill = new SolidColorBrush(Colors.Green);
                    else
                        ellipseStatusConnection.Fill = new SolidColorBrush(Colors.Red);
                }
             );
        }

        private void ClosingProgram(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (server != null)
            {
                //communicationWithServer.SendMessageToServer("disconnect");
                server.Disconnect();
            }
        }

        public class Message
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public string Side { get; set; }
        }
    }
}
