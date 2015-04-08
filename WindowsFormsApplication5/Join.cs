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
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace WindowsFormsApplication5
{
   
    
    public partial class Join : Form
    {
        delegate void SetStatusPlayer(string name);
        delegate void StartGameCallBack();
        TcpClient tcpclnt;
        Stream stm;
        private Thread connectThread = null;
        public Join()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var form1 = (Form1)Tag;
            form1.Show();
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (port_box.Text == "" || ip_box.Text == "" || name_box.Text == "")
            {
                MessageBox.Show("Please fill all the required fields.");
            }
            else
            {

                this.connectThread = new Thread(new ThreadStart(this.StartConnection));
                connectThread.Start();
                                
            }
        }

        private void StatusPlayer(string status)
        {
            if (this.label2.InvokeRequired)
            {
                SetStatusPlayer d = new SetStatusPlayer(StatusPlayer);
                this.Invoke(d, new object[] { status });
            }
            else
            {
                this.label4.Text = status;
            }
        }

        private void StartConnection()
        {
            string ip, name;
            int port;
            ip = ip_box.Text;
            port = Convert.ToInt16(port_box.Text);
            name = name_box.Text;

            try
            {
                tcpclnt = new TcpClient();
                Console.WriteLine("Connecting.....");
                IPAddress ipAd = IPAddress.Parse(ip);
                tcpclnt.Connect(ipAd, port);
                // use the ipaddress as in the server program

                Console.WriteLine("Connected");
                Console.Write("string to be transmitted : " + name);

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes("connect?" + name);
                Console.WriteLine("Transmitting.....");
                stm = tcpclnt.GetStream();
                stm.Write(ba, 0, ba.Length);


                while (true)
                {
                    byte[] b = new byte[100];
                    int k = stm.Read(b, 0, ba.Length);
                    Debug.WriteLine("Recieved...");
                    string aux = null;
                    for (int i = 0; i < k; i++)
                    {
                        Debug.Write(Convert.ToChar(b[i]));
                        aux += Convert.ToChar(b[i]);
                    }


                    if (aux == "success")
                    {
                        StatusPlayer("success");
                        
                    }
                    else if (aux == "start")
                    {
                        StartGame();
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error..... " + exc.StackTrace);
            }
        }

        private void StartGame()
        {
            if (this.InvokeRequired)
            {
                StartGameCallBack d = new StartGameCallBack(StartGame);
                this.Invoke(d, new object[] {  });
            }
            else
            {
                Game game = new Game(tcpclnt, stm);
                game.Tag = this;
                game.Show(this);
                game.Focus();
                this.connectThread.Abort();
                Hide();
            }
        }

    }
}
