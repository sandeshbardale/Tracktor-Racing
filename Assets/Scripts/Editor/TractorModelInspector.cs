#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BhootiyaRasta.Vehicle;

namespace BhootiyaRasta.Editor
{
    public static class TractorModelInspector
    {
        // [InitializeOnLoadMethod]
        private static void AutoInspect()
        {
        }

        // [MenuItem("Bhootiya Rasta/Inspect David Brown Tractor Model")]
        public static void InspectModel()
        {
            var (m0, m1) = DavidBrownTractorImporter.LoadTractorMeshes();
            if (m0 != null && m1 != null)
            {
                Debug.Log($"<color=#00FF00><b>[DavidBrownTractor]</b> Mesh 0: {m0.vertexCount} verts, Bounds: {m0.bounds.size} | Mesh 1: {m1.vertexCount} verts, Bounds: {m1.bounds.size}</color>");
            }
        }
    }
}
#endif
