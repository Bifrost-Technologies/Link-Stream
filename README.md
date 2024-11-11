# LinkStream
![linkstream-thin-banner](https://user-images.githubusercontent.com/24855008/214090668-e52dfc8f-00a2-47dd-807c-fd4c3cfd8a9a.png)
LinkStream is middleware designed to be integrated into applications or games that use Solana's C# SDK.
Utilizing TCP server & client protocols to send transaction messages from dapps to wallet based applications. 

Local streams can be protected with encryption, while remote usage should be strict with whitelisted IP access. 
Reference the Server/Client Examples provided on github. 

Tranquility is currently the only compatible desktop wallet and can be downloaded here [Tranquility](https://github.com/Bifrost-Technologies/Tranquility)


### Wallet (Server) Example:
```
using LinkStream.Server;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Wallet;

//Initialize the Link Server - Port must match - Every dapp should use a unique port to prevent socket issues. 50505 is an example port.
LinkNetwork _LinkNetwork = new LinkNetwork(50505, _LinkServerName: "Pseudo Wallet App");

//Setting a request handler event is required in order to respond to the packets in the packetprocessor. 
_LinkNetwork.SignatureRequestEvent += HandleRequestEvent;

//When integrating LinkStream into applications force this task to run in the background. In this example it runs on the main thread but ideally you want it to run on its own thread to prevent thread blocking issues.
await _LinkNetwork.LinkStream();
Console.ReadKey();



//Event triggered when a transaction message is requested from a client
async void HandleRequestEvent(object? sender, SignRequestEventArgs e)
{
    //Our pseudo wallet app decodes the instructions and displays it. In a real application the user would click a button and sign the transaction after reading the instructions. In this example we auto sign and send the transaction.
    string _decodedInstructions = PacketProcessor.DecodeTransactionMessage(Convert.FromBase64String(e.Message));
    Console.WriteLine(_decodedInstructions);

    IRpcClient rpcClient = ClientFactory.GetClient(Cluster.MainNet);

    Wallet wallet = new Wallet("", passphrase: "");
    Account signer = wallet.GetAccount(0);

    byte[] transactionMessage = Convert.FromBase64String(e.Message);
    byte[] signedTransaction = signer.Sign(transactionMessage);
    List<byte[]> signatures = new() { signedTransaction };
    Transaction tx = Transaction.Populate(Message.Deserialize(transactionMessage), signatures);

    _LinkNetwork.SetOutboundMessage(Convert.ToBase64String(tx.Serialize()));
    await Task.CompletedTask;
}

```


### DApp (Client) Example
```
using LinkStream.Client;
using LinkStream.Packets;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Wallet;

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

//Retrieve serialized tx and send it to an RPC to be processed etc
string tx = await linkClient.SendPacket(LinkPackets.CraftPacket(linkClient, LinkStream.Types.PacketTypes.RequestSignature, Convert.ToBase64String(transactionMessage)));
Console.WriteLine(tx);


Console.ReadKey();
```
