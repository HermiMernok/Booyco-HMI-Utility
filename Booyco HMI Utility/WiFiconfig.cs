﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Booyco_HMI_Utility
{
    public class WiFiconfig
    {        
        //get status of the wifi hotspot created by the device
        #region WiFi hotspot
        public List<NetworkDevice> GetAllLocalIPv4(NetworkInterfaceType _type)
        {
            List<NetworkDevice> ipAddrList = new List<NetworkDevice>();
            var NetWorkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface item in NetWorkInterfaces)
            {

                if (item.OperationalStatus == OperationalStatus.Up) //item.NetworkInterfaceType == _type && 
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddrList.Add(new NetworkDevice() { DeviceName = item.Name, DeviceTipe = item.NetworkInterfaceType.ToString(), DeviceIP = ip.Address.ToString() });
                        }
                    }
                }
            }
            return ipAddrList;
        }
        public void WirelessHotspot(string ssid, string key, bool status)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process process = Process.Start(processStartInfo);

            if (process != null)
            {
                if (status)
                {
                    process.StandardInput.WriteLine("netsh wlan set hostednetwork mode=allow ssid=" + ssid + " key=" + key);
                    process.StandardInput.WriteLine("netsh wlan start hostednetwork");
                    process.StandardInput.Close();
                }
                else
                {
                    process.StandardInput.WriteLine("netsh wlan stop hostednetwork");
                    process.StandardInput.Close();
                }
            }
        }
        int prevCount = 0;
        public void IpWatcherStart()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(IPWatch);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
            dispatcherTimer.Start();
        }
        private void IPWatch(object sender, EventArgs e)
        {

            GlobalSharedData.NetworkDevices = GetAllLocalIPv4(NetworkInterfaceType.Ethernet);
            if (GlobalSharedData.NetworkDevices.Count != prevCount)
            {
                prevCount = GlobalSharedData.NetworkDevices.Count;
            }

            if (GlobalSharedData.NetworkDevices.Where(t => t.DeviceName.Contains("Wireless")).Count() > 0)
            {
                GlobalSharedData.WiFiApStatus = "Wifi Accesspoint Up";
            }
            else
            {
                GlobalSharedData.WiFiApStatus = "Wifi Accesspoint Error";
            }

        }
        #endregion

        public static bool ConnectionError = false;
        public static string Hearted = "";
        public static byte[] HeartbeatMessage;

        #region TCP server
        public void ServerRun()
        {
            #region HeartbeatCreation
            HeartbeatMessage = Enumerable.Repeat((byte)0, 522).ToArray();
            HeartbeatMessage[0] = (byte)'[';
            HeartbeatMessage[1] = (byte)'&';
            HeartbeatMessage[2] = (byte)'B';
            HeartbeatMessage[3] = (byte)'h';
            HeartbeatMessage[4] = (byte)'e';
            HeartbeatMessage[5] = (byte)'a';
            HeartbeatMessage[6] = (byte)'r';
            HeartbeatMessage[7] = (byte)'t';
            HeartbeatMessage[8] = (byte)'b';
            HeartbeatMessage[9] = (byte)'e';
            HeartbeatMessage[10] = (byte)'a';
            HeartbeatMessage[11] = (byte)'t';
            HeartbeatMessage[521] = (byte)']';
            #endregion

            Thread newThread = new Thread(new ThreadStart(StartServer))
            {
                IsBackground = true,
                Name = "ServerThread"
            };
            newThread.Start();
        }
        
        #region TCP server dependants
        IPEndPoint ip;
        TcpListener server;
        Socket socket;
        TcpClient client;
        private DispatcherTimer dispatcherTimer;
        public static bool endAll = false;
        public static string SelectedIP;
        public static List<TcpClient> clients;
        public static List<TCPclient> TCPclients = new List<TCPclient>();
        private int clientnum = 0;
        private static int pretCount = 0;
        #endregion

        static void ConfigureTcpSocket(Socket tcpSocket)
        {
            // Don't allow another socket to bind to this port.
            tcpSocket.ExclusiveAddressUse = true;

            //tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);


            //byte[] InValue = new byte[12];
            //bool ON_ = true;
            //uint KeepInetrVal = 1000;
            //uint KeepLive = 5000;

            //byte[] ON_bytes = BitConverter.GetBytes(Convert.ToUInt32(ON_));
            //byte[] KeepInetrVal_bytes = BitConverter.GetBytes(Convert.ToUInt32(KeepInetrVal));
            //byte[] KeepLive_bytes = BitConverter.GetBytes(Convert.ToUInt32(KeepLive));

            //Array.Copy(ON_bytes, InValue, 4);
            //Array.Copy(KeepLive_bytes, 0, InValue, 4, 4);
            //Array.Copy(KeepInetrVal_bytes, 0, InValue, 8, 4);

            //tcpSocket.IOControl(IOControlCode.KeepAliveValues, InValue, null);

            // Disable the Nagle Algorithm for this tcp socket.
            tcpSocket.NoDelay = true;

            // Set the receive buffer size to 8k
            //tcpSocket.ReceiveBufferSize = 8192;

            //           // Set the timeout for synchronous receive methods to 
            //           // 1 second (1000 milliseconds.)
            //tcpSocket.ReceiveTimeout = 3000;

            // Set the send buffer size to 8k.
            //tcpSocket.SendBufferSize = 8192;

            //           // Set the timeout for synchronous send methods
            //           // to 1 second (1000 milliseconds.)			
            //           tcpSocket.SendTimeout = 3000;

            // Set the Time To Live (TTL) to 42 router hops.
            //tcpSocket.Ttl = 42;

            //Console.WriteLine("Tcp Socket configured:");

            //Console.WriteLine("  ExclusiveAddressUse {0}",
            //            tcpSocket.ExclusiveAddressUse);

            //Console.WriteLine("  LingerState {0}, {1}",
            //             tcpSocket.LingerState.Enabled,
            //                 tcpSocket.LingerState.LingerTime);

            //Console.WriteLine("  NoDelay {0}",
            //                                          tcpSocket.NoDelay);

            //Console.WriteLine("  ReceiveBufferSize {0}",
            //            tcpSocket.ReceiveBufferSize);

            //Console.WriteLine("  ReceiveTimeout {0}",
            //            tcpSocket.ReceiveTimeout);

            //Console.WriteLine("  SendBufferSize {0}",
            //            tcpSocket.SendBufferSize);

            //Console.WriteLine("  SendTimeout {0}",
            //                                          tcpSocket.SendTimeout);

            //Console.WriteLine("  Ttl {0}",
            //                                          tcpSocket.Ttl);

            //Console.WriteLine("  IsBound {0}",
            //                        tcpSocket.IsBound);

            //Console.WriteLine("");
        }

        private void StartServer()
        {

            endAll = false;
            IpWatcherStart();
            SelectedIP = "";
            clients = new List<TcpClient>();
            try
            {
                server = new TcpListener(IPAddress.Any, 13000);
                server.Start();
                //                ip = new IPEndPoint(IPAddress.Any, 13000); //Any IPAddress that connects to the server on any port
                //                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Initialize a new Socket
                //                ConfigureTcpSocket(socket);
                //                socket.Bind(ip); //Bind to the client's IP
                //                socket.Listen(10); //Listen for maximum 10 connections

                Thread ClientsThread = new Thread(new ThreadStart(GetClients))
                {
                    IsBackground = true,
                    Name = "ClientsThread"
                };
                ClientsThread.Start();

            }
            catch (SocketException e)
            {
                Debug.WriteLine("SocketException: {0}", e);
                var prc = new ProcManager();
                prc.KillByPort(13000);
                Thread.Sleep(20);
                StartServer();
            }
        }
 
        private void GetClients()
        {
            int clientslot = 0;
            GlobalSharedData.ServerStatus = " Server running. Waiting for a client...";

            while (clientnum < 10 && !endAll)
            {
                Console.WriteLine("Waiting for a client...");
                try
                {
                    client = server.AcceptTcpClient();
                    clients.Add(client);
                    IPEndPoint clientep = (IPEndPoint)clients[clientnum].Client.RemoteEndPoint;

                    Console.WriteLine("Connected with {0} at port {1}", clientep.Address, clientep.Port);
                    GlobalSharedData.ServerStatus = "Connected with " + clientep.Address + " at port" + clientep.Port;
                    ClientLsitChanged();

                    Thread readThread = new Thread(() => RecieveBytes(client.Client.RemoteEndPoint))
                    {
                        IsBackground = true,
                        Name = "ServerRecieve:" + clientnum.ToString()
                    };
                    readThread.Start();
                    clientslot = clientnum;
                    Thread sendThread = new Thread(() => ClientSendBytes(client.Client.RemoteEndPoint, clientslot))
                    {
                        IsBackground = true,
                        Name = "ServerSend:" + clientnum.ToString()
                    };
                    sendThread.Start();

                    clientnum++;
                }
                catch
                {
                    Console.WriteLine("Client connection failed...");
                    GlobalSharedData.ServerStatus = "Client connection failed...";
                }

            }

            Console.WriteLine("Maximum amount of clients reached!");
            GlobalSharedData.ServerStatus = "Server closed";

        }

        private void RecieveBytes(EndPoint clientnumr)
        {
            int messagecount = 0;
            int ValidMessages = 0;

            List<TcpClient> clientR = clients.Where(t => t.Client.RemoteEndPoint == clientnumr).ToList();
            byte[] data2 = new byte[522];
            byte[] Buffer = new byte[522];
            int i;
            int count = 0;
            clientR[0].ReceiveTimeout = 15000;
            clientR[0].NoDelay = true;

            NetworkStream stream = clientR[0].GetStream();

            while (clientR[0].Connected && !endAll)
            {
                try
                {
                    if ((i = stream.Read(data2, 0, data2.Length)) != 0)
                    {
                        #region bufferCreator
                        if (i==522)
                        {
                            Array.Copy(data2, Buffer, 522);
                            messagecount++;
                        }
                        else if(count == 0)
                        {                           
                            Array.Copy(data2, 0, Buffer, count, i);
                            count = i;
                        }
                        else if(count>0)
                        {
                            Array.Copy(data2, 0, Buffer, count, 522 - count);
                            count = 0;
                            messagecount++;
                        }
                        
                        #endregion
                        #region Message Debug
                        // string recmeg = Encoding.UTF8.GetString(data2, 0, i);
                        //Console.WriteLine("Received:" + recmeg + " from: " + clientR[0].RemoteEndPoint + " count:==== " + messagecount.ToString());
                        Console.WriteLine("Recieved message: =======: " + messagecount.ToString() + " ============= length: " + i.ToString());
                        #endregion

                        //GlobalSharedData.ServerStatus = "Received: " + recmeg + " from: " + clientR[0].RemoteEndPoint;

                        #region Message Paresers
                        if (data2[0] == '[' && data2[1] == '&' && data2[2] == 'B' && data2[3] == 'h' /*&& Buffer[521] == ']'*/)
                        {
                            ValidMessages++;
                            #region heartbeatmessage
                            //if(!Bootloader.BootReady && !Bootloader.BootStart)
                            //{
                            GlobalSharedData.ServerStatus = "Heartbeat message recieved";
                            TCPclients.ElementAt(clients.IndexOf(clientR[0])).Name = Encoding.ASCII.GetString(Buffer, 8, 15);
                            TCPclients.ElementAt(clients.IndexOf(clientR[0])).VID = BitConverter.ToInt32(Buffer, 4);
                            TCPclients.ElementAt(clients.IndexOf(clientR[0])).FirmRev = Buffer[23];
                            TCPclients.ElementAt(clients.IndexOf(clientR[0])).FirmSubRev = Buffer[24];
                            stream.Write(HeartbeatMessage, 0, HeartbeatMessage.Length); //Send the data to the client                         
                            Console.WriteLine("====================heartbeat recieved ======================:" + ValidMessages.ToString());
                            //}
                            #endregion
                        }
                        else if(Buffer[2] == 'B')
                        {
                            ValidMessages++;
                            Bootloader.BootloaderParse(data2, clientnumr);
                        }
                        #endregion

                        Hearted = " message recieved:" + ValidMessages.ToString() + " of " + messagecount.ToString();
                        
                        //WiFimessages.Parse(data2, clientnumr);

                        data2 = new byte[522];
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("-------------- {0} recieve broke", clientnumr);
                    //Console.WriteLine(e.ToString());
                    break;
                }
            }
            Console.WriteLine("-------------- {0} closed recieve", clientnumr);
            clientR[0].Close();
            clients.Remove(clientR[0]);
            ClientLsitChanged();
            clientnum--;

        }

        private void ClientSendBytes(EndPoint clientnumr, int remover)
        {                    
            try
            {
                List<TcpClient> clientR = clients.Where(t => t.Client.RemoteEndPoint == clientnumr).ToList();
                NetworkStream stream = clientR[0].GetStream();

                //stream.Write(HeartbeatMessage, 0, HeartbeatMessage.Length);

                byte[] data = new byte[HeartbeatMessage.Length];
                int counter = 0;
                while (clientR[0].Connected && !endAll)
                {
                    try
                    {
                        //Send the data to the client
                        clientR = clients.Where(t => t.Client.RemoteEndPoint == clientnumr).ToList();
                        if ((SelectedIP == clientnumr.ToString() || GlobalSharedData.BroadCast == true) && GlobalSharedData.ServerMessageSend != null && GlobalSharedData.ServerMessageSend != null)
                        {
                            data = new byte[GlobalSharedData.ServerMessageSend.Length];
                            data = GlobalSharedData.ServerMessageSend;
                            stream.Write(data, 0, data.Length); //Send the data to the client
                            //ServerStatus = "Sent: " + ServerMessageSend + " to " + clientR[0].RemoteEndPoint;
                            GlobalSharedData.ServerStatus = "Message sent";
                            Console.WriteLine("Sent: {0}", Encoding.UTF8.GetString(GlobalSharedData.ServerMessageSend));
                            GlobalSharedData.ServerMessageSend = null;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("-------------- The sending broke");
                        break;
                    }
                }
            }
            catch
            {
                Console.WriteLine("-------------- {0} closed send", clientnumr);
            }

            Console.WriteLine("-------------- {0} closed send", clientnumr);
            //clientR[0].Close();
            //clients.Remove(clientR[0]);
            //ClientLsitChanged();
            //clientnum--;
        }
        
        public List<TCPclient> ClientLsitChanged()
        {

            try
            {
                if (clients.Count == 0)
                {
                    Bootloader.BootStart = false;
                }
                if (clients != null && clients.Count != pretCount)
                {
                    List<TCPclient> TCPclientsdumm = new List<TCPclient>();
                    foreach (var item in clients)
                    {
                        TCPclientsdumm.Add(new TCPclient() { IP = item.Client.RemoteEndPoint.ToString() });
                    }
                    TCPclients = TCPclientsdumm;
                    pretCount = clients.Count;
                    return TCPclientsdumm;
                }
                else
                    return TCPclients;

                
            }
            catch
            {
                Console.WriteLine("----failed to update client list----");
                return TCPclients;
            }
                                    

        }

        public void ServerStop()
        {
            endAll = true;
            try
            {
                foreach (TcpClient item in clients)
                {
                    item.Close();
                }
                server.Stop();
            }
            catch
            {
                Console.WriteLine("Server close error=========");
            }                       

        }
        #endregion


    }

    #region Open port management
    public class PRC
    {
        public int PID { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
    }
    public class ProcManager
    {
        public void KillByPort(int port)
        {
            var processes = GetAllProcesses();
            if (processes.Any(p => p.Port == port))
                try
                {
                    Process.GetProcessById(processes.First(p => p.Port == port).PID).Kill();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            else
            {
                Console.WriteLine("No process to kill!");
            }
        }

        public List<PRC> GetAllProcesses()
        {
            var pStartInfo = new ProcessStartInfo();
            pStartInfo.FileName = "netstat.exe";
            pStartInfo.Arguments = "-a -n -o";
            pStartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            pStartInfo.UseShellExecute = false;
            pStartInfo.RedirectStandardInput = true;
            pStartInfo.RedirectStandardOutput = true;
            pStartInfo.RedirectStandardError = true;

            var process = new Process()
            {
                StartInfo = pStartInfo
            };
            process.Start();

            var soStream = process.StandardOutput;

            var output = soStream.ReadToEnd();
            if (process.ExitCode != 0)
                throw new Exception("somethign broke");

            var result = new List<PRC>();

            var lines = Regex.Split(output, "\r\n");
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("Proto"))
                    continue;

                var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var len = parts.Length;
                if (len > 2)
                    result.Add(new PRC
                    {
                        Protocol = parts[0],
                        Port = int.Parse(parts[1].Split(':').Last()),
                        PID = int.Parse(parts[len - 1])
                    });


            }
            return result;
        }
    }
    #endregion

    #region Class properties
    public class NetworkDevice : INotifyPropertyChanged
    {
        #region OnProperty Changed
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
        #endregion

        private string _deviceName;

        public string DeviceName
        {
            get { return _deviceName; }
            set { _deviceName = value; OnPropertyChanged("DeviceName"); }
        }

        private string _deviceType;

        public string DeviceTipe
        {
            get { return _deviceType; }
            set { _deviceType = value; OnPropertyChanged("DeviceTipe"); }
        }

        private string _deviceIP;

        public string DeviceIP
        {
            get { return _deviceIP; }
            set { _deviceIP = value; OnPropertyChanged("DeviceIP"); }
        }

    }

    public class TCPclient : INotifyPropertyChanged
    {
        #region OnProperty Changed
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
        #endregion

        private string _IP;

        public string IP
        {
            get { return _IP; }
            set { _IP = value; OnPropertyChanged("IP"); }
        }

        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; OnPropertyChanged("Name"); }
        }

        private int _VID;

        public int VID
        {
            get { return _VID; }
            set { _VID = value; OnPropertyChanged("VID"); }
        }

        private int _FirmRev;

        public int FirmRev
        {
            get { return _FirmRev; }
            set { _FirmRev = value; OnPropertyChanged("FirmRev"); }
        }

        private int _FirmSubRev;

        public int FirmSubRev
        {
            get { return _FirmSubRev; }
            set { _FirmSubRev = value; OnPropertyChanged("FirmSubRev"); }
        }



    }
    #endregion
}
