using CommNetUtils.CommLines;
using KSP.Sim;

namespace CommNetUtils.CommNet
{
    public class CommNetLink
    {
        public ConnectionGraphNode Node1 { get; private set; }
        public ConnectionGraphNode Node2 { get; private set; }
        public CommNetMapConnection Connection;

        public CommNetLink(ConnectionGraphNode node1, ConnectionGraphNode node2)
        {
            Node1 = node1;
            Node2 = node2;
        }
    }
}
