using UnityEngine;

namespace CommNetUtils.CommNet
{
    public static class PluginMaterials
    {
        private const string ShaderToUse = "Sprites/Default";
        private static readonly Color Color = Color.green;

        public static Material CommLineCommLineMaterial { get; private set; }

        public static void GenerateMaterials()
        {
            CommLineCommLineMaterial = new Material(Shader.Find(ShaderToUse));
            CommLineCommLineMaterial.color = Color;
        }
    }
}
