using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;



namespace Server
{
    public class Server
    {
        public static Dictionary<String, String> DSACCOUNT = new Dictionary<string, string>();
        public static Dictionary<String, TcpClient> DSConnection = new Dictionary<string, TcpClient>();
        public static Dictionary<String, List<String>> DSNhom = new Dictionary<string, List<string>>();
        public static List<String> list_nhom = new List<String>();
        public static List<String> new_list_nhom;

        static void Main(string[] args)
        {
            IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2009);
            TcpListener server = new TcpListener(iep);
            DSACCOUNT.Add("nam", "111");
            DSACCOUNT.Add("thanh", "222");
            DSACCOUNT.Add("tuan", "333");
            DSACCOUNT.Add("vu", "444");
            server.Start();
            while (true)
            {
                //chap nhan ket noi 
                TcpClient client = server.AcceptTcpClient();
                //Tao ra tuyen moi de xu ly moi Client 
                new ClientThread(client);
            }
            server.Stop();
        }

        class ClientThread : Server
        {
            private Thread tuyen;
            private TcpClient client;
            public ClientThread(TcpClient client)
            {
                this.client = client;
                tuyen = new Thread(new ThreadStart(GuiNhanDL));
                tuyen.Start();
            }

            private void GuiNhanDL()
            {
                StreamReader sr = new StreamReader(client.GetStream());
                StreamWriter sw = new StreamWriter(client.GetStream());
                String User = "";
                try
                {
                    bool thoat = false;
                    while (!thoat)
                    {
                        String kq = sr.ReadLine();
                        String[] mang = kq.Split(':');
                        switch (mang[0])
                        {
                            case "1":
                                if (DSACCOUNT.Keys.Contains(mang[1]) && DSACCOUNT[mang[1]] == mang[2])
                                {
                                    sw.WriteLine("3:OK");
                                    sw.Flush();
                                    Thread.Sleep(500);
                                    if (DSConnection.Keys.Contains(mang[1]))
                                    {
                                        DSConnection[mang[1]].Close();
                                        DSConnection.Remove(mang[1]);
                                    }
                                    DSConnection.Add(mang[1], client);
                                    User = mang[1];
                                    String message = "4";
                                    foreach (string name in DSConnection.Keys)
                                        message += ":" + name;

                                    foreach (string name in DSConnection.Keys)
                                    {
                                        StreamWriter swpartner = new StreamWriter(DSConnection[name].GetStream());
                                        swpartner.WriteLine(message);
                                        swpartner.Flush();
                                    }
                                }
                                else
                                {
                                    thoat = true;
                                    sw.WriteLine("3:NO");
                                }
                                break;
                            case "2":
                                //mang[0]==2
                                //mang[1]==user can goi
                                //mang[2]==message
                                if (DSConnection.Keys.Contains(mang[1]))
                                {
                                    StreamWriter swpartner = new StreamWriter(DSConnection[mang[1]].GetStream());
                                    swpartner.WriteLine("5" + ":" + User + ":" + mang[2]);
                                    swpartner.Flush();
                                }
                                break;
                            case "6":
                                if (mang[2] == "Close")
                                {
                                    thoat = true;
                                    client.Close();
                                    if (User != "" && DSConnection.Keys.Contains(User))
                                        DSConnection.Remove(User);
                                    string msg = "4";
                                    foreach (string name in DSConnection.Keys)
                                        msg += ":" + name;
                                    foreach (string name in DSConnection.Keys)
                                    {
                                        StreamWriter swpartner = new StreamWriter(DSConnection[name].GetStream());
                                        swpartner.WriteLine(msg);
                                        swpartner.Flush();
                                    }
                                }
                                break;
                            case "7":
                                /////7:ThanhVienDSOnline:TenNhom
                                //mang[0] : 7
                                //mamg[1] : ThanhVienDSOnline
                                //mang[2] : TenNhom 
                                //mang[3] : END || NEXT
                                list_nhom.Add(User);
                                list_nhom.Add(mang[1]);
                                new_list_nhom = list_nhom.Distinct().ToList();
                                /* Kiem tra new_list_nhom
                                foreach (String ten in new_list_nhom)
                                {
                                    Console.WriteLine("CHECK_new_list_nhom : {0}", ten);
                                }
                                */
                                if (mang[3] == "END")
                                {
                                    goto case "xu_ly";
                                }
                                break;
                            case "xu_ly":
                                try
                                {
                                    DSNhom.Add(mang[2], new_list_nhom);
                                    /* kiem tra DSnhom
                                    Console.WriteLine("CHECK DSNhom");
                                    foreach (String key in DSNhom.Keys)
                                    {
                                        foreach (String val in DSNhom[key])
                                        {
                                            Console.WriteLine("{0} : {1}", key, val);
                                        }
                                    }
                                    */
                                    String mess = "8" + ":" + mang[2];
                                    for (int i = 0; i < new_list_nhom.Count; i++)
                                    {
                                        mess += ":" + new_list_nhom[i];
                                    }
                                    foreach (String thv in new_list_nhom)
                                    {
                                        StreamWriter swpartner = new StreamWriter(DSConnection[thv].GetStream());
                                        swpartner.WriteLine(mess);
                                        swpartner.Flush();
                                    }
                                    list_nhom = new List<string>();
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("Nhom da ton tai");
                                }
                                
                                break;

                            case "9"://9:Nhom:NoiDungChat
                                //mang[0]: 9
                                //mang[1]: Nhom
                                //mang[2]: NoiDungChat
                                String Nhom_Chat = mang[1];
                                String NoiDungChat = mang[2];
                                if (DSNhom.Keys.Contains(Nhom_Chat))
                                {
                                    foreach (String thanhvien in DSNhom[Nhom_Chat])
                                    {
                                        StreamWriter swpartner = new StreamWriter(DSConnection[thanhvien].GetStream());
                                        swpartner.WriteLine("10" + ":" + Nhom_Chat + ":" + User + ":" + NoiDungChat); //10:Nhom_1 : Nam : abcxyz
                                        swpartner.Flush();
                                    }
                                }
                                else Console.WriteLine("Nhom Khong Ton tai");
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                    if (User != "" && DSConnection.Keys.Contains(User))
                        DSConnection.Remove(User);
                    string msg = "4";
                    foreach (string name in DSConnection.Keys)
                        msg += ":" + name;
                    foreach (string name in DSConnection.Keys)
                    {
                        StreamWriter swpartner = new StreamWriter(DSConnection[name].GetStream());
                        swpartner.WriteLine(msg);
                        swpartner.Flush();
                    }
                }
                client.Close();
            }
        }
    }
}



