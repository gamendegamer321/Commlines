using CommLines.CommLines;
using KSP.Sim;

namespace CommLines.CommNet
{
    public class CommNetLink
    {
        public ConnectionGraphNode Node1 { get; private set; }
        public ConnectionGraphNode Node2 { get; private set; }
        public CommLineConnection Connection;

        public CommNetLink(ConnectionGraphNode node1, ConnectionGraphNode node2)
        {
            Node1 = node1;
            Node2 = node2;
        }
    }
}
