using Org.BouncyCastle.Utilities;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using LinkStream.Client;
using LinkStream.Packets;
using System.Diagnostics;

//Initialize the LinkStream Client - Match the port to the server. 
LinkClient linkClient = new LinkClient(50505, _LinkClientName: "Pseudo Dapp/Game");

IRpcClient rpcClient = ClientFactory.GetClient(Cluster.MainNet);
PublicKey fromAccount = new PublicKey("ENTER WALLET ADDRESS HERE");
RequestResult<ResponseValue<LatestBlockHash>> blockHash = rpcClient.GetLatestBlockHash();


byte[] transactionMessage = new TransactionBuilder()
    .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
    .SetFeePayer(fromAccount)
    .AddInstruction(MemoProgram.NewMemoV2("LinkStream Client Test!"))
    .CompileMessage();


await linkClient.SendPacket(LinkPackets.CraftPacket(linkClient, LinkStream.Types.PacketTypes.RequestSignature, Convert.ToBase64String(transactionMessage)));
Console.WriteLine(Convert.ToBase64String(transactionMessage));


Console.ReadKey();