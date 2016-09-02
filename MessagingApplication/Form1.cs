using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

namespace MessagingApplication
{
    public partial class MessagingForm : Form
    {
        //private const int portNumber = 12000; //need observer pattern
        //private const string hostName = "10.2.20.22";
        delegate void SetTextCallback(string text);
        //TcpClient client;
        //NetworkStream networkStream = default(NetworkStream);
        //Thread thread = null;
        ////private const string hostName = "localhost";
        //string readData = null;
        //string messageData = null;
        //string userData = null;
        private const int _BUFFER_SIZE = 2048;
        private static readonly byte[] _buffer = new byte[_BUFFER_SIZE];
        Socket socket;
        TcpClient tcpClient = new TcpClient();
        IPAddress ipAddress = IPAddress.Loopback;//.Parse("192.168.0.2");
        //IPAddress ipAddress = IPAddress.Parse("10.2.20.14");
        int PortNumber = 12000;
        public MessagingForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void MessagingForm_Load(object sender, EventArgs e)
        {
        }
        private void ConnectToServer()
        {
            TextChat.Text += Environment.NewLine + "Connecting...";
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(new IPEndPoint(ipAddress, PortNumber));
                TextChat.Text += Environment.NewLine + "Connected";
            }
            catch
            {
                MessageBox.Show("Unable to Connect");
            }
        }
        private void sendData()
        {
            byte[] data = Encoding.Default.GetBytes(TextChat.Text);
            socket.Send(BitConverter.GetBytes(data.Length), 0, 4, 0);
            socket.Send(data);
        }
        private void ThreadMod(string mod)
        {
            TextChat.Text += Environment.NewLine + mod;
        }
        private void ReceiveData(IAsyncResult result)
        {
            Socket current = (Socket)result.AsyncState;
            int received;
            Action<string> DelegateModifyText = ThreadMod;
            try
            {
                received = current.EndReceive(result);
            }
            catch (SocketException)
            {
                
                current.Close(); // Dont shutdown because the socket may be disposed and its disconnected anyway
                
                return;
            }
            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Invoke(DelegateModifyText, text);
            current.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveData, current);
        }
        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            //TextChat.Text += Environment.NewLine + "Connecting...";
            //try
            //{              
            //    tcpClient.Connect(ipAddress, PortNumber);
            //    TextChat.Text += Environment.NewLine + "Connected";
            //    TextChat.Text += Environment.NewLine + "Enter the string to be transmitted";
            //    networkStream = tcpClient.GetStream();
            //}
            //catch { }
            ConnectToServer();
            try
            {
                string str = "@#" + UserNameText.Text;
                //Stream stream = tcpClient.GetStream();
                ASCIIEncoding ascii = new ASCIIEncoding();
                byte[] bite = ascii.GetBytes(str);
                TextChat.Text += Environment.NewLine + "Transmitting...";
                socket.Send(bite, 0, bite.Length, 0);
                //stream.Write(bite, 0, bite.Length);
                //byte[] bb = new byte[100];
                //int k = stream.Read(bb, 0, 100);
                string Response = Encoding.ASCII.GetString(_buffer);
                TextChat.Text += Environment.NewLine + "Response from server: " + Response;
                socket.Receive(_buffer, 0, _buffer.Length, 0);
                socket.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveData, socket);

                Thread thread = new Thread(GetMessage);
                thread.Start();
            }
            catch { }
        }
        //private void UpdateGUI()
        //{
        //    if (InvokeRequired)
        //       Invoke(new MethodInvoker(UpdateGUI));
        //    else if (InvokeRequired != true && userData == null)
        //        TextChat.Text = TextChat.Text + Environment.NewLine + messageData;
        //    else
        //    {
        //        TextChat.Text = TextChat.Text + Environment.NewLine + ">> " + messageData;
                
        //    }
        //}
        //private void ConnectLoop()
        //{
        //    int attempts = 0;

        //    while (!tcpClient.Connected)
        //    {
        //        try
        //        {
        //            attempts++;
        //            //tcpClient.Connect("10.2.20.51", 9999);
        //            tcpClient.Connect(ipAddress, PortNumber);
        //        }
        //        catch (SocketException)
        //        {
        //            TextChat.Clear();
        //            TextChat.AppendText("Connection attempts: " + attempts.ToString());
        //        }
        //    }
        //    TextChat.Clear();
        //    messageData = "Conected to Chat Server ...";
        //    //UpdateGUI();
        //    networkStream = tcpClient.GetStream();
        //    ButtonConnect.Visible = false;
        //    SendButton.Visible = true;
        //    UserTextBox.Visible = true;

        //    byte[] outStreamMessage = Encoding.ASCII.GetBytes(UserNameText.Text + "m$m");
        //    networkStream.Write(outStreamMessage, 0, outStreamMessage.Length);
        //    networkStream.Flush();
        //    Thread ctThread = new Thread(GetMessage);
        //    ctThread.Start();
        //}
        private void SendButton_Click(object sender, EventArgs e)
        {
            string s = UserNameText.Text + ": " + UserTextBox.Text;
            byte[] message = Encoding.ASCII.GetBytes(s);
            socket.Send(message, 0, message.Length, 0);
            UserTextBox.Clear();
        }
        public void GetMessage()
        {
            byte[] bytes = new byte[1024];
            while (tcpClient.Connected)
            {
                try
                {
                //networkStream = tcpClient.GetStream();
                int bytesRead = socket.Receive(bytes, 0, bytes.Length, 0);
                this.SetText(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                }
                catch { Application.Exit(); }
                //UpdateGUI();
            }
        }
        private void SetText(string text)
        {
            if (this.UserTextBox.InvokeRequired)
            {
                SetTextCallback setText = new SetTextCallback(SetText);
                this.Invoke(setText, new object[] { text });
            }
            else
            {
                this.TextChat.Text = this.TextChat.Text + "\r\n" + text;
            }
        }
        private void UserTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void UserNameText_TextChanged(object sender, EventArgs e)
        {

        }

        private void TextChat_TextChanged(object sender, EventArgs e)
        {
            TextChat.SelectionStart = TextChat.Text.Length;
            TextChat.ScrollToCaret();
        }
    }
}
