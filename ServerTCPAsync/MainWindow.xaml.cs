using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Sockets;
using System.Net;

namespace ConnentTCPServer
{
    public partial class MainWindow : Window
    {
        private TcpListener servers;
        private TcpClient clients;


        public MainWindow()
        {
            InitializeComponent();
            adrress.Text = IPAddress.Loopback.ToString();
            port.Text = "22";
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            IPAddress adrressIP;
            try
            {
                adrressIP = IPAddress.Any;
                //nasłuch wszystkich adresów z jednego portu
            }
            catch
            {
                MessageBox.Show("Błędny format adresu IP!");
                adrress.Text = String.Empty;
                return;
            }

            int ports = System.Convert.ToInt16(port.Text);

            try
            {
                info.Items.Add("Oczekuję na połączenie");
                servers = new TcpListener(adrressIP, ports);
                servers.Start();
                clients = servers.AcceptTcpClient();
                info.Items.Add("Nawiązano połączenie");
                connect.IsEnabled = false;
                stop.IsEnabled = true;
                clients.Close();
                servers.Stop();
                do
                {
                    clients = await servers.AcceptTcpClientAsync();
                    info.Items.Add("Nawiązano połączenie");
                    connect.IsEnabled = false;
                    stop.IsEnabled = true;
                    await StreamTest();
                }
                while (servers.Pending());
            }

            catch (Exception ex)
            {
                info.Items.Add("Błąd inicjacji serwera!");
                MessageBox.Show(ex.ToString(), "Błąd");
            }
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            servers.Stop();
            clients.Close();
            info.Items.Add("Zakonczono pracę serwera ...");
            connect.IsEnabled = true;
            stop.IsEnabled = false;
            lisening = false;
        }

        private bool lisening = true;

        private async Task StreamTest()
        {
            await Task.Factory.StartNew(() =>
            {
                NetworkStream netStream = clients.GetStream();
                while (lisening)
                {
                    if (netStream.CanRead)
                    {
                        byte[] bytes = new byte[clients.ReceiveBufferSize];

                        // Read can return anything from 0 to numBytesToRead. 
                        // This method blocks until at least one byte is read.
                        netStream.Read(bytes, 0, (int)clients.ReceiveBufferSize);
                        if (bytes.All(x => x == 0))
                        {
                            lisening = false;
                            break;
                        }
                        // Returns the data received from the host to the console.
                        string returndata = Encoding.UTF8.GetString(bytes);
                        //netStream.Flush();
                        if (!string.IsNullOrEmpty(returndata) && !string.IsNullOrWhiteSpace(returndata))
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                message.Items.Add($"To jest wiadomość otrzymana od klienta: {Environment.NewLine}{returndata}");
                            }));
                        }
                    }
                    else
                    {
                        message.Items.Add("Nie można odczytać waidomości od klienta.");
                        lisening = false;
                    }
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    info.Items.Add($"Zakończono odczytywanie");
                }));
            });
        }
    }
}
