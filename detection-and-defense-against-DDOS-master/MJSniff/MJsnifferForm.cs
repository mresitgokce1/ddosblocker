using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using NetFwTypeLib;

namespace MJsniffer
{

    
    public enum Protocol
    {
        TCP = 6,
        UDP = 17,
        Unknown = -1
    };

    public partial class MJsnifferForm : Form
    {
        const string guidFWPolicy2 = "{E2B3C97F-6AE1-41AC-817A-F6F92166D7DD}";
        const string guidRWRule = "{2C5BC43E-3369-4C33-AB0C-BE9469677AF4}";
        private Socket mainSocket ,mainSocket1;                          //The socket which captures all incoming packets
        private byte[] byteData = new byte[4096];
        private bool bContinueCapturing = false;            //A flag to check if packets are to be captured or not
        int count = 0,dropped = 0,processpacket=1;
        int timerflag = 1 , treshold = 1000;

        public static BizimListeArray bArray = new BizimListeArray();
        
        public MJsnifferForm()
        {
            //Type typeFWPolicy2 = Type.GetTypeFromCLSID(new Guid(guidFWPolicy2));
            //Type typeFWRule = Type.GetTypeFromCLSID(new Guid(guidRWRule));
            //INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(typeFWPolicy2);
            //INetFwRule newRule = (INetFwRule)Activator.CreateInstance(typeFWRule);
            //newRule.Name = "InBound_Rule";
            //newRule.Description = "Block inbound traffic from 192.164.0.0 over TCP port 9999";
            //newRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
            //newRule.LocalPorts = "9999";
            //newRule.RemoteAddresses = "192.164.0.0";
            //newRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            //newRule.Enabled = true;
            //newRule.Grouping = "@firewallapi.dll,-23255";
            //newRule.Profiles = fwPolicy2.CurrentProfileTypes;
            //newRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            //fwPolicy2.Rules.Add(newRule);
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (cmbInterfaces.Text == "")
            {
                MessageBox.Show("Select an Interface to capture the packets.", "MJsniffer", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                if (!bContinueCapturing)        
                {
                    //Start capturing the packets...

                    btnStart.Text = "&Dur";

                    bContinueCapturing = true;

                    //For sniffing the socket to capture the packets has to be a raw socket, with the
                    //address family being of type internetwork, and protocol being IP
                    mainSocket = new Socket(AddressFamily.InterNetwork,
                        SocketType.Raw, ProtocolType.IP);
                    
                    //Bind the socket to the selected IP address
                    mainSocket.Bind(new IPEndPoint(IPAddress.Parse(cmbInterfaces.Text), 0));

                    //Set the socket  options
                    mainSocket.SetSocketOption(SocketOptionLevel.IP,            //Applies only to IP packets
                                               SocketOptionName.HeaderIncluded, //Set the include the header
                                               true);                           //option to true

                    byte[] byTrue = new byte[4] {1, 0, 0, 0};
                    byte[] byOut = new byte[4]{1, 0, 0, 0}; //Capture outgoing packets

                    //Socket.IOControl is analogous to the WSAIoctl method of Winsock 2
                    mainSocket.IOControl(IOControlCode.ReceiveAll,              //Equivalent to SIO_RCVALL constant
                                                                                //of Winsock 2
                                         byTrue,                                    
                                         byOut);

                    //Start receiving the packets asynchronously
                    
                    mainSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                        new AsyncCallback(OnReceive), null);
                }
                else
                {
                    btnStart.Text = "&Dinle";
                    bContinueCapturing = false;
                    //To stop capturing the packets close the socket
                    mainSocket.Close ();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MJsniffer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int nReceived = mainSocket.EndReceive(ar);

                //Analyze the bytes received...

                if (processpacket == 1)
                {
                    ParseData(byteData, nReceived);
                }

                if (processpacket == 0)
                {
                    //update dropped text
                    //processpacket = 1;
                    dropped += 1;
                    
                    label1.Invoke(new MethodInvoker(delegate {
                        label1.Text = dropped.ToString() + " saldırı var";  
                    }));

                    if (timerflag == 1)
                    {
                        System.Timers.Timer mytimer = new System.Timers.Timer(1000);
                        mytimer.Elapsed += (timerSender, timerEvent) => beginsocket(timerSender, timerEvent, null);
                        mytimer.AutoReset = true;
                        mytimer.Enabled = true;
                        timerflag = 0;
                    }
                }


                if (bContinueCapturing)
                {
                    byteData = new byte[4096];

                    //Another call to BeginReceive so that we continue to receive the incoming
                    //packets
                    mainSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                        new AsyncCallback(OnReceive), null);
                }
                else
                {
                     
                }

                    
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "MJsniffer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            
        }


        public void beginsocket(object source, System.Timers.ElapsedEventArgs e, string receiver)
        {
            processpacket = 1;
            bContinueCapturing = true;
            timerflag = 1;
            try
            {
                mainSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                          new AsyncCallback(OnReceive), null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "MJsniffer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnReceive1(IAsyncResult ar)
        {
            try
            {
                int nReceived = mainSocket.EndReceive(ar);

                //Analyze the bytes received...

                ParseData(byteData, nReceived);

                if (bContinueCapturing)
                {
                    byteData = new byte[4096];

                    //Another call to BeginReceive so that we continue to receive the incoming
                    //packets
                    mainSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                        new AsyncCallback(OnReceive), null);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MJsniffer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ParseData(byte[] byteData, int nReceived)
        {
            

            //Since all protocol packets are encapsulated in the IP datagram
            //so we start by parsing the IP header and see what protocol data
            //is being carried by it
            IPHeader ipHeader = new IPHeader(byteData, nReceived);

          
            //Now according to the protocol being carried by the IP datagram we parse 
            //the data field of the datagram
            string[] arr = new string[8];
            
             switch (ipHeader.ProtocolType)
            {
                case Protocol.TCP:

                    TCPHeader tcpHeader = new TCPHeader(ipHeader.Data,              //IPHeader.Data stores the data being 
                                                                                    //carried by the IP datagram
                                                        ipHeader.MessageLength);//Length of the data field                    


                    arr = new string[8];
           
                    if (listView1.InvokeRequired)
                    {
                        listView1.Invoke(new MethodInvoker(delegate
                        {
                            if (false)//ipHeader.DestinationAddress.ToString().CompareTo(cmbInterfaces.SelectedItem.ToString())==1)
                            {

                            }else{
                         
                       ++count;
            arr[0] = count.ToString();
            arr[1] = tcpHeader.SourcePort;
            arr[2] = tcpHeader.DestinationPort;
            arr[3] = ipHeader.SourceAddress.ToString();
            arr[4] = ipHeader.DestinationAddress.ToString();
            arr[5] = "TCP";
            arr[6] = tcpHeader.Flags.ToString();
            //forward packet to server
            //IPAddress ipad = IPAddress.Parse("192.168.0.66");
            //TcpListener mylister = new TcpListener(ipad,25);
            //mylister.Start();
            //Socket ss = mylister.AcceptSocket();
            //ASCIIEncoding asen = new ASCIIEncoding();
            //ss.Send(asen.GetBytes("The string was recieved by the server."));
            //Console.WriteLine("\nSent Acknowledgement");
            
            //mainSocket.SendTo(byteData,);
            checkmalicious(arr[0],arr[1],arr[2],arr[3],arr[4],arr[5],arr[6],arr);
                            }
                        }));
                    }
           

                 

                    break;



                case Protocol.UDP:

                    UDPHeader udpHeader = new UDPHeader(ipHeader.Data,              //IPHeader.Data stores the data being 
                                                                                    //carried by the IP datagram
                                                       (int)ipHeader.MessageLength);//Length of the data field                    

                   
                                                       
                  
                     UDPHeader udp = new UDPHeader(ipHeader.Data,              //IPHeader.Data stores the data being 
                                                                                    //carried by the IP datagram
                                                        ipHeader.MessageLength);//Length of the data field                    


                    arr = new string[8];
           
                    if (listView1.InvokeRequired)
                    {
                        listView1.Invoke(new MethodInvoker(delegate
                        {
                            if (false)//ipHeader.DestinationAddress.ToString().CompareTo(cmbInterfaces.SelectedItem.ToString())==1)
                            {

                            }else{
                         
                       ++count;
            arr[0] = count.ToString();
            arr[1] = udp.SourcePort;
            arr[2] = udp.DestinationPort;
            arr[3] = ipHeader.SourceAddress.ToString();
            arr[4] = ipHeader.DestinationAddress.ToString();
            arr[5] = "UDP";
            arr[6] = "";
            //forward packet to server
            //IPAddress ipad = IPAddress.Parse("192.168.0.66");
            //TcpListener mylister = new TcpListener(ipad,25);
            //mylister.Start();
            //Socket ss = mylister.AcceptSocket();
            //ASCIIEncoding asen = new ASCIIEncoding();
            //ss.Send(asen.GetBytes("The string was recieved by the server."));
            //Console.WriteLine("\nSent Acknowledgement");
            
            //mainSocket.SendTo(byteData,);
            checkmalicious(arr[0],arr[1],arr[2],arr[3],arr[4],arr[5],arr[6],arr);
                            }
                        }));
                    }                 
                                                       
                  break;

                case Protocol.Unknown:
                    break;
            }

           
        }

        private void checkmalicious(string p, string p_2, string p_3,string sip, string dip,string pro,string flags,string[] arr)
        {
        
            if( true )
            {
                if (checkBox1.Checked == true)
                {
                var eklenecek = new BizimListe(sourceIp: sip, destinationIp: dip, zamanDateTime: DateTime.Now);
                bArray.Ekle(eklenecek);
                var sorgu = bArray.Sorgula();
                if (sorgu.Item1 == true) label1.Text = sorgu.Item2.ToString() + " IP Adresinden DoS Saldırısı Var";  
                
                
                }
                ListViewItem itm;
                string[] arr1 = arr;


                if (pro.CompareTo("UDP") == 0)
                {
                    if (p_2.CompareTo("0") == 0 || p_3.CompareTo("0") == 0)
                    {
                        arr1[7] = "UDP PORT 0 ATTACK";
                        itm = new ListViewItem(arr1);
                        listView1.Items.Add(itm);
                        listView1.Items[count - 1].BackColor = Color.Red;
                        listView1.Items[count - 1].ForeColor = Color.White;
                        if (checkBox1.Checked == true)
                        {
                            //mainSocket.Close();

                            bContinueCapturing = false;

                            processpacket = 0;


                        }
                    }
                    else if (count <= treshold)
                    {
                        arr1[7] = "----------------";

                        itm = new ListViewItem(arr1);

                        listView1.Items.Add(itm);

                    }
                    else if (count > treshold)
                    {
                        arr1[7] = "Treshold crossed";

                        itm = new ListViewItem(arr1);

                        listView1.Items.Add(itm);
                        listView1.Items[count - 1].BackColor = Color.Brown;
                        listView1.Items[count - 1].ForeColor = Color.White;
                        if (checkBox1.Checked == true)
                        {
                            //mainSocket.Close();

                            bContinueCapturing = false;

                            processpacket = 0;


                        }
                    }

                }


                //conditions for tcp
                if (pro.CompareTo("TCP") == 0)
                {

                    if (p_2.CompareTo("0") == 0 || p_3.CompareTo("0") == 0)
                    {
                        arr1[7] = "PORT 0 ATTACK";
                        itm = new ListViewItem(arr1);
                        listView1.Items.Add(itm);
                        listView1.Items[count - 1].BackColor = Color.Red;
                        listView1.Items[count - 1].ForeColor = Color.White;
                        if (checkBox1.Checked == true)
                        {
                            //mainSocket.Close();

                            bContinueCapturing = false;

                            processpacket = 0;


                        }


                        //mainSocket = new Socket(AddressFamily.InterNetwork,
                        //        SocketType.Raw, ProtocolType.IP);


                        //mainSocket.Bind(new IPEndPoint(IPAddress.Parse(cmbInterfaces.Text), 0));


                        //mainSocket.SetSocketOption(SocketOptionLevel.IP,            //Applies only to IP packets
                        //                            SocketOptionName.HeaderIncluded, //Set the include the header
                        //                           true);                           //option to true

                        // byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
                        // byte[] byOut = new byte[4] { 1, 0, 0, 0 }; //Capture outgoing packets


                        //mainSocket.IOControl(IOControlCode.ReceiveAll,              //Equivalent to SIO_RCVALL constant

                        //                   byTrue,
                        //                 byOut);


                        //bContinueCapturing = true;


                    }
                    else if (sip.CompareTo("0.0.0.0") == 0)
                    {
                        arr1[7] = "INVALID IP ATTACK";
                        itm = new ListViewItem(arr1);
                        listView1.Items.Add(itm);
                        listView1.Items[count - 1].BackColor = Color.Red;
                        listView1.Items[count - 1].ForeColor = Color.White;
                        if (checkBox1.Checked == true)
                        {
                            bContinueCapturing = false;
                            processpacket = 0;
                        }
                    }
                    else if (flags.Contains("SYN") == true && flags.Contains("FIN") == true)
                    {
                        arr1[7] = "SYN FIN ATTACK";
                        itm = new ListViewItem(arr1);
                        listView1.Items.Add(itm);
                        listView1.Items[count - 1].BackColor = Color.Red;
                        listView1.Items[count - 1].ForeColor = Color.White;
                        if (checkBox1.Checked == true)
                        {
                            bContinueCapturing = false;
                            processpacket = 0;
                        }
                    }
                    else if (flags.Contains("URG") == true && flags.Contains("FIN") == true && flags.Contains("PSH") == true)
                    {
                        arr1[7] = "INVALID FLAG SETTINGS (URG,FIN,PSH)";
                        itm = new ListViewItem(arr1);
                        listView1.Items.Add(itm);
                        listView1.Items[count - 1].BackColor = Color.Red;
                        listView1.Items[count - 1].ForeColor = Color.White;
                        if (checkBox1.Checked == true)
                        {
                            bContinueCapturing = false;
                            processpacket = 0;
                        }
                    }
                    else if (flags.Contains("SYN") == true && flags.Contains("RST") == true)
                    {
                        arr1[7] = "INVALID FLAG SETTINGS (SYN,RST)";
                        itm = new ListViewItem(arr1);
                        listView1.Items.Add(itm);
                        listView1.Items[count - 1].BackColor = Color.Red;
                        listView1.Items[count - 1].ForeColor = Color.White;
                        if (checkBox1.Checked == true)
                        {
                            bContinueCapturing = false;
                            processpacket = 0;
                        }
                    }
                    else if (flags.Contains("SYN") == true && flags.Contains("ACK") == true)
                    {
                        arr1[7] = "INVALID FLAG SETTINGS (SYN,ACK)";
                        itm = new ListViewItem(arr1);
                        listView1.Items.Add(itm);
                        listView1.Items[count - 1].BackColor = Color.Red;
                        listView1.Items[count - 1].ForeColor = Color.White;
                        if (checkBox1.Checked == true)
                        {
                            //bContinueCapturing = false;
                            processpacket = 0;
                        }
                    }
                    else
                    {
                        if (count <= treshold)
                        {
                            arr1[7] = "----------------";

                            itm = new ListViewItem(arr1);

                            listView1.Items.Add(itm);

                        }
                        else if (count > treshold)
                        {
                            arr1[7] = "Treshold crossed";

                            itm = new ListViewItem(arr1);

                            listView1.Items.Add(itm);
                            listView1.Items[count - 1].BackColor = Color.Brown;
                            listView1.Items[count - 1].ForeColor = Color.White;

                        }

                        processpacket = 1;

                    }

                }
            }
            
            }
            

           

        

       

        private void SnifferForm_Load(object sender, EventArgs e)
        {
            

            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.FullRowSelect = true;
            checkBox1.Checked = false;
            //Add column header
            listView1.Columns.Add("Sıra NO.", 75);
            listView1.Columns.Add("Source Port", 100);
            listView1.Columns.Add("Destination Port", 100);
            listView1.Columns.Add("Source IP", 200);
            listView1.Columns.Add("Destination IP", 200);
            listView1.Columns.Add("PROTOKOL", 100);
            listView1.Columns.Add("FLAGS", 150);
            listView1.Columns.Add("REASON", 200);

             


            string strIP = null;

            IPHostEntry HosyEntry = Dns.GetHostEntry((Dns.GetHostName()));
            if (HosyEntry.AddressList.Length > 0)
            {
                foreach (IPAddress ip in HosyEntry.AddressList)
                {
                    strIP = ip.ToString();
                    cmbInterfaces.Items.Add(strIP);
                }
            }            
        }

        private void SnifferForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bContinueCapturing)
            {
                mainSocket.Close();
            }
        }

        private void cmbInterfaces_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

 
}