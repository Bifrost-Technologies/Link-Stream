using System.Net.Sockets;
using System.Net;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.DataProtection;
using LinkStream.Packets;
using System.Diagnostics;
using LinkStream.Client;
using System.Security.Cryptography;

namespace LinkStream.Server
{
    public class SignRequestEventArgs : EventArgs
    {
        public SignRequestEventArgs(string _transactionMessage)
        {
            Message = _transactionMessage;
        }

        public string Message { get; set; }
    }
    public class LinkNetwork
    {
        public string? LinkServiceName { get; set; }
        public bool isOnline { get; set; }
        public bool isLocal { get; set; }
        public Int32 LinkPort { get; set; }
        private TcpListener? LinkServer { get; set; }
        private TcpClient? LinkClient { get; set; }
        private IPAddress LinkServerIP { get; }
        private IDataProtector Protector { get; set; }

        public event EventHandler<SignRequestEventArgs> signatureRequestEvent;

        public LinkNetwork(Int32 _LinkPort, string _LinkServerIP = "127.0.0.1", string _LinkServerName = "")
        {
            LinkServerIP = IPAddress.Parse(_LinkServerIP);
            LinkPort = _LinkPort;
            LinkServiceName = _LinkServerName;  
            IDataProtectionProvider provider = DataProtectionProvider.Create("LinkStream");
            Protector = provider.CreateProtector("GateKeeper");
            //KEEP IT LOCAL for maximum security - Make sure ports being used are not open on your network.
            //If you are using LinkStream for a remote connection between dapps make sure to whitelist IP access to specific ports.
            if (_LinkServerIP == "127.0.0.1")
                isLocal= true;
            else
                isLocal = false;
        }
        public void TriggerSignRequest(string _transactionMessage)
        {
            SignRequestEventArgs requestArgs = new SignRequestEventArgs(_transactionMessage);
            SignRequestEvent(requestArgs);
        }
        protected virtual void SignRequestEvent(SignRequestEventArgs e)
        {
            EventHandler<SignRequestEventArgs> SignEvent = signatureRequestEvent;

            if (SignEvent != null)
                SignEvent(this, e);
        }
        public async Task LinkStream()
        {
            try
            {
                LinkServer = new TcpListener(LinkServerIP, LinkPort);
                Byte[] bytes = new Byte[1400];

                LinkServer.Start();
                while (isOnline == true)
                {
                    LinkClient = await LinkServer.AcceptTcpClientAsync();
                    NetworkStream stream = LinkClient.GetStream();

                    int i = await stream.ReadAsync(bytes, 0, bytes.Length);
                    string data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    string response = string.Empty;
                    string data_decrypted = string.Empty;
                    if (isLocal)
                        data_decrypted = Protector.Unprotect(data);
                    else
                        data_decrypted = data;
                    
                    try
                    {
                       await PacketProcessor.ReadStreamRequest(this, data_decrypted);
                    }
                    catch (Exception packetIssues)
                    {
                        Debug.WriteLine(packetIssues);
                    }
                    //Clears and recycles the client
                    stream.Close();
                    LinkClient.Close();
                    stream.Dispose();
                    LinkClient.Dispose();
                    LinkClient = null;
                }
            }
            catch (Exception ae)
            {
                Console.WriteLine(ae);

            }

        }
    }
}
