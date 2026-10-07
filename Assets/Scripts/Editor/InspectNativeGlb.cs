#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BhootiyaRasta.Editor
{
    public static class InspectNativeGlb
    {
        // [MenuItem("Bhootiya Rasta/Inspect Native GLB Prefab")]
        public static void InspectGlb()
        {
            GameObject glb = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Tractor/david_brown_25d_tractor.glb");
            if (glb == null)
            {
                Debug.LogError("Could not load GLB as GameObject!");
                return;
            }

            GameObject inst = Object.Instantiate(glb);
            var renderers = inst.GetComponentsInChildren<Renderer>();
            Bounds b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            Debug.Log($"<color=#00FFFF><b>[NativeGLB]</b> Bounds Center: {b.center}, Size: {b.size}, Min: {b.min}, Max: {b.max}</color>");
            Object.DestroyImmediate(inst);
        }
    }
}
#endif
