using Microsoft.AspNetCore.DataProtection;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

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
        public bool IsOnline { get; set; }
        public bool IsLocal { get; set; }

        public bool EncryptionEnabled { get; set; }
        public Int32 LinkPort { get; set; }
        private TcpListener? LinkServer { get; set; }
        private TcpClient? LinkClient { get; set; }
        private IPAddress LinkServerIP { get; }
        private IDataProtector Protector { get; set; }
        private string OutboundMessage = string.Empty;

        public event EventHandler<SignRequestEventArgs>? SignatureRequestEvent;

        public LinkNetwork(Int32 _LinkPort, string _LinkServerIP = "127.0.0.1", string _LinkServerName = "", bool _encryptionEnabled = false)
        {
            LinkServerIP = IPAddress.Parse(_LinkServerIP);
            LinkPort = _LinkPort;
            LinkServiceName = _LinkServerName;
            EncryptionEnabled = _encryptionEnabled;
            IDataProtectionProvider provider = DataProtectionProvider.Create("LinkStream");
            Protector = provider.CreateProtector("GateKeeper");
            //KEEP IT LOCAL for maximum security - Make sure ports being used are not open on your network.
            //If you are using LinkStream for a remote connection between dapps make sure to whitelist IP access to specific ports.
            if (_LinkServerIP == "127.0.0.1")
                IsLocal = true;
            else
                IsLocal = false;
        }
        public void SetOutboundMessage(string signedMessage)
        {
            OutboundMessage = signedMessage;
        }
        public void TriggerSignRequest(string _transactionMessage)
        {
            SignRequestEventArgs requestArgs = new SignRequestEventArgs(_transactionMessage);
            SignRequestEvent(requestArgs);
        }
        protected virtual void SignRequestEvent(SignRequestEventArgs e)
        {
            if (SignatureRequestEvent != null)
            {
                EventHandler<SignRequestEventArgs> SignEvent = SignatureRequestEvent;
                if (SignEvent != null)
                    SignEvent(this, e);
            }
        }
        public async Task LinkStream(int timeoutInSeconds = 60)
        {
            try
            {
                LinkServer = new TcpListener(LinkServerIP, LinkPort);
                Byte[] bytes = new Byte[1400];

                LinkServer.Start();
                IsOnline = true;
                while (IsOnline)
                {
                    try
                    {
                        LinkClient = await LinkServer.AcceptTcpClientAsync();
                        NetworkStream stream = LinkClient.GetStream();

                        int i = await stream.ReadAsync(bytes, 0, bytes.Length);
                        string data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        string response = string.Empty;
                        string data_decrypted = string.Empty;
                        if (IsLocal && EncryptionEnabled)
                            data_decrypted = Protector.Unprotect(data);
                        else
                            data_decrypted = data;

                        response = PacketProcessor.ReadStreamRequest(this, data_decrypted);
                        if (response == "Transaction request received successfully")
                        {
                            int countdown = timeoutInSeconds * 1000;
                            while (OutboundMessage == string.Empty || countdown > 1000)
                            {
                                await Task.Delay(1000);
                                countdown -= 1000;
                            }
                            if (OutboundMessage != string.Empty)
                            {
                                response = OutboundMessage;
                            }
                        }
                        Byte[] response_data = System.Text.Encoding.ASCII.GetBytes(response);
                        await stream.WriteAsync(response_data, 0, response_data.Length);
                        stream.Close();
                        LinkClient.Close();
                        stream.Dispose();
                        LinkClient.Dispose();
                        LinkClient = null;
                        OutboundMessage = string.Empty;
                    }
                    catch (Exception packetIssues)
                    {
                        Debug.WriteLine(packetIssues);
                    }

                }
            }
            catch (Exception ae)
            {
                Console.WriteLine(ae);

            }

        }
    }
}
