using KSP.Map;
using KSP.Sim;

namespace Comlines.Commlines
{
    public class CommData
    {
        public Map3DFocusItem Item { get; private set; }
        public ConnectionGraphNode Node { get; private set; }

        public CommData(Map3DFocusItem item, ConnectionGraphNode node)
        {
            Item = item;
            Node = node;
        }
    }
}
