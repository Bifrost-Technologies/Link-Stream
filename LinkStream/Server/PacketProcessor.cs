using Solnet.Programs;
using Solnet.Rpc.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkStream.Server
{
    public static class PacketProcessor
    {
        public static async Task<string> ReadStreamRequest(LinkNetwork _LinkServer, string data)
        {
            try
            {
                string[] data_received = data.Split('|');
                if (data_received[0] == "RequestSignature")
                {
                    string transactionMessage = data_received[1];
                    _LinkServer.TriggerSignRequest(transactionMessage);
                    return "Transaction request received successfully";
                }
                else
                {
                    return "Packet is invalid";
                }
            }
            catch (Exception hk)
            {
                Console.WriteLine(hk);
                return "Error occured during packet processing";
            }
            await Task.CompletedTask;
        }
        public static string DecodeTransactionMessage(ReadOnlySpan<byte> messageData)
        {
            List<DecodedInstruction> ix = InstructionDecoder.DecodeInstructions(Message.Deserialize(messageData));

            string aggregate = ix.Aggregate(
                "",
                (s, instruction) =>
                {
                    s += $"\n\tProgram: {instruction.ProgramName}\n\t\t\t Instruction: {instruction.InstructionName}\n";
                    return instruction.Values.Aggregate(
                        s,
                        (current, entry) =>
                            current + $"\t\t\t\t{entry.Key} - {Convert.ChangeType(entry.Value, entry.Value.GetType())}\n");
                });
            Debug.WriteLine(aggregate);

            return aggregate;
        }
    }
}
