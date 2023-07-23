using UnityEngine;

namespace Commlines.Commlines
{
    public static class MaterialManager
    {
        private static string ShaderToUse = "Sprites/Default";
        private static Color color = new(50, 249, 0);
        private static Material _material;

        public static Material material
        {
            get
            {
                if (_material == null)
                {
                    _material = new Material(Shader.Find(ShaderToUse));
                    _material.color = color;
                }
                
                return _material;
            }
        }
    }
}
