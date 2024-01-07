using KSP.Sim;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace CommLines.Jobs;

public struct AllConnectionsJob : IJob
{
    [ReadOnly] public NativeArray<ConnectionGraph.ConnectionGraphJobNode> Nodes;
    [WriteOnly] public NativeArray<float> SpeedToPrevious;

    public void Execute()
    {
        for (var i = 0; i < Nodes.Length; i++)
        {
            // Ignore inactive nodes
            if ((Nodes[i].Flags & ConnectionGraphNodeFlags.IsActive) == ConnectionGraphNodeFlags.None) continue;

            var maxDistanceSquared = Nodes[i].MaxRange * Nodes[i].MaxRange;

            for (var j = i + 1; j < Nodes.Length; j++)
            {
                // Ignore inactive nodes
                if ((Nodes[j].Flags & ConnectionGraphNodeFlags.IsActive) == ConnectionGraphNodeFlags.None) continue;

                var distSquared = math.distancesq(Nodes[i].Position, Nodes[j].Position);
                var smallestRangeSquared = Nodes[i].MaxRange < Nodes[j].MaxRange // Get the smallest of the 2
                    ? maxDistanceSquared
                    : Nodes[j].MaxRange * Nodes[j].MaxRange;

                var tableIndex = i * (Nodes.Length - 1) - i * (i + 1) / 2 + (j - 1);
                
                if (distSquared < smallestRangeSquared)
                {
                    SpeedToPrevious[tableIndex] = (float)(1 - distSquared / smallestRangeSquared);
                }
                else
                {
                    SpeedToPrevious[tableIndex] = -1;
                }
            }
        }
    }
}