using System;
using System.Collections.Generic;
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

namespace Booyco_HMI_Utility
{
    public class WiFiconfig
    {
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

        public List<NetworkDevice> GetAllLocalIPv4(NetworkInterfaceType _type)
        {
            List<NetworkDevice> ipAddrList = new List<NetworkDevice>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
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

        public static List<TCPclient> TCPclients = new List<TCPclient>();

        public static byte[] HeartbeatMessage;

        public void IpWatcherStart()
        {
            Thread newThread = new Thread(new ThreadStart(IPWatch))
            {
                IsBackground = true,
                Name = "IpWatcherThread"
            };
            newThread.Start();
        }

        int prevCount = 0;
        private void IPWatch()
        {
            while (true)
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

        }

        public static string SelectedIP;

        public void ServerRun()
        {           
            HeartbeatMessage = Enumerable.Repeat((byte)0, 522).ToArray();
            HeartbeatMessage[0] = (byte)'[';
            HeartbeatMessage[1] = (byte)'&';
            HeartbeatMessage[2] = (byte)'h';
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

            Thread newThread = new Thread(new ThreadStart(StartServer))
            {
                IsBackground = true,
                Name = "ServerThread"
            };
            newThread.Start();
        }

        IPEndPoint ip;
        Socket socket;
        Socket client;
        public static List<Socket> clients;

        private void StartServer()
        {
            

            IpWatcherStart();
            SelectedIP = "";
            clients = new List<Socket>();
            try
            {
                ip = new IPEndPoint(IPAddress.Any, 13000); //Any IPAddress that connects to the server on any port
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Initialize a new Socket

                socket.Bind(ip); //Bind to the client's IP
                socket.Listen(10); //Listen for maximum 10 connections

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

        private int clientnum = 0;
        private static int pretCount = 0;

        private void GetClients()
        {
            int clientslot = 0;
            GlobalSharedData.ServerStatus = "Waiting for a client...";

            while (clientnum < 10)
            {
                Console.WriteLine("Waiting for a client...");
                try
                {
                    client = socket.Accept();
                    clients.Add(client);
                    IPEndPoint clientep = (IPEndPoint)clients[clientnum].RemoteEndPoint;

                    Console.WriteLine("Connected with {0} at port {1}", clientep.Address, clientep.Port);
                    GlobalSharedData.ServerStatus = "Connected with " + clientep.Address + " at port" + clientep.Port;
                    ClientLsitChanged();

                    Thread readThread = new Thread(() => RecieveBytes(client.RemoteEndPoint))
                    {
                        IsBackground = true,
                        Name = "ServerRecieve:" + clientnum.ToString()
                    };
                    readThread.Start();
                    clientslot = clientnum;
                    Thread sendThread = new Thread(() => SendBytes(client.RemoteEndPoint, clientslot))
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
            GlobalSharedData.ServerStatus = "Maximum amount of clients reached!";

        }
        
        private void RecieveBytes(EndPoint clientnumr)
        {
            int messagecount = 0;
            try
            {
                List<Socket> clientR = clients.Where(t => t.RemoteEndPoint == clientnumr).ToList();
                byte[] data2 = new byte[522];
                int i;
                while (!clientR[0].Poll(10, SelectMode.SelectRead))
                {

                    if ((i = clientR[0].Receive(data2, data2.Length, SocketFlags.None)) != 0)
                    {
                        string recmeg = Encoding.UTF8.GetString(data2, 0, i);
                        Console.WriteLine("Received:" + recmeg + " from: " + clientR[0].RemoteEndPoint + " count:==== "+ messagecount.ToString());
                        messagecount++;
                        GlobalSharedData.ServerStatus = "Received: " + recmeg + " from: " + clientR[0].RemoteEndPoint;

                        if (data2[2] == 'h' /*&& message.Length == 522*/)
                        {
                            #region heartbeatmessage
                            if(!Bootloader.BootReady)
                            {
                                TCPclients.ElementAt(clients.IndexOf(clientR[0])).Name = Encoding.ASCII.GetString(data2, 8, 15);
                                TCPclients.ElementAt(clients.IndexOf(clientR[0])).VID = BitConverter.ToInt32(data2, 4);
                                TCPclients.ElementAt(clients.IndexOf(clientR[0])).FirmRev = data2[23];
                                TCPclients.ElementAt(clients.IndexOf(clientR[0])).FirmSubRev = data2[24];
                                //Thread.Sleep(50);
                                clientR[0].Send(HeartbeatMessage, HeartbeatMessage.Length, SocketFlags.None); //Send the data to the client
                            }
                            
                            //Console.WriteLine("====================heartbeat recieved ======================");
                            #endregion                          
                        }

                        //WiFimessages.Parse(data2, clientnumr);
                        Bootloader.BootloaderParse(data2, clientnumr);

                        data2 = new byte[522];
                    }

                }
                Console.WriteLine("-------------- {0} closed recieve", clientnumr);
                TCPclients.RemoveAt(clients.IndexOf(clientR[0]));
            }
            catch (Exception e)
            {
                Console.WriteLine("-------------- {0} closed recieve", clientnumr);
                Console.WriteLine(e.ToString());
            }

        }

        private void SendBytes(EndPoint clientnumr, int remover)
        {


            List<Socket> clientR = clients.Where(t => t.RemoteEndPoint == clientnumr).ToList();
            clientR[0].Send(HeartbeatMessage, HeartbeatMessage.Length, SocketFlags.None); //Send the data to the client
            byte[] data = new byte[HeartbeatMessage.Length];

            while (!clientR[0].Poll(50, SelectMode.SelectRead))
            {
                try
                {
                    clientR = clients.Where(t => t.RemoteEndPoint == clientnumr).ToList();
                    if ((SelectedIP == clientnumr.ToString() || GlobalSharedData.BroadCast == true) && GlobalSharedData.ServerMessageSend != null && GlobalSharedData.ServerMessageSend != null)
                    {
                        data = new byte[GlobalSharedData.ServerMessageSend.Length];
                        data = GlobalSharedData.ServerMessageSend;
                        clientR[0].Send(data, data.Length, SocketFlags.None); //Send the data to the client
                        //ServerStatus = "Sent: " + ServerMessageSend + " to " + clientR[0].RemoteEndPoint;
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

            Console.WriteLine("-------------- {0} closed send", clientnumr);
            clientR[0].Close();
            clients.Remove(clientR[0]);
            ClientLsitChanged();
            clientnum--;
        }

        public List<TCPclient> ClientLsitChanged()
        {

            try
            {
                if (clients != null && clients.Count != pretCount)
                {
                    List<TCPclient> TCPclientsdumm = new List<TCPclient>();
                    foreach (Socket item in clients)
                    {
                        TCPclientsdumm.Add(new TCPclient() { IP = item.RemoteEndPoint.ToString() });
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

    }   

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
}
