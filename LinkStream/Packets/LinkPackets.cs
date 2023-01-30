using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using LinkStream.Client;
using LinkStream.Types;

namespace LinkStream.Packets
{
    public static class LinkPackets
    {
        public static string CraftPacket(LinkClient _linkClient, PacketTypes packetType, string tx_message)
        {
            int i = (int)packetType;
            switch (i) 
            {
                case 0:
                {
                        if (_linkClient.isLocal && _linkClient.encryptedStream)
                            return _linkClient.EncryptPacket("RequestSignature" + "|" + tx_message);
                        else
                            return "RequestSignature" + "|" + tx_message;
                        
                }
                case 1:
                {
                        if (_linkClient.isLocal && _linkClient.encryptedStream)
                            return _linkClient.EncryptPacket("GetWalletAddress");
                        else
                            return "GetWalletAddress";
                    }
            }
            return "Unknown PacketType";

        }
    }
}
