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
        NetworkStream networkStream;
        //Thread thread = null;
        ////private const string hostName = "localhost";
        //string readData = null;
        TcpClient tcpClient = new TcpClient();
        IPAddress ipAddress = IPAddress.Parse("10.2.20.16");
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
            Console.WriteLine("Connecting...");
            try
            {              
                tcpClient.Connect(ipAddress, PortNumber);
                TextChat.Text += Environment.NewLine + "Connected";
                TextChat.Text += Environment.NewLine + "Enter the string to be transmitted";
                networkStream = tcpClient.GetStream();
            }
            catch { }
        }
        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            try {
                string str = UserTextBox.Text + "$";
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
            //client = new TcpClient();
            //readData = "Client Connected to Server.";            
            //TextMessage();
            //client.Connect(hostName, portNumber);
            //networkStream = client.GetStream();
            //byte[] byteTime = Encoding.ASCII.GetBytes(readData);
            //networkStream.Write(byteTime, 0, byteTime.Length);
            //networkStream.Flush();
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
            networkStream = tcpClient.GetStream();
            byte[] bytes = new byte[1024];
            while (true)
            {
                try
                {
                int bytesRead = networkStream.Read(bytes, 0, bytes.Length);
                this.SetText(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                }
                catch { Application.Exit(); }
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
    }
}
