using UnityEngine;

namespace MapUtils.CommNet
{
    public static class MaterialManager
    {
        private const string ShaderToUse = "Sprites/Default";
        private static readonly Color Color = Color.green;
        private static Material _material;

        public static Material Material
        {
            get
            {
                if (_material == null)
                {
                    _material = new Material(Shader.Find(ShaderToUse));
                    _material.color = Color;
                }
                
                return _material;
            }
        }
    }
}
