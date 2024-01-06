using KSP.Sim;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace CommLines.Jobs;

public struct PathConnectivityJob : IJob
{
    [ReadOnly] public NativeArray<ConnectionGraph.ConnectionGraphJobNode> Nodes;
    [ReadOnly] public NativeArray<int> PrevNode;
    [WriteOnly] public NativeArray<float> ConnectionSpeed;

    public void Execute()
    {
        for (var i = 0; i < Nodes.Length; i++)
        {
            var prevIndex = PrevNode[i];
            if (prevIndex < 0 || prevIndex >= Nodes.Length)
            {
                ConnectionSpeed[i] = -1;
                continue;
            }

            var smallestRange = Math.Min(Nodes[i].MaxRange, Nodes[prevIndex].MaxRange);
            var smallestRangeSquared = smallestRange * smallestRange;

            var distSquared = math.distancesq(Nodes[i].Position, Nodes[prevIndex].Position);
            
            ConnectionSpeed[i] = (float)(1 - distSquared / smallestRangeSquared);
        }
    }
}