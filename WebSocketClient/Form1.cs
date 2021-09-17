using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WSKClient
{
    public partial class Form2 : Form
    {
        ClientWebSocket websocketClient = new ClientWebSocket();
        CancellationTokenSource cancellationToken = new CancellationTokenSource();

        System.Threading.Tasks.Task connection;
        public Form2()
        {
            InitializeComponent();
        }

        private void btnKetNoi_Click(object sender, EventArgs e)
        {
            try
            {
                connection = websocketClient.ConnectAsync(
                   new Uri("ws://127.0.0.1:2010"),
                   cancellationToken.Token);
                // MessageBox.Show(connection.GetType().FullName);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            websocketClient.CloseAsync(
               WebSocketCloseStatus.NormalClosure,
               String.Empty,
               cancellationToken.Token);

            // this will cause OnError on the server if not closed first
            cancellationToken.Cancel();
            this.Close();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            connection.ContinueWith(async tsk =>
            {
                // sends a string/text message causes OnMessage to be called
                await websocketClient.SendAsync(
                  new ArraySegment<byte>(Encoding.UTF8.GetBytes(txtGui.Text)),
                  WebSocketMessageType.Text,
                  true,
                  cancellationToken.Token);

                // receives a string/text from the server
                var buffer = new byte[128];
                await websocketClient.ReceiveAsync(
                  new ArraySegment<byte>(buffer), cancellationToken.Token);
                txtNhan.Text += Environment.NewLine + Encoding.UTF8.GetString(buffer) + Environment.NewLine;

            });
        }
    }
}
