using blockchainGUI.CodeBehind;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace blockchainGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Blockchain blockchain;
        private TcpListener tcpListener;
        private int port;
        private const string localhost = "127.0.0.1";
        public MainWindow()
        {
            InitializeComponent();
            UpdateBlockchainStatus();
            blockchain = new Blockchain();

        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string nodeName = NodeNameInput.Text;
            if (string.IsNullOrEmpty(nodeName))
            {
                MessageBox.Show("Vnesite ime!");
                return;
            }
        }

        private async void ConnectPortButton_Click(object sender, RoutedEventArgs e)
        {
            if(!int.TryParse(ConnectPortInput.Text, out port))
            {
                MessageBox.Show("Vnesite ustrezen port!");
                return;
            }

            try
            {
                tcpListener = new TcpListener(IPAddress.Parse(localhost), port);
                tcpListener.Start();
                MessageBox.Show($"Poslušam za povezave na vratah: {port}");
                await Task.Run(() => RecieveData());
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Napaka pri vzpostavitvi povezave.");
            }
        }

        private async void RecieveData()
        {
            while (true)
            {
                try
                {
                    var client = await tcpListener.AcceptTcpClientAsync();
                    var stream = client.GetStream();
                    byte[] buffer = new byte[4096];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Received: {receivedData}");
                        HandleData(receivedData);
                    });

                    client.Close();
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => MessageBox.Show($"Napaka pri prejemu podatkov: {ex.Message}"));
                }
            }
        }

        private void HandleData(string data)
        {
            try
            {
                var newBlock = CodeBehind.Block.FromJson(data);
                if (blockchain.ValidateBlock(newBlock, blockchain.Chain.Last()))
                {
                    blockchain.Chain.Add(newBlock);
                    MessageBox.Show("Dodan blok druge instance");
                }
                else
                {
                    MessageBox.Show("Prejeta nepravilna struktura bloka.");
                }
                UpdateBlockchainStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Napak pri obravnavi podatkov: {ex.Message}");
            }
        }

        private async void MineButton_Click(object sender, RoutedEventArgs e)
        {
            string data = NodeNameInput.Text;

            try
            {
                var newBlock = blockchain.AddBlock(data);
                UpdateBlockchainStatus();

                await BroadcastBlock(newBlock);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error mining block: {ex.Message}");
            }
        }
        private async Task BroadcastBlock(CodeBehind.Block block)
        {
            string json = block.ToJson();
            foreach (var peerPort in ConnectPortInput.Text.Split(',').Select(p => int.TryParse(p, out var val) ? val : 0).Where(p => p > 0))
            {
                try
                {
                    using (var client = new TcpClient(localhost, peerPort))
                    using (var stream = client.GetStream())
                    {
                        byte[] data = Encoding.UTF8.GetBytes(json);
                        await stream.WriteAsync(data, 0, data.Length);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error broadcasting to port {peerPort}: {ex.Message}");
                }
            }
        }


        private void UpdateBlockchainStatus()
        {
            if (blockchain.Chain.Any())
            {
                var latestBlock = blockchain.Chain.Last();
                MiningOutput.Text = $"Latest Block:\nIndex: {latestBlock.Index}\nHash: {latestBlock.Hash}\nNonce: {latestBlock.Nonce}\nDifficulty: {latestBlock.Difficulty}\nTimestamp: {latestBlock.Timestamp}";
            }
            else
            {
                MiningOutput.Text = "No blocks mined yet.";
            }
        }
    }
}