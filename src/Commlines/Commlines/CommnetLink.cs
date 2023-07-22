using KSP.Sim;
using System;
using System.Collections.Generic;
using System.Text;

namespace Comlines.Commlines
{
    public class CommnetLink
    {
        public ConnectionGraphNode Node1 { get; private set; }
        public ConnectionGraphNode Node2 { get; private set; }

        public CommnetLink(ConnectionGraphNode node1, ConnectionGraphNode node2)
        {
            Node1 = node1;
            Node2 = node2;
        }
    }
}
