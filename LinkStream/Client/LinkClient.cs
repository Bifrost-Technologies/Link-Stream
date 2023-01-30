using Microsoft.AspNetCore.DataProtection;
using Org.BouncyCastle.Utilities.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LinkStream.Client
{
    public class LinkClient
    {
        public string LinkClientName { get; set; }
        public Int32 LinkPort { get; set; }
        public bool isLocal { get; set; }
        public bool encryptedStream { get; set; }
        private IDataProtector Protector { get; set; }
        private TcpClient? Client { get; set; }
        private NetworkStream? LinkStream { get; set; }
        private string LinkServerIP { get; set; }

        public LinkClient(Int32 _LinkPort, string _LinkServerIP = "127.0.0.1", string _LinkClientName = "", bool _encryptedStream = false)
        {
            LinkClientName = _LinkClientName;
            LinkServerIP = _LinkServerIP;
            LinkPort = _LinkPort;
            encryptedStream = _encryptedStream;
            IDataProtectionProvider provider = DataProtectionProvider.Create("LinkStream");
            Protector = provider.CreateProtector("GateKeeper");
            if (_LinkServerIP == "127.0.0.1")
                isLocal = true;
            else
                isLocal = false;
        }
        public string EncryptPacket(string message)
        {
            return Protector.Protect(message);
        }

        public async Task<string> SendPacket(string craftedPacket)
        {
            string response = String.Empty;

            try
            {
                Client = new TcpClient(LinkServerIP, LinkPort);

                //Packet is sent to LinkStream Server
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(craftedPacket);
                LinkStream = Client.GetStream();
                await LinkStream.WriteAsync(data, 0, data.Length); 

                //Clear byte array and begin awaiting reading the response
                data = new Byte[256];
                Int32 bytes = await LinkStream.ReadAsync(data, 0, data.Length);
                response = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Debug.WriteLine(response);

                //Clears and recycles the client
                LinkStream.Close();
                Client.Close();
                LinkStream.Dispose();
                Client.Dispose();
                LinkStream = null;
                Client = null;

            }
            catch (ArgumentNullException e)
            {
                Debug.WriteLine("ArgumentNullException: {0}", e);
                response = "Packet Transfer Failed";

            }
            catch (SocketException e)
            {
                Debug.WriteLine("SocketException: {0}", e);
                response = "Packet Transfer Failed - Another client is already linked to the LinkNetwork";
            }

            return response;
        }
    }
}
