using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Threading;
using System.Windows.Input;
namespace MuiltClientTest.VM
{
    public  class ClinetVM:INotifyPropertyChanged
    {

        IList<TcpClient> allSocket = new List<TcpClient>();

       static  Mutex mlock = new Mutex();

        Thread sendthread;
        Thread recivehread;

        public ClinetVM()
        {
           
        }

        # region property
                string serverAddress="127.0.0.1";

                public string ServerAddress
                {
                    get { return serverAddress; }
                    set { serverAddress = value; }
                }

                int port=8888;

                public int Port
                {
                    get { return port; }
                    set { port = value; }
                }

                 

                int clientCount=3;

                public int ClientCount
                {
                    get { return clientCount; }
                    set { clientCount = value; }
                }

                string sendData="这是一个测试";

                public string SendData
                {
                    get {
                        return sendData;
                    }
                    set
                    {
                        sendData = value;
                        OnPropertyChanged("SendData");
                    }
                }
                string reciveData;

                public string ReciveData
                {
                    get { return reciveData; }
                    set { reciveData = value;
                    OnPropertyChanged("ReciveData");
                    }
                }
                StringBuilder log = new StringBuilder();

                public string Log
                {
                    get
                    {
                        return log.ToString();
                    }
                }


        #endregion

       public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region mothrod

        //记录到日志中
        void loginfo(string strlog)
        {
            log.Append(strlog+System.Environment.NewLine);

            OnPropertyChanged("Log");

            Console.WriteLine("事件更新界面刷新");
        }

        ICommand startcmd = null;
        public ICommand StartCmd
        {
            get 
            {
                if (startcmd == null)
                {
                    startcmd = new baseCmd(StartTest);
                }
                return startcmd;
            }
        }

        ICommand stopCmd = null;
        public ICommand StopCmd
        {
            get 
            {
                if (stopCmd == null)
                {
                    stopCmd = new baseCmd(EndTest);
                }
                return stopCmd;
            }
        }

        private void StartTest()
        {
            if (string.IsNullOrEmpty(serverAddress))
            {
                loginfo("服务器地址不能为空！");
                return;
            }

            IPAddress _serverAddress = null;
            if (!IPAddress.TryParse(serverAddress, out _serverAddress))
            {
                loginfo("不是有效的IP地址！");
                return;
            }


            if (ClientCount > 0)
            {
                for (int i=0; i < clientCount; i++)
                {

                    TcpClient client = new TcpClient();
                    try
                    {
                        client.Connect(serverAddress, port);

                        allSocket.Add(client);
                    }
                    catch (SocketException)
                    {
                        loginfo("无法连接到目标计算机");
                        return;
                    }
                 
                }
                sendthread = new Thread(new ThreadStart(SendDataF));
                sendthread.IsBackground = true;
                sendthread.Start();
                //recivehread = new Thread(new ThreadStart(ReciveDataF));
                //recivehread.Start();
            
            }
            else 
            {
                loginfo("客户端的个数必须大于0");
            }
     
        }


        private void EndTest()
        {
            sendthread.Abort();
           
            //recivehread.Abort();

            allSocket.Clear();
        }

        private void SendDataF()
        {
            while (true)
            {
                Console.WriteLine("我执行了一次");
                if (String.IsNullOrEmpty(SendData))
                {
                    SendData = "测试语句";
                }
                byte[] data = composite(SendData);
                //mlock.WaitOne();
                foreach (TcpClient client in allSocket)
                {
                    if (client.Connected)
                    {
                        NetworkStream stream = client.GetStream();
                        if (stream.CanWrite)
                        {
                            stream.Write(data, 0, data.Length);
                        }
                        loginfo("发送了数据：" + sendData);
                       // stream.Close();
                        if (client.Connected)
                        {
                            loginfo("连接正常：" + sendData);
                        }
                        else
                        {
                            loginfo("连接断了啊！！！：" + sendData);
                        }
                    
                    }
                    else
                    {
                        loginfo("连接中断");
                    }
                }

                byte[] rdata = new byte[1024];
                foreach (TcpClient client in allSocket)
                {
                    if (client.Connected)
                    {
                        NetworkStream stream = client.GetStream();
                        if (stream.CanRead)
                        {
                            int messgele = stream.Read(rdata, 0, rdata.Length);
                            ReciveData = Encoding.Default.GetString(rdata, 0, messgele);
                        }
                        loginfo("收到数据：" + ReciveData);
                       // stream.Close();
                    }
                    else
                    {
                        loginfo("连接中断");
                    }
                }
                //mlock.ReleaseMutex();
                Thread.Sleep(1000);
            }
            
        }

        private void ReciveDataF()
        {
            while (true)
            {
                mlock.WaitOne();
                byte[] data = new byte[1024];
                foreach (TcpClient client in allSocket)
                {
                    if (client.Connected)
                    {
                        NetworkStream stream = client.GetStream();
                        if (stream.CanRead)
                        {
                            int messgele = stream.Read(data, 0, data.Length);
                            ReciveData = Encoding.Default.GetString(data, 0, messgele);
                        }
                        loginfo("收到数据：" + ReciveData);
                        stream.Close();
                    }
                }
                mlock.ReleaseMutex();
                Thread.Sleep(1000);
            }
            
        }

        private byte[] composite(string data)
        {
            byte[] buffer = Encoding.Default.GetBytes(data);
            string cdata = Convert.ToBase64String(buffer);
            byte[] basebuffer = Encoding.Default.GetBytes(cdata);
            byte[] finalbuffer=new byte[basebuffer.Length+23];

            finalbuffer[0] = 13;//开始位
            finalbuffer[1] = Convert.ToByte('A');//功能编码

            //不能超过长度
            string user = "testUser";
            byte[] userbytes = Encoding.Default.GetBytes(user);
            userbytes.CopyTo(finalbuffer, 2);

            //不能超过9999,否则需要改变算法
            byte[] datalens =Encoding.Default.GetBytes(basebuffer.Length.ToString());

            datalens.CopyTo(finalbuffer, 18);
            basebuffer.CopyTo(finalbuffer, 22);
            finalbuffer[finalbuffer.Length - 1] = 10;
            return finalbuffer;
        }

        #endregion
    }
}
