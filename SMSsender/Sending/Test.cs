using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;

namespace SMSsender.Sending
{
    public class Test
    {
        SerialPort serialPort1;
        int m_iTxtMsgState = 0;
        const int NUM_MESSAGE_STATES = 4;

        public void Start()
        {
            serialPort1 = new SerialPort(GetUSBComPort());

            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
            }

            serialPort1.Open();

            //ThreadStart myThreadDelegate = new ThreadStart(ReceiveAndOutput);
            //Thread myThread = new Thread(myThreadDelegate);
            //myThread.Start();

            this.serialPort1.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
        }

        public void Close()
        {
            serialPort1.Close();
        }

        public void DoWork()
        {
            ProcessMessageState();
        }

        private void SendLine(string sLine)
        {
            serialPort1.Write(sLine);
            sLine = sLine.Replace("\u001A", "");
        }

        private void ProcessMessageState()
        {
            switch (m_iTxtMsgState)
            {
                case 0:
                    m_iTxtMsgState = 1;
                    SendLine("AT\r\n");  //NOTE: SendLine must be the last thing called in all of these!
                    break;

                case 1:
                    m_iTxtMsgState = 2;
                    SendLine("AT+CMGF=1\r\n");

                    break;

                case 2:
                    m_iTxtMsgState = 3;
                    SendLine("AT+CMGW=" + Convert.ToChar(34) + "+48693281675" + Convert.ToChar(34) + "\r\n");
                    break;

                case 3:
                    m_iTxtMsgState = 4;
                    SendLine("A simple demo of SMS text messaging." + Convert.ToChar(26));
                    break;

                case 4:
                    m_iTxtMsgState = 5;

                    break;

                case 5:
                    m_iTxtMsgState = NUM_MESSAGE_STATES;
                    break;
            }
        }

        /* //I don't think this part does anything
        private void serialPort1_DataReceived_1(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            string response = serialPort1.ReadLine();
            this.BeginInvoke(new MethodInvoker(() => textBox1.AppendText(response + "\r\n")));
        }
        */
        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(500);

                char[] msg;
                msg = new char[613];
                int iNumToRead = serialPort1.BytesToRead;

                serialPort1.Read(msg, 0, iNumToRead);

                string response = new string(msg);

                serialPort1.DiscardInBuffer();

                if (m_iTxtMsgState == 4)
                {
                    int pos_cmgw = response.IndexOf("+CMGW:");
                    string cmgw_num = response.Substring(pos_cmgw + 7, 4);
                    SendLine("AT+CMSS=" + cmgw_num + "\r\n");
                    //stop listening to messages received
                }

                if (m_iTxtMsgState < NUM_MESSAGE_STATES)
                {
                    ProcessMessageState();
                }
            }
            catch
            { }
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    m_iTxtMsgState = 0;
        //    DoWork();
        //}

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    string[] sPorts = SerialPort.GetPortNames();
        //    foreach (string port in sPorts)
        //    {
        //        consoleOut.Text += port + "\r\n";
        //    }
        //}

        private string GetUSBComPort()
        {
            string[] sPorts = SerialPort.GetPortNames();
            foreach (string port in sPorts)
            {
                if (port != null)
                {
                    return port;
                }
            }

            return null;
        }

    }
}
