using KSP.Map;
using KSP.Sim.impl;
using UnityEngine;

namespace MapUtils.CommNet
{
    public class CommNetMapConnection : MonoBehaviour
    {
        private const float Width = 0.03f;

        public IGGuid Source { get; private set; }
        public readonly List<IGGuid> Targets = new List<IGGuid>();

        private Map3DFocusItem _sourceData;
        private readonly List<Map3DFocusItem> _targetData = new List<Map3DFocusItem>();

        private LineRenderer _renderer;

        public void Update()
        {
            if (_renderer == null)
            {
                return;
            }

            var positions = new List<Vector3>();
            var localPos = _sourceData.transform.position;

            for (int i = 0; i < _targetData.Count; i++)
            {
                if (i != 0 || _targetData.Count == 1)
                {
                    positions.Add(localPos);
                }

                positions.Add(_targetData[i].transform.position);
            }

            _renderer.positionCount = positions.Count;
            _renderer.SetPositions(positions.ToArray());
        }

        private void OnDestroy()
        {
            CommNetManager.Destroyed(this);
            Destroy(_renderer);
        }

        public void Setup(Map3DFocusItem thisNode, Map3DFocusItem targetNode)
        {
            // Set the data
            Source = thisNode.AssociatedMapItem.SimGUID;
            Targets.Add(targetNode.AssociatedMapItem.SimGUID);

            _sourceData = thisNode;
            _targetData.Add(targetNode);

            // Create the line renderer
            _renderer = gameObject.AddComponent<LineRenderer>();
            _renderer.material = MaterialManager.Material;
            _renderer.widthMultiplier = Width;
        }

        public void Add(Map3DFocusItem targetNode)
        {
            Targets.Add(targetNode.AssociatedMapItem.SimGUID);
            _targetData.Add(targetNode);
        }

        public void Remove(Map3DFocusItem targetNode)
        {
            if (targetNode == null)
            {
                return;
            }

            Targets.Remove(targetNode.AssociatedMapItem.SimGUID);
            _targetData.Remove(targetNode);

            if (Targets.Count == 0)
            {
                Destroy(this);
            }
        }
    }
}
