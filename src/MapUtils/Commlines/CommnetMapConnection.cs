using Commlines.Commlines;
using KSP.Map;
using KSP.Sim.impl;
using UnityEngine;

namespace Comlines.Commlines
{
    public class CommnetMapConnection : MonoBehaviour
    {
        private const float WIDTH = 0.03f;

        public IGGuid source { get; private set; }
        public readonly List<IGGuid> targets = new List<IGGuid>();

        private Map3DFocusItem sourceData;
        private readonly List<Map3DFocusItem> targetData = new List<Map3DFocusItem>();

        private LineRenderer renderer;

        public void Update()
        {
            if (renderer == null)
            {
                return;
            }

            var positions = new List<Vector3>();
            var localPos = sourceData.transform.position;

            for (int i = 0; i < targetData.Count; i++)
            {
                if (i != 0 || targetData.Count == 1)
                {
                    positions.Add(localPos);
                }

                positions.Add(targetData[i].transform.position);
            }

            renderer.positionCount = positions.Count;
            renderer.SetPositions(positions.ToArray());
        }

        private void OnDestroy()
        {
            CommlineManager.Destroyed(this);
            Destroy(renderer);
        }

        public void Setup(Map3DFocusItem thisNode, Map3DFocusItem targetNode)
        {
            // Set the data
            source = thisNode.AssociatedMapItem.SimGUID;
            targets.Add(targetNode.AssociatedMapItem.SimGUID);

            sourceData = thisNode;
            targetData.Add(targetNode);

            // Create the line renderer
            renderer = gameObject.AddComponent<LineRenderer>();
            renderer.material = MaterialManager.material;
            renderer.widthMultiplier = WIDTH;
        }

        public void Add(Map3DFocusItem targetNode)
        {
            targets.Add(targetNode.AssociatedMapItem.SimGUID);
            targetData.Add(targetNode);
        }

        public void Remove(Map3DFocusItem targetNode)
        {
            if (targetNode == null)
            {
                return;
            }

            targets.Remove(targetNode.AssociatedMapItem.SimGUID);
            targetData.Remove(targetNode);

            if (targets.Count == 0)
            {
                Destroy(this);
            }
        }
    }
}
