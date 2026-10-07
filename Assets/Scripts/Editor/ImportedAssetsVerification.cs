#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BhootiyaRasta.Editor
{
    public static class ImportedAssetsVerification
    {
        // [InitializeOnLoadMethod]
        private static void AutoVerify()
        {
        }

        // [MenuItem("Bhootiya Rasta/Verify All Imported 3D Models")]
        public static void VerifyAll()
        {
            VerifyModel("Assets/Models/Tractor/tractor.glb");
            VerifyModel("Assets/Models/Tractor/IndianTractor_Chassis.obj");
            VerifyModel("Assets/Models/Tractor/IndianTractor_Wheel_FL.obj");
            VerifyModel("Assets/Models/Tractor/IndianTractor_Wheel_FR.obj");
            VerifyModel("Assets/Models/Tractor/IndianTractor_Wheel_RL.obj");
            VerifyModel("Assets/Models/Tractor/IndianTractor_Wheel_RR.obj");
            VerifyModel("Assets/Models/Road/fps_map_ghost_city.glb");
            VerifyModel("Assets/Models/Ghost/ghost_face_classic_game_model.glb");
            VerifyModel("Assets/Models/Background/low_poly_forest_tree_pack.glb");
        }

        private static void VerifyModel(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                var renderers = prefab.GetComponentsInChildren<Renderer>(true);
                var filters = prefab.GetComponentsInChildren<MeshFilter>(true);
                Debug.Log($"<color=#00FF00><b>[ImportedAsset]</b> LOADED OK: {path} | Renderers: {renderers.Length} | MeshFilters: {filters.Length}</color>");
            }
            else
            {
                Debug.LogError($"<color=#FF0000><b>[ImportedAsset]</b> FAILED TO LOAD: {path}</color>");
            }
        }
    }
}
#endif
