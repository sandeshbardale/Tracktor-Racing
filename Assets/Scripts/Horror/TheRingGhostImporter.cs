using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BhootiyaRasta.Horror
{
    public static class TheRingGhostImporter
    {
        public static GameObject BuildTheRingGhost()
        {
            GameObject ghostRoot = new GameObject("IndianGhost_Bhootni");
            ghostRoot.tag = "Respawn";

            var gc = ghostRoot.AddComponent<GhostController>();

            // GhostBody container for floating & hovering
            GameObject bodyContainer = new GameObject("GhostBodyRoot");
            bodyContainer.transform.SetParent(ghostRoot.transform, false);
            gc.ghostBody = bodyContainer.transform;

            GameObject modelInstance = null;

#if UNITY_EDITOR
            // 1. Try loading the imported GLB model directly
            GameObject glbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Ghost/the_ring_ghost_lady.glb");
            if (glbPrefab != null)
            {
                modelInstance = Object.Instantiate(glbPrefab, bodyContainer.transform);
                modelInstance.name = "TheRingGhostModel";
                modelInstance.transform.localPosition = Vector3.zero;
                // If the GLB is in cm, scale to 0.01 for 1.8m height
                var bounds = CalculateBounds(modelInstance);
                if (bounds.size.y > 10f)
                {
                    modelInstance.transform.localScale = Vector3.one * 0.01f;
                }
                else
                {
                    modelInstance.transform.localScale = Vector3.one;
                }
            }

            // 2. Fallback to the converted OBJ if GLB wasn't loaded
            if (modelInstance == null)
            {
                GameObject objPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Ghost/the_ring_ghost_lady.obj");
                if (objPrefab != null)
                {
                    modelInstance = Object.Instantiate(objPrefab, bodyContainer.transform);
                    modelInstance.name = "TheRingGhostModel_OBJ";
                    modelInstance.transform.localPosition = Vector3.zero;
                    modelInstance.transform.localScale = Vector3.one;
                }
            }
#endif

            // Apply eerie ghost material
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Material ghostDressMat = new Material(litShader);
            ghostDressMat.name = "Mat_TheRing_GhostDress";
            Color ghostlyWhite = new Color(0.82f, 0.85f, 0.88f, 0.92f);
            ghostDressMat.color = ghostlyWhite;
            if (ghostDressMat.HasProperty("_BaseColor")) ghostDressMat.SetColor("_BaseColor", ghostlyWhite);
            if (ghostDressMat.HasProperty("_Smoothness")) ghostDressMat.SetFloat("_Smoothness", 0.05f);

            if (modelInstance != null)
            {
                var renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    r.sharedMaterial = ghostDressMat;
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    r.receiveShadows = true;
                }
            }

            // Eerie Glowing Red Eyes
            GameObject eyesRoot = new GameObject("GhostEyes");
            eyesRoot.transform.SetParent(bodyContainer.transform, false);
            eyesRoot.transform.localPosition = new Vector3(0f, 1.55f, 0.15f);

            GameObject eyeL = new GameObject("EyeLeft");
            eyeL.transform.SetParent(eyesRoot.transform, false);
            eyeL.transform.localPosition = new Vector3(-0.06f, 0f, 0f);
            Light ltL = eyeL.AddComponent<Light>();
            ltL.type = LightType.Point;
            ltL.range = 2.0f;
            ltL.intensity = 1.2f;
            ltL.color = new Color(0.95f, 0.1f, 0.1f);
            gc.eyeLightLeft = ltL;

            GameObject eyeR = new GameObject("EyeRight");
            eyeR.transform.SetParent(eyesRoot.transform, false);
            eyeR.transform.localPosition = new Vector3(0.06f, 0f, 0f);
            Light ltR = eyeR.AddComponent<Light>();
            ltR.type = LightType.Point;
            ltR.range = 2.0f;
            ltR.intensity = 1.2f;
            ltR.color = new Color(0.95f, 0.1f, 0.1f);
            gc.eyeLightRight = ltR;

            // Ghostly Cold Aura Fog Particles
            GameObject auraObj = new GameObject("GhostAuraMist");
            auraObj.transform.SetParent(bodyContainer.transform, false);
            auraObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            ParticleSystem ps = auraObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.6f;
            main.startSpeed = 0.2f;
            main.startSize = 1.4f;
            main.startColor = new Color(0.6f, 0.75f, 0.9f, 0.15f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 12f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.6f;
            gc.auraMist = ps;

            gc.headTransform = eyesRoot.transform;
            gc.ghostRenderers = ghostRoot.GetComponentsInChildren<Renderer>(true);

            return ghostRoot;
        }

        private static Bounds CalculateBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                b.Encapsulate(renderers[i].bounds);
            }
            return b;
        }
    }
}
