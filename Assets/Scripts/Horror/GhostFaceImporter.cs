using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BhootiyaRasta.Horror
{
    public static class GhostFaceImporter
    {
        public static GameObject BuildGhostFace()
        {
            GameObject ghostRoot = new GameObject("IndianGhost_Bhootni");
            ghostRoot.tag = "Respawn";

            var gc = ghostRoot.AddComponent<GhostController>();

            // Body container for hovering & floating
            GameObject bodyContainer = new GameObject("GhostBodyRoot");
            bodyContainer.transform.SetParent(ghostRoot.transform, false);
            gc.ghostBody = bodyContainer.transform;

            GameObject modelInstance = null;

#if UNITY_EDITOR
            // 1. Try loading the imported Ghost Face GLB model
            GameObject glbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Ghost/ghost_face_classic_game_model.glb");
            if (glbPrefab != null)
            {
                modelInstance = Object.Instantiate(glbPrefab, bodyContainer.transform);
                modelInstance.name = "GhostFace_Model";
                modelInstance.transform.localPosition = Vector3.zero;
                // GLB is in centimeters (~197 cm), scale to 0.01 for ~1.97m height
                modelInstance.transform.localScale = Vector3.one * 0.01f;
            }
#endif

            // Fallback to procedural/previous if not found
            if (modelInstance == null)
            {
                Debug.LogWarning("[GhostFaceImporter] GLB model not found, using procedural fallback");
                modelInstance = new GameObject("ProceduralGhostFallback");
                modelInstance.transform.SetParent(bodyContainer.transform, false);
            }

            // Create URP Lit PBR materials
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Material robeMat = new Material(litShader);
            robeMat.name = "Mat_Ghost_BlackRobe";
            robeMat.color = new Color(0.04f, 0.04f, 0.05f); // Deep dark shroud
            if (robeMat.HasProperty("_Smoothness")) robeMat.SetFloat("_Smoothness", 0.08f);

            Material maskMat = new Material(litShader);
            maskMat.name = "Mat_Ghost_WhiteMask";
            maskMat.color = new Color(0.88f, 0.90f, 0.92f); // Bone white mask
            if (maskMat.HasProperty("_Smoothness")) maskMat.SetFloat("_Smoothness", 0.45f);

            var renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                r.receiveShadows = true;
                string rName = r.gameObject.name.ToLower();
                if (rName.Contains("mask"))
                {
                    r.sharedMaterial = maskMat;
                }
                else
                {
                    r.sharedMaterial = robeMat;
                }
            }

            // Eerie Glowing Red Spectral Eyes
            GameObject eyesRoot = new GameObject("GhostEyes");
            eyesRoot.transform.SetParent(bodyContainer.transform, false);
            eyesRoot.transform.localPosition = new Vector3(0f, 1.62f, 0.18f);

            GameObject eyeL = new GameObject("EyeLeft");
            eyeL.transform.SetParent(eyesRoot.transform, false);
            eyeL.transform.localPosition = new Vector3(-0.065f, 0f, 0f);
            Light ltL = eyeL.AddComponent<Light>();
            ltL.type = LightType.Point;
            ltL.range = 2.5f;
            ltL.intensity = 1.6f;
            ltL.color = new Color(1.0f, 0.08f, 0.08f);
            gc.eyeLightLeft = ltL;

            GameObject eyeR = new GameObject("EyeRight");
            eyeR.transform.SetParent(eyesRoot.transform, false);
            eyeR.transform.localPosition = new Vector3(0.065f, 0f, 0f);
            Light ltR = eyeR.AddComponent<Light>();
            ltR.type = LightType.Point;
            ltR.range = 2.5f;
            ltR.intensity = 1.6f;
            ltR.color = new Color(1.0f, 0.08f, 0.08f);
            gc.eyeLightRight = ltR;

            // Trigger collider
            CapsuleCollider col = ghostRoot.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 1.0f, 0f);
            col.radius = 0.8f;
            col.height = 2.2f;

            Debug.Log("<color=#00FF00><b>[GhostFaceImporter]</b> Successfully built Ghost Face Model with Glowing Eyes!</color>");
            return ghostRoot;
        }
    }
}
