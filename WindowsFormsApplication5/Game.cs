using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Input;
using System.Net.Sockets;
using System.IO;



namespace WindowsFormsApplication5
{
    public partial class Game : Form
    {

        private Thread moveThread = null;
        delegate void SetMoveCallback(int ball_x, int ball_y);
        delegate void SetPlayer2PosCallBack(int p2);
        private Thread listen = null;
        private Bitmap DrawArea;
        private int width;
        private int height;
        private int player1_x;
        private int player2_x;
        private int ball_x;
        private int ball_y;
        private int vel_x;
        private int vel_y;
        private int punt_player1;
        private int punt_player2;
        private TcpClient tcpclnt;
        private Stream stm;
        private TcpListener myClients;
        private Socket s;
        private bool ishost;
        //host constructor
        public Game(TcpListener tcplist, Socket sock)
        {
            ishost = true;
            this.Focus();
            myClients = tcplist;
            s = sock;
            myClients.Start();
            InitializeComponent();
            DrawArea = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = DrawArea;
            width = DrawArea.Width;
            height = DrawArea.Height;
            player1_x = 1;
            player2_x = width - 20;
            ball_x = width / 2 - 5;
            ball_y = height / 2 - 5;
            vel_x = -3;
            vel_y = -2;
            punt_player1 = 0;
            punt_player2 = 0;
            paint_player2(0);
            paint_player1(player1_x);
            paint_ball(ball_x, ball_y);
            label1.Text = "Player 1: " + Convert.ToString(punt_player1) + " points";
            label2.Text = "Player 2: " + Convert.ToString(punt_player2) + " points";
           
            //envia informacion al cliente
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] buffer;
            if (s != null)
            {
                try
                {
                    buffer = asen.GetBytes(player1_x + "?" + player2_x + "?" + ball_x + "?" + ball_y );
                    s.Send(buffer);
                }
                catch
                {
                }

            }


            this.moveThread = new Thread(new ThreadStart(this.MoveBall));
            this.moveThread.Start();

            this.listen = new Thread(new ThreadStart(this.ListenClient));
            this.listen.Start();
        }


        //Client constructor
        public Game(TcpClient tcpclnt, Stream stm)
        {
            ishost = false;
            this.Focus();
            // TODO: Complete member initialization
            this.tcpclnt = tcpclnt;
            this.stm = stm;
            InitializeComponent();
            DrawArea = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = DrawArea;
            width = DrawArea.Width;
            height = DrawArea.Height;

            //recibe informacion del servidor
            String[] info = null;
            try
            {

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = new byte[100];
                


                    byte[] b = new byte[100];
                    int k = stm.Read(b, 0, ba.Length);
                    
                    string aux = null;
                    for (int i = 0; i < k; i++)
                    {
                        aux += Convert.ToChar(b[i]);
                    }
                    info = aux.Split('?');
                
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error..... " + exc.StackTrace);
            }
        


            player1_x = Convert.ToInt32(info[0]);
            player2_x = Convert.ToInt32(info[1]);
            ball_x = Convert.ToInt32(info[2]);
            ball_y = Convert.ToInt32(info[3]);
            vel_x = -3;
            vel_y = -2;
            punt_player1 = 0;
            punt_player2 = 0;
            paint_player2(0);
            paint_player1(player1_x);
            paint_ball(ball_x, ball_y);
            label1.Text = "Player 1: " + Convert.ToString(punt_player1) + " points";
            label2.Text = "Player 2: " + Convert.ToString(punt_player2) + " points";

            this.listen = new Thread(new ThreadStart(this.ListenServer));
            this.listen.Start();
        }

        private void pictureBox1_Paint(object sender,
       System.Windows.Forms.PaintEventArgs e)
        {
            // Declares the Graphics object and sets it to the Graphics object
            // supplied in the PaintEventArgs.
            Graphics graphics;
            // Sets g to a graphics object representing the drawing surface of the
            // control or form g is a member of.
            graphics = Graphics.FromImage(DrawArea);

            graphics.Dispose();

        }

        private void paint_player1(int pos_x)
        {

            using (var graphics = Graphics.FromImage(pictureBox1.Image))
            {

                Pen mypen = new Pen(Color.Red);

                graphics.DrawLine(mypen, pos_x, 5, pos_x + 20, 5);

                pictureBox1.Image = DrawArea;

                graphics.Dispose();
            }
        }

        private void paint_player2(int pos_y)
        {
            using (var graphics = Graphics.FromImage(pictureBox1.Image))
            {
                Pen mypen = new Pen(Color.Green);

                graphics.DrawLine(mypen, pos_y, height - 5, pos_y + 20, height - 5);

                pictureBox1.Image = DrawArea;

                graphics.Dispose();
            }
        }

        private void paint_ball(int ball_x, int ball_y)
        {
            SolidBrush blueBrush = new SolidBrush(Color.Blue);
            using (var g = Graphics.FromImage(pictureBox1.Image))
            {
                g.DrawEllipse(Pens.Blue, ball_x, ball_y, 10, 10);
                g.FillEllipse(blueBrush, ball_x, ball_y, 10, 10);
                pictureBox1.Refresh();
            }

        }

        private void paint(int ball_x, int ball_y, int pos_x, int pos_y)
        {
            DrawArea = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = DrawArea;
            paint_ball(ball_x, ball_y);
            paint_player1(pos_x);
            paint_player2(pos_y);
        }

        public void MoveBall()
        {
            while (true)
            {
                Thread.Sleep(50);

                MoveCallback(ball_x, ball_y);
                ball_x += vel_x;
                ball_y += vel_y;

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] buffer;
                if (s != null)
                {
                    try
                    {
                        //+ player2_x + "?"
                        buffer = asen.GetBytes(player1_x + "?"  + ball_x + "?" + ball_y);
                        s.Send(buffer);
                    }
                    catch
                    {
                    }

                }

            }



        }

        private void MoveCallback(int ball_x, int ball_y)
        {
            if (this.pictureBox1.InvokeRequired)
            {
                SetMoveCallback d = new SetMoveCallback(MoveCallback);
                this.Invoke(d, new object[] { ball_x, ball_y });
            }
            else
            {
                DrawArea = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
                pictureBox1.Image = DrawArea;
                if(ishost){
                    CheckCollisions();
                }

                paint(ball_x, ball_y, player1_x, player2_x);
            }
        }

        private void CheckCollisions()
        {

            if (ball_y <= 5 && player1_x <= ball_x && ball_x <= (player1_x + 20) && vel_y < 0)
            {
                vel_y *= -1;
            }
            else if (ball_y >= height - 15 && player2_x <= ball_x && ball_x <= (player2_x + 20) && vel_y > 0)
            {
                vel_y *= -1;
            }
            else
            {
                if (ball_x >= width - 10 || ball_x <= 5)
                {
                    vel_x *= -1;
                }

                if (ball_y >= height || ball_y <= 5)
                {
                    vel_y *= -1;
                    if (ball_y <= 5)
                    {
                        punt_player2++;
                        label2.Text = "Player 2: " + Convert.ToString(punt_player2) + " points";
                    }
                    else if (ball_y >= height - 10)
                    {
                        punt_player1++;
                        label1.Text = "Player 1: " + Convert.ToString(punt_player1) + " points";
                    }

                }

            }


        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void Game_KeyDown(object sender, KeyEventArgs e)
        {

            if (ishost)
            {
                if (e.KeyCode == Keys.D)
                {
                    if (player1_x < width - 20)
                    {
                        player1_x += 10;
                    }

                }
                if (e.KeyCode == Keys.A)
                {
                    if (player1_x >= 10)
                    {
                        player1_x -= 10;
                    }

                }
            }
            else
            {
                //if client
                if (e.KeyCode == Keys.Right)
                {
                    if (player2_x < width - 20)
                    {
                        player2_x += 10;
                    }

                }
                if (e.KeyCode == Keys.Left)
                {
                    if (player2_x >= 10)
                    {
                        player2_x -= 10;
                    }

                }

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(Convert.ToString(player2_x));
                
                stm = tcpclnt.GetStream();
                stm.Write(ba, 0, ba.Length);



            }
        }


        private void ListenServer()
        {
            String[] info = null;
            ASCIIEncoding asen = new ASCIIEncoding();
            while (true)
            {
                try
                {


                    byte[] ba = new byte[100];



                    byte[] b = new byte[100];
                    int k = stm.Read(b, 0, ba.Length);

                    string aux = null;
                    for (int i = 0; i < k; i++)
                    {
                        aux += Convert.ToChar(b[i]);
                    }
                    info = aux.Split('?');

                }
                catch (Exception exc)
                {
                    Console.WriteLine("Error..... " + exc.StackTrace);
                }



                player1_x = Convert.ToInt32(info[0]);
                //player2_x = Convert.ToInt32(info[1]);
                ball_x = Convert.ToInt32(info[1]);
                ball_y = Convert.ToInt32(info[2]);
                MoveCallback(ball_x, ball_y);
            }
        }

        private void ListenClient()
        {
            while (true)
            {
                                
                byte[] b = new byte[100];
                int k = s.Receive(b);
               
                string aux = null;
                for (int i = 0; i < k; i++)
                {
                    aux += Convert.ToChar(b[i]);
                }

                player2_x = Convert.ToInt32(aux);

            }
        }



    }
}

