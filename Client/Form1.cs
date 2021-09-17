using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;


namespace Client
{
    public partial class Form1 : Form
    {
        private static Socket client;
        private static byte[] data = new byte[1024];
        Thread receiver;
        bool thoat = false;
        public Form1()
        {
            InitializeComponent();
            btnSend.Enabled = false;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2009);
            client.BeginConnect(iep, new AsyncCallback(Connected), client);
        }
        void AcceptConn(IAsyncResult iar)
        {
            Socket oldserver = (Socket)iar.AsyncState;
            client = oldserver.EndAccept(iar);
            receiver = new Thread(new ThreadStart(ReceiveData));
            receiver.Start();
            receiver.IsBackground = false;
        }
        void Connected(IAsyncResult iar)
        {
            try
            {
                client.EndConnect(iar);
                receiver = new Thread(new ThreadStart(ReceiveData));
                receiver.Start();
            }
            catch (SocketException)
            {

            }
        }
        void SendData(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            int sent = remote.EndSend(iar);
        }
        void ReceiveData()
        {
            int recv;
            string stringData;
            byte[] message = Encoding.ASCII.GetBytes("1:" + txtuser.Text + ":" + txtpass.Text + Environment.NewLine);
            client.BeginSend(message, 0, message.Length, 0, new AsyncCallback(SendData), client);
            while (!thoat)
            {
                try
                {
                    recv = client.Receive(data);
                    stringData = Encoding.ASCII.GetString(data, 0, recv);
                    stringData = stringData.Replace("\n", "");
                    stringData = stringData.Replace("\r", "");
                    String[] mang = stringData.Split(':');
                    switch (mang[0])
                    {
                        case "3":
                            if (mang[1] == "OK")
                            {
                                btnSend.Enabled = true;
                            }
                            break;
                        case "4": //"4:name"
                            DSACCOUNT.Items.Clear();
                            for (int i = 1; i < mang.Length; i++)
                            {
                                if (mang[i] != txtuser.Text)
                                    DSACCOUNT.Items.Add(mang[i]);
                            }
                            break;
                        case "5": //"5:" + User + ":" + mang[2]
                            txtReceive.Text += mang[1] + " : " + mang[2] + Environment.NewLine;
                            break;
                        case "8": ////8 : TenNhom : TenThanhVien_1 : TenThanhVien_2 : TenThanhVien_3
                                  //mang[0] : 8
                                  //mang[1] : TenNhom
                                  //mang[2] : TenThanhVien
                                  //DSNhom.Items.Clear();
                            for (int i = 1; i < 2; i++)
                            {
                                if (!DSNhom.Items.Contains(mang[i]))
                                {
                                    DSNhom.Items.Add(mang[i]);
                                }
                                else continue;
                            }
                            break;
                        case "10"://10 : Nhom_1 : Nam : abcxyz
                                  //mang[0] : 10
                                  //mang[1] : Nhom_1
                                  //mang[2] : Nam
                                  //mang[3] : abcxyz
                            txtReceive.Text += mang[2] + " gửi tới " + mang[1] + " : " + mang[3] + Environment.NewLine;
                            break;
                    }
                }
                catch (Exception)
                {
                }
            }
            return;                
        }


        private void btnSend_Click(object sender, EventArgs e)
        {
            byte[] message = new byte[1024];
            foreach(String ten in DSACCOUNT.SelectedItems)
            {
                message = Encoding.ASCII.GetBytes("2:" + ten.ToString() + ":" + txtSend.Text + Environment.NewLine);
                client.BeginSend(message, 0, message.Length, 0, new AsyncCallback(SendData), client);
            }
        }

        private void btnTaoNhom_Click(object sender, EventArgs e)
        {

            if (txtTenNhom.Text != "" && !DSNhom.Items.Contains(txtTenNhom.Text))
            {
                // Gửi về Server
                byte[] message = new byte[1024];
                foreach (String ThanhVienDSOnline in DSACCOUNT.SelectedItems)
                {
                    if (DSACCOUNT.SelectedItems.IndexOf(ThanhVienDSOnline) == DSACCOUNT.SelectedItems.Count - 1)
                    {
                        message = Encoding.ASCII.GetBytes("7:" + ThanhVienDSOnline.ToString() + ":" + txtTenNhom.Text + ":END" + Environment.NewLine);
                        client.BeginSend(message, 0, message.Length, 0, new AsyncCallback(SendData), client);
                    }
                    else
                    {
                        //7:ThanhVienDSOnline:TenNhom
                        message = Encoding.ASCII.GetBytes("7:" + ThanhVienDSOnline.ToString() + ":" + txtTenNhom.Text + ":NEXT" + Environment.NewLine);
                        //Lỗi: message = Encoding.ASCII.GetBytes("7:" + txtTenNhom.Text + ":" + ThanhVienDSOnline.ToString() + Environment.NewLine);
                        client.BeginSend(message, 0, message.Length, 0, new AsyncCallback(SendData), client);
                    }
                }                        
            }
            else MessageBox.Show("Tên Nhóm đã tồn tại");
        }

        
        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            thoat = true;
            if (client != null)
            {

                client.Close();
                receiver.Abort();
            }
            //receiver.Suspend();

        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            byte[] message = Encoding.ASCII.GetBytes("6:" + txtuser.Text + ":Close" + Environment.NewLine);
            client.Send(message);
            client.Close();
            thoat = true;
            this.Close();
        }

        /*
        public static String NoiDungGui = "";
        public static String TenNhom = "";
        */
        private void btnChat_Click(object sender, EventArgs e)
        {
            try
            {
                if (DSNhom.SelectedIndex != -1)
                {
                    byte[] message = Encoding.ASCII.GetBytes("9:" + DSNhom.SelectedItem + ":" + txtChat.Text + Environment.NewLine); //9:Nhom:NoiDungChat
                    client.BeginSend(message, 0, message.Length, 0, new AsyncCallback(SendData), client);
                }
                else MessageBox.Show("Vui lòng chọn nhóm");
            }
            catch (Exception)
            {
            }
        }
    }
}