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