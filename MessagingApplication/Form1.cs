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
        NetworkStream networkStream = default(NetworkStream);
        //Thread thread = null;
        ////private const string hostName = "localhost";
        //string readData = null;
        string messageData = null;
        string userData = null;
        Socket socket;
        TcpClient tcpClient = new TcpClient();
        //IPAddress ipAddress = IPAddress.Loopback;//.Parse("192.168.0.2");
        IPAddress ipAddress = IPAddress.Parse("192.168.0.2");
        int PortNumber = 12000;
        public MessagingForm()
        {
            InitializeComponent();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void MessagingForm_Load(object sender, EventArgs e)
        {
        }
        private void Connect()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12000));
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
        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            TextChat.Text += Environment.NewLine + "Connecting...";
            try
            {              
                tcpClient.Connect(ipAddress, PortNumber);
                TextChat.Text += Environment.NewLine + "Connected";
                TextChat.Text += Environment.NewLine + "Enter the string to be transmitted";
                networkStream = tcpClient.GetStream();
            }
            catch { }
            try {
                string str = "@#" + UserNameText.Text;
                Stream stream = tcpClient.GetStream();
                ASCIIEncoding ascii = new ASCIIEncoding();
                byte[] bite = ascii.GetBytes(str);
                TextChat.Text += Environment.NewLine + "Transmitting...";
                stream.Write(bite, 0, bite.Length);
                byte[] bb = new byte[100];
                //int k = stream.Read(bb, 0, 100);
                string Response = Encoding.ASCII.GetString(bb);
                TextChat.Text += Environment.NewLine + "Response from server: " + Response;
                Thread thread = new Thread(GetMessage);
                thread.Start();
            }
            catch { }
        }
        private void UpdateGUI()
        {
            if (InvokeRequired)
               Invoke(new MethodInvoker(UpdateGUI));
            else if (InvokeRequired != true && userData == null)
                TextChat.Text = TextChat.Text + Environment.NewLine + messageData;
            else
            {
                TextChat.Text = TextChat.Text + Environment.NewLine + ">> " + messageData;
                
            }
        }
        private void ConnectLoop()
        {
            int attempts = 0;

            while (!tcpClient.Connected)
            {
                try
                {
                    attempts++;
                    //tcpClient.Connect("10.2.20.51", 9999);
                    tcpClient.Connect(ipAddress, PortNumber);
                }
                catch (SocketException)
                {
                    TextChat.Clear();
                    TextChat.AppendText("Connection attempts: " + attempts.ToString());
                }
            }
            TextChat.Clear();
            messageData = "Conected to Chat Server ...";
            //UpdateGUI();
            networkStream = tcpClient.GetStream();
            ButtonConnect.Visible = false;
            SendButton.Visible = true;
            UserTextBox.Visible = true;

            byte[] outStreamMessage = Encoding.ASCII.GetBytes(UserNameText.Text + "m$m");
            networkStream.Write(outStreamMessage, 0, outStreamMessage.Length);
            networkStream.Flush();
            Thread ctThread = new Thread(GetMessage);
            ctThread.Start();
        }
        private void SendButton_Click(object sender, EventArgs e)
        {
            string s = UserNameText.Text + ": " + UserTextBox.Text;
            networkStream = tcpClient.GetStream();
            byte[] message = Encoding.ASCII.GetBytes(s);
            networkStream.Write(message, 0, message.Length);
            networkStream.Flush();
            UserTextBox.Clear();
        }
        public void GetMessage()
        {
            byte[] bytes = new byte[1024];
            while (tcpClient.Connected)
            {
                try
                {
                networkStream = tcpClient.GetStream();
                int bytesRead = networkStream.Read(bytes, 0, bytes.Length);
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
