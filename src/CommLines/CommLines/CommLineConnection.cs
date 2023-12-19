using CommLines.CommNet;
using KSP.Map;
using KSP.Sim.impl;
using UnityEngine;

namespace CommLines.CommLines
{
    public class CommLineConnection : MonoBehaviour
    {
        private const float Width = 0.03f;

        public IGGuid Source { get; private set; }
        public IGGuid Target { get; private set; }

        private Map3DFocusItem _sourceData;
        private Map3DFocusItem _targetData;

        private LineRenderer _renderer;
        
        public void Update()
        {
            if (_renderer == null)
            {
                return;
            }

            var positions = new[] { _sourceData.transform.position, _targetData.transform.position };
            _renderer.SetPositions(positions);
        }

        private void OnDestroy()
        {
            CommLineManager.Destroyed(this);
            Destroy(_renderer);
        }

        public void Setup(Map3DFocusItem thisNode, Map3DFocusItem targetNode)
        {
            // Set the data
            Source = thisNode.AssociatedMapItem.SimGUID;
            Target = targetNode.AssociatedMapItem.SimGUID;

            _sourceData = thisNode;
            _targetData = targetNode;

            // Create the line renderer
            _renderer = gameObject.AddComponent<LineRenderer>();
            _renderer.material = PluginMaterials.CommLineCommLineMaterial;
            _renderer.widthMultiplier = Width;
            _renderer.positionCount = 2;
            
            var positions = new[] { _sourceData.transform.position, _targetData.transform.position };
            _renderer.SetPositions(positions);
        }

        public void SetColor(Color color)
        {
            if (_renderer == null)
            {
                return;
            }
            
            _renderer.material.color = color;
        }
        
        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
