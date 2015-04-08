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
using System.Threading;

namespace WindowsFormsApplication5
{
    public partial class Host_form : Form
    {
        private Thread connectThread = null;
        delegate void SetUpdatePlayers(string name);
        TcpListener myClients;
        Socket s;
        public Host_form()
        {
            InitializeComponent();
            label1.Text += Convert.ToString(GetPublicIP());

            this.connectThread = new Thread(new ThreadStart(this.ListenPort));
            connectThread.Start();
                        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.connectThread.Abort();
            var form1 = (Form1)Tag;
            form1.Show();
            Close();
        }

        public IPAddress GetPublicIP()
        {
           return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }

        private void UpdatePlayers(string name)
        {
            if (this.label2.InvokeRequired)
            {
                SetUpdatePlayers d = new SetUpdatePlayers(UpdatePlayers);
                this.Invoke(d, new object[] { name });
            }
            else
            {
                this.label2.Text += name;
            }
        }

        public void ListenPort()
        {

            

            try
            {
               
                IPAddress ipAd = IPAddress.Parse("0.0.0.0");
                // use local m/c IP address, and 
                // use the same in the client

                /* Initializes the Listener */
                myClients = new TcpListener(ipAd, 8001);

                /* Start Listeneting at the specified port */
                myClients.Start();

                Debug.WriteLine("The server is running at port 8001...");
                Debug.WriteLine("The local End point is  :" +
                                  myClients.LocalEndpoint);
                Debug.WriteLine("Waiting for a connection.....");
                s = myClients.AcceptSocket();
                while (true)
                {
                    
                    Debug.WriteLine("Connection accepted from " + s.RemoteEndPoint);

                    byte[] b = new byte[100];
                    int k = s.Receive(b);
                    Debug.WriteLine("Recieved...");
                    string aux = null;
                    for (int i = 0; i < k; i++)
                    {
                        Debug.Write(Convert.ToChar(b[i]));
                        aux += Convert.ToChar(b[i]);
                    }

                    ASCIIEncoding asen = new ASCIIEncoding();

                    String[] info = aux.Split('?');
                    byte[] buffer;
                    if (info[0] == "connect")
                    {
                        buffer = asen.GetBytes("success");
                        UpdatePlayers(info[1]);
                        
                        s.Send(buffer);
                    }
                    else
                    {
                        buffer = asen.GetBytes("error");
                        s.Send(buffer);
                    }
                }

                /*ASCIIEncoding asen = new ASCIIEncoding();
                s.Send(asen.GetBytes("The string was recieved by the server."));
                Debug.WriteLine("\nSent Acknowledgement");
                //clean up
                s.Close();
                myList.Stop();*/

            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }    
        }

        private void button2_Click(object sender, EventArgs e)
        {
           
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] buffer;
            if (s != null)
            {
                try
                {
                    buffer = asen.GetBytes("start");
                    s.Send(buffer);
                }
                catch
                {
                }

            }

            Game game = new Game(myClients, s);

            game.Show();
            this.connectThread.Abort();
            this.Close();

        }

    }
}
