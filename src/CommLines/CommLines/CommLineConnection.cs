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

        private Map3DFocusItem _mapItem1;
        private Map3DFocusItem _mapItem2;

        private LineRenderer _renderer;
        private bool _initialized;
        
        public void Update()
        {
            if (!_initialized)
            {
                return;
            }

            // Keep updating the position each frame with the map position
            var positions = new[] { _mapItem1.transform.position, _mapItem2.transform.position };
            _renderer.SetPositions(positions);
        }

        private void OnDestroy()
        {
            // When this object gets destroyed, make sure the CommLineManager knows about it
            CommLineManager.Destroyed(this);
        }

        public void Setup(Map3DFocusItem node1, Map3DFocusItem node2)
        {
            // Set the data
            Source = node1.AssociatedMapItem.SimGUID;
            Target = node2.AssociatedMapItem.SimGUID;

            _mapItem1 = node1;
            _mapItem2 = node2;

            // Create the line renderer
            _renderer = gameObject.AddComponent<LineRenderer>();
            _renderer.material = PluginMaterials.CommLineCommLineMaterial;
            _renderer.widthMultiplier = Width;
            _renderer.positionCount = 2;
            
            var positions = new[] { _mapItem1.transform.position, _mapItem2.transform.position };
            _renderer.SetPositions(positions);

            _initialized = true; // Only now allow the renderer to update
        }

        public void SetColor(Color color)
        {
            if (!_initialized)
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
