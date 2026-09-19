using UnityEditor;
using UnityEngine.UIElements;

namespace Features.CodeGeneratorModule.CustomCodeGeneratorModule.Scripts.Editor.EditorWindow {
    internal static class CodeGenerationTools {
        internal static VisualTreeAsset GetVisualTreeAsset(string visualTreeAssetName) {
            string[] guids = AssetDatabase.FindAssets(visualTreeAssetName + " t:VisualTreeAsset");
            if (guids.Length > 0) {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            }
            return null;
        }
    }
}