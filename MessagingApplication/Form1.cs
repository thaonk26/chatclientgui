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
        private const int bufferSize = 2048;
        private static readonly byte[] _buffer = new byte[bufferSize];
        Socket socket;
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
            current.BeginReceive(_buffer, 0, bufferSize, 0, ReceiveData, current);
        }
        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            ConnectToServer();
            ConnectionMessage();
        }
        private void SendButton_Click(object sender, EventArgs e)
        {
            SendData();
        }      
        private void SendData()
        {
            Action<string> DelegateModifyText = ThreadMod;
            string s = UserNameText.Text + ": " + UserTextBox.Text;
            byte[] message = Encoding.ASCII.GetBytes(s);
            Invoke(DelegateModifyText, s);
            socket.Send(message, 0, message.Length, 0);            
            UserTextBox.Clear();
        }
        private void ConnectionMessage()
        {
            try
            {
                string str = "@#" + UserNameText.Text;
                ASCIIEncoding ascii = new ASCIIEncoding();
                byte[] bite = ascii.GetBytes(str);
                TextChat.Text += Environment.NewLine + "Transmitting...";
                socket.Send(bite, 0, bite.Length, 0);
                string Response = Encoding.ASCII.GetString(_buffer);
                TextChat.Text += Environment.NewLine + "Response from server: " + Response;
                socket.Receive(_buffer, 0, _buffer.Length, 0);
                socket.BeginReceive(_buffer, 0, bufferSize, SocketFlags.None, ReceiveData, socket);
            }
            catch { }
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
