using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BhootiyaRasta.Vehicle
{
    public static class IndianTractorImporter
    {
        private static Material cachedBodyMat;
        private static Material cachedTireMat;
        private static Material cachedRimMat;
        private static Material cachedGlassMat;
        private static Material cachedMetalMat;
        private static Material cachedYellowMat;

        public static GameObject BuildIndianTractor()
        {
            GameObject tractorRoot = new GameObject("IndianTractor_Bhootni");
            tractorRoot.tag = "Player";

            // 1. Rigidbody
            Rigidbody rb = tractorRoot.AddComponent<Rigidbody>();
            rb.mass = 2200f;
            rb.centerOfMass = new Vector3(0f, 0.45f, 0.1f);
            rb.linearDamping = 0.35f;
            rb.angularDamping = 3.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // 2. Tractor Controller & Audio
            TractorController tc = tractorRoot.AddComponent<TractorController>();
            TractorAudio ta = tractorRoot.AddComponent<TractorAudio>();

            // Setup Physics Material (smooth rolling without sticking)
            PhysicsMaterial wheelMat = new PhysicsMaterial("PhysMat_TractorWheel");
            wheelMat.dynamicFriction = 0.08f;
            wheelMat.staticFriction = 0.12f;
            wheelMat.bounciness = 0.0f;
            wheelMat.frictionCombine = PhysicsMaterialCombine.Minimum;
            wheelMat.bounceCombine = PhysicsMaterialCombine.Minimum;

            // Tractor Visual Scale (0.48 scale brings it to ~3.6m length, 1.8m width)
            float visualScale = 0.48f;

            // Wheel Hub Offsets in Root Space (scaled)
            // Raw values from tractor_spec.json:
            // FL: [-1.282, 1.542, 1.831] * 0.48 -> [-0.615, 0.740, 0.879]
            // FR: [ 1.282, 1.542, 1.831] * 0.48 -> [ 0.615, 0.740, 0.879]
            // RL: [-1.346, 1.542, -1.831] * 0.48 -> [-0.646, 0.740, -0.879]
            // RR: [ 1.346, 1.542, -1.831] * 0.48 -> [ 0.646, 0.740, -0.879]
            Vector3 flHub = new Vector3(-0.615f, 0.740f, 0.879f);
            Vector3 frHub = new Vector3( 0.615f, 0.740f, 0.879f);
            Vector3 rlHub = new Vector3(-0.646f, 0.740f, -0.879f);
            Vector3 rrHub = new Vector3( 0.646f, 0.740f, -0.879f);
            Vector3 steerHub = new Vector3(0.0f, 1.64f, -0.09f);

            float flRadius = 1.241f * visualScale; // ~0.60m
            float rlRadius = 1.542f * visualScale; // ~0.74m

            // 3. Chassis Visual Container
            GameObject visualContainer = new GameObject("VisualModel");
            visualContainer.transform.SetParent(tractorRoot.transform, false);
            tc.tractorBody = visualContainer.transform;

            // Load Chassis OBJ
            GameObject chassisMeshObj = LoadModelMesh("Assets/Models/Tractor/IndianTractor_Chassis.obj", "TractorChassisMesh");
            if (chassisMeshObj != null)
            {
                chassisMeshObj.transform.SetParent(visualContainer.transform, false);
                chassisMeshObj.transform.localScale = Vector3.one * visualScale;
                ApplyTractorMaterials(chassisMeshObj);
            }

            // 4. Create 4 Rotating Wheels
            // Front Left
            GameObject flObj = CreateWheelNode("Wheel_FrontLeft", tractorRoot.transform, flHub, visualScale, "Assets/Models/Tractor/IndianTractor_Wheel_FL.obj");
            tc.frontLeftWheel = flObj.transform;

            // Front Right
            GameObject frObj = CreateWheelNode("Wheel_FrontRight", tractorRoot.transform, frHub, visualScale, "Assets/Models/Tractor/IndianTractor_Wheel_FR.obj");
            tc.frontRightWheel = frObj.transform;

            // Rear Left
            GameObject rlObj = CreateWheelNode("Wheel_RearLeft", tractorRoot.transform, rlHub, visualScale, "Assets/Models/Tractor/IndianTractor_Wheel_RL.obj");
            tc.rearLeftWheel = rlObj.transform;

            // Rear Right
            GameObject rrObj = CreateWheelNode("Wheel_RearRight", tractorRoot.transform, rrHub, visualScale, "Assets/Models/Tractor/IndianTractor_Wheel_RR.obj");
            tc.rearRightWheel = rrObj.transform;

            // Steering Wheel
            GameObject steerObj = CreateWheelNode("SteeringWheel", visualContainer.transform, steerHub, visualScale, "Assets/Models/Tractor/IndianTractor_SteeringWheel.obj");
            tc.steeringWheel = steerObj.transform;

            // 5. Physics Colliders
            // SphereColliders under each wheel for rolling
            AddWheelCollider(flObj, flRadius, wheelMat);
            AddWheelCollider(frObj, flRadius, wheelMat);
            AddWheelCollider(rlObj, rlRadius, wheelMat);
            AddWheelCollider(rrObj, rlRadius, wheelMat);

            // Chassis Center BoxCollider (High clearance so it never snags ground)
            BoxCollider chassisCol = tractorRoot.AddComponent<BoxCollider>();
            chassisCol.center = new Vector3(0f, 1.15f, 0.05f);
            chassisCol.size = new Vector3(1.25f, 1.10f, 2.80f);
            chassisCol.sharedMaterial = wheelMat;

            // 6. Dual High-Beam Realistic Halogen Headlights
            List<Light> hLights = new List<Light>();

            // Left Headlight
            GameObject hlLeft = new GameObject("Headlight_L");
            hlLeft.transform.SetParent(visualContainer.transform, false);
            hlLeft.transform.localPosition = new Vector3(-0.48f, 1.28f, 1.62f);
            hlLeft.transform.localRotation = Quaternion.Euler(3.5f, -1.0f, 0f);
            Light ltL = hlLeft.AddComponent<Light>();
            ltL.type = LightType.Spot;
            ltL.range = 80f;
            ltL.spotAngle = 65f;
            ltL.innerSpotAngle = 40f;
            ltL.intensity = 5.5f;
            ltL.color = new Color(1.0f, 0.95f, 0.84f);
            ltL.shadows = LightShadows.Soft;
            ltL.shadowStrength = 0.85f;
            hLights.Add(ltL);

            // Right Headlight
            GameObject hlRight = new GameObject("Headlight_R");
            hlRight.transform.SetParent(visualContainer.transform, false);
            hlRight.transform.localPosition = new Vector3(0.48f, 1.28f, 1.62f);
            hlRight.transform.localRotation = Quaternion.Euler(3.5f, 1.0f, 0f);
            Light ltR = hlRight.AddComponent<Light>();
            ltR.type = LightType.Spot;
            ltR.range = 80f;
            ltR.spotAngle = 65f;
            ltR.innerSpotAngle = 40f;
            ltR.intensity = 5.5f;
            ltR.color = new Color(1.0f, 0.95f, 0.84f);
            ltR.shadows = LightShadows.Soft;
            ltR.shadowStrength = 0.85f;
            hLights.Add(ltR);

            // Forward Ground Spread / Fog Light (Wide road illumination ahead of wheels)
            GameObject hlGlow = new GameObject("Headlight_Glow");
            hlGlow.transform.SetParent(visualContainer.transform, false);
            hlGlow.transform.localPosition = new Vector3(0f, 1.15f, 2.4f);
            Light ltGlow = hlGlow.AddComponent<Light>();
            ltGlow.type = LightType.Point;
            ltGlow.range = 16.0f;
            ltGlow.intensity = 2.8f;
            ltGlow.color = new Color(1.0f, 0.94f, 0.82f);
            hLights.Add(ltGlow);

            // 7. Red Rear Tail Lights
            List<Light> tLights = new List<Light>();
            GameObject tlLeft = new GameObject("Taillight_L");
            tlLeft.transform.SetParent(visualContainer.transform, false);
            tlLeft.transform.localPosition = new Vector3(-0.65f, 1.10f, -1.65f);
            Light ltTL = tlLeft.AddComponent<Light>();
            ltTL.type = LightType.Point;
            ltTL.range = 6.0f;
            ltTL.intensity = 2.0f;
            ltTL.color = new Color(1.0f, 0.08f, 0.08f);
            tLights.Add(ltTL);

            GameObject tlRight = new GameObject("Taillight_R");
            tlRight.transform.SetParent(visualContainer.transform, false);
            tlRight.transform.localPosition = new Vector3(0.65f, 1.10f, -1.65f);
            Light ltTR = tlRight.AddComponent<Light>();
            ltTR.type = LightType.Point;
            ltTR.range = 6.0f;
            ltTR.intensity = 2.0f;
            ltTR.color = new Color(1.0f, 0.08f, 0.08f);
            tLights.Add(ltTR);

            // Assign lights to controller via reflection / properties
            tc.SetLights(hLights.ToArray(), tLights.ToArray());

            Debug.Log("<color=#00FF00><b>[IndianTractorImporter]</b> Successfully built Indian Tractor with 4 Rotating Tyres & Realistic Physics!</color>");
            return tractorRoot;
        }

        private static GameObject CreateWheelNode(string name, Transform parent, Vector3 localPos, float scale, string objPath)
        {
            GameObject wheelHub = new GameObject(name);
            wheelHub.transform.SetParent(parent, false);
            wheelHub.transform.localPosition = localPos;

            GameObject meshChild = LoadModelMesh(objPath, name + "_Mesh");
            if (meshChild != null)
            {
                meshChild.transform.SetParent(wheelHub.transform, false);
                meshChild.transform.localPosition = Vector3.zero;
                meshChild.transform.localScale = Vector3.one * scale;
                ApplyTractorMaterials(meshChild);
            }

            return wheelHub;
        }

        private static void AddWheelCollider(GameObject wheelObj, float radius, PhysicsMaterial physMat)
        {
            SphereCollider sc = wheelObj.AddComponent<SphereCollider>();
            sc.radius = radius;
            sc.center = Vector3.zero;
            sc.sharedMaterial = physMat;
        }

        private static GameObject LoadModelMesh(string relativePath, string gameObjectName)
        {
            GameObject modelPrefab = null;
#if UNITY_EDITOR
            modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
#endif
            if (modelPrefab != null)
            {
                GameObject inst = Object.Instantiate(modelPrefab);
                inst.name = gameObjectName;
                return inst;
            }

            Debug.LogWarning($"[IndianTractorImporter] Could not load OBJ at: {relativePath}");
            return null;
        }

        private static void ApplyTractorMaterials(GameObject root)
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ??
                               Shader.Find("URP/Lit") ??
                               Shader.Find("Universal Render Pipeline/Simple Lit") ??
                               Shader.Find("Standard") ??
                               Shader.Find("Mobile/Diffuse");

            Texture2D diffuseTex = null;
            Texture2D normalTex = null;
            Texture2D metallicTex = null;

#if UNITY_EDITOR
            Material savedMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Tractor_PBR.mat");
            if (savedMat != null)
            {
                cachedBodyMat = savedMat;
            }
#endif
            if (cachedBodyMat == null)
            {
                cachedBodyMat = new Material(litShader) { name = "Mat_IndianTractor_PBR" };
                if (diffuseTex != null)
                {
                    cachedBodyMat.mainTexture = diffuseTex;
                    if (cachedBodyMat.HasProperty("_BaseMap")) cachedBodyMat.SetTexture("_BaseMap", diffuseTex);
                }
                if (normalTex != null && cachedBodyMat.HasProperty("_BumpMap"))
                {
                    cachedBodyMat.EnableKeyword("_NORMALMAP");
                    cachedBodyMat.SetTexture("_BumpMap", normalTex);
                }
                if (metallicTex != null && cachedBodyMat.HasProperty("_MetallicGlossMap"))
                {
                    cachedBodyMat.EnableKeyword("_METALLICSPECGLOSSMAP");
                    cachedBodyMat.SetTexture("_MetallicGlossMap", metallicTex);
                }
                if (cachedBodyMat.HasProperty("_Smoothness")) cachedBodyMat.SetFloat("_Smoothness", 0.55f);
                if (cachedBodyMat.HasProperty("_Metallic")) cachedBodyMat.SetFloat("_Metallic", 0.4f);
            }

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                r.receiveShadows = true;

                int matCount = r.sharedMaterials.Length > 0 ? r.sharedMaterials.Length : 1;
                Material[] newMats = new Material[matCount];
                for (int m = 0; m < matCount; m++)
                {
                    newMats[m] = cachedBodyMat;
                }
                r.sharedMaterials = newMats;
            }
        }
    }
}
