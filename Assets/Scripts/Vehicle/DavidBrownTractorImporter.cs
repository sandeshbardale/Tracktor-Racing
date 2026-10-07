#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BhootiyaRasta.Vehicle
{
    public static class DavidBrownTractorImporter
    {
        private static Mesh cachedMesh0 = null;
        private static Mesh cachedMesh1 = null;
        private static Material cachedMaterial = null;

        private static string ResolvePath(string relativePath)
        {
            if (File.Exists(relativePath)) return relativePath;
            if (relativePath.StartsWith("Assets/"))
            {
                string sub = relativePath.Substring("Assets/".Length);
                string fromData = Path.Combine(Application.dataPath, sub);
                if (File.Exists(fromData)) return fromData;
            }
            string fromCwd = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            if (File.Exists(fromCwd)) return fromCwd;
            return relativePath;
        }

        public static Material GetOrCreateTractorMaterial()
        {
            if (cachedMaterial != null) return cachedMaterial;

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            cachedMaterial = new Material(litShader);
            cachedMaterial.name = "Mat_DavidBrown25D";

            // Load extracted textures
            string diffusePath = ResolvePath("Assets/Models/Tractor/tractor_texture_0.jpg");
            string normalPath = ResolvePath("Assets/Models/Tractor/tractor_texture_2.png");
            string metalRoughPath = ResolvePath("Assets/Models/Tractor/tractor_texture_1.png");

            Texture2D diffTex = null;
#if UNITY_EDITOR
            diffTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Models/Tractor/tractor_texture_0.jpg");
#endif
            if (diffTex == null && File.Exists(diffusePath))
            {
                byte[] diffBytes = File.ReadAllBytes(diffusePath);
                diffTex = new Texture2D(2, 2, TextureFormat.RGB24, true);
                diffTex.LoadImage(diffBytes);
                diffTex.name = "Tex_DavidBrown_Diffuse";
            }
            if (diffTex != null)
            {
                cachedMaterial.mainTexture = diffTex;
                if (cachedMaterial.HasProperty("_BaseMap")) cachedMaterial.SetTexture("_BaseMap", diffTex);
            }

            Texture2D normTex = null;
#if UNITY_EDITOR
            normTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Models/Tractor/tractor_texture_2.png");
#endif
            if (normTex == null && File.Exists(normalPath))
            {
                byte[] normBytes = File.ReadAllBytes(normalPath);
                normTex = new Texture2D(2, 2, TextureFormat.RGBA32, true);
                normTex.LoadImage(normBytes);
                normTex.name = "Tex_DavidBrown_Normal";
            }
            if (normTex != null && cachedMaterial.HasProperty("_BumpMap"))
            {
                cachedMaterial.EnableKeyword("_NORMALMAP");
                cachedMaterial.SetTexture("_BumpMap", normTex);
            }

            Texture2D metalTex = null;
#if UNITY_EDITOR
            metalTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Models/Tractor/tractor_texture_1.png");
#endif
            if (metalTex == null && File.Exists(metalRoughPath))
            {
                byte[] metalBytes = File.ReadAllBytes(metalRoughPath);
                metalTex = new Texture2D(2, 2, TextureFormat.RGBA32, true);
                metalTex.LoadImage(metalBytes);
                metalTex.name = "Tex_DavidBrown_MetallicRoughness";
            }
            if (metalTex != null && cachedMaterial.HasProperty("_MetallicGlossMap"))
            {
                cachedMaterial.EnableKeyword("_METALLICSPECGLOSSMAP");
                cachedMaterial.SetTexture("_MetallicGlossMap", metalTex);
            }

            if (cachedMaterial.HasProperty("_Metallic")) cachedMaterial.SetFloat("_Metallic", 0.4f);
            if (cachedMaterial.HasProperty("_Smoothness")) cachedMaterial.SetFloat("_Smoothness", 0.45f);

            return cachedMaterial;
        }

        public static (Mesh, Mesh) LoadTractorMeshes(bool forceReload = false)
        {
            if (!forceReload && cachedMesh0 != null && cachedMesh1 != null)
            {
                return (cachedMesh0, cachedMesh1);
            }

            cachedMesh0 = null;
            cachedMesh1 = null;

            string objPath = ResolvePath("Assets/Models/Tractor/david_brown_25d_tractor.obj");
            if (!File.Exists(objPath))
            {
                Debug.LogError("[DavidBrownImporter] OBJ file not found at: " + objPath);
                return (null, null);
            }

            List<Vector3> allPositions = new List<Vector3>();
            List<Vector2> allUvs = new List<Vector2>();
            List<Vector3> allNormals = new List<Vector3>();

            List<Vector3> m0_verts = new List<Vector3>();
            List<Vector2> m0_uvs = new List<Vector2>();
            List<Vector3> m0_normals = new List<Vector3>();
            List<int> m0_tris = new List<int>();
            Dictionary<string, int> m0_vertMap = new Dictionary<string, int>();

            List<Vector3> m1_verts = new List<Vector3>();
            List<Vector2> m1_uvs = new List<Vector2>();
            List<Vector3> m1_normals = new List<Vector3>();
            List<int> m1_tris = new List<int>();
            Dictionary<string, int> m1_vertMap = new Dictionary<string, int>();

            int currentMesh = 0; // 0 for Object_0, 1 for Object_1

            using (var reader = new StreamReader(objPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length < 2) continue;

                    if (line.StartsWith("o ") || line.StartsWith("g "))
                    {
                        if (line.Contains("1")) currentMesh = 1;
                        else if (line.Contains("0")) currentMesh = 0;
                        continue;
                    }

                    if (line[0] == 'v' && line[1] == ' ')
                    {
                        var parts = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                        float x = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        float y = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                        float z = float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                        allPositions.Add(new Vector3(x, y, z));
                    }
                    else if (line.StartsWith("vt "))
                    {
                        var parts = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                        float u = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        float v = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                        allUvs.Add(new Vector2(u, v));
                    }
                    else if (line.StartsWith("vn "))
                    {
                        var parts = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                        float x = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        float y = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                        float z = float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                        allNormals.Add(new Vector3(x, y, z));
                    }
                    else if (line[0] == 'f' && line[1] == ' ')
                    {
                        var parts = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                        int pCount = parts.Length - 1;
                        if (pCount < 3) continue;

                        int[] faceIndices = new int[pCount];
                        var targetVerts = (currentMesh == 0) ? m0_verts : m1_verts;
                        var targetUvs = (currentMesh == 0) ? m0_uvs : m1_uvs;
                        var targetNorms = (currentMesh == 0) ? m0_normals : m1_normals;
                        var targetTris = (currentMesh == 0) ? m0_tris : m1_tris;
                        var targetMap = (currentMesh == 0) ? m0_vertMap : m1_vertMap;

                        for (int i = 0; i < pCount; i++)
                        {
                            string token = parts[i + 1];
                            if (targetMap.TryGetValue(token, out int existingIdx))
                            {
                                faceIndices[i] = existingIdx;
                            }
                            else
                            {
                                var subTokens = token.Split('/');
                                int vIdx = int.Parse(subTokens[0]) - 1;
                                int vtIdx = (subTokens.Length > 1 && subTokens[1].Length > 0) ? (int.Parse(subTokens[1]) - 1) : -1;
                                int vnIdx = (subTokens.Length > 2 && subTokens[2].Length > 0) ? (int.Parse(subTokens[2]) - 1) : -1;

                                int newIdx = targetVerts.Count;
                                targetVerts.Add(allPositions[vIdx]);
                                targetUvs.Add(vtIdx >= 0 && vtIdx < allUvs.Count ? allUvs[vtIdx] : Vector2.zero);
                                targetNorms.Add(vnIdx >= 0 && vnIdx < allNormals.Count ? allNormals[vnIdx] : Vector3.up);

                                targetMap[token] = newIdx;
                                faceIndices[i] = newIdx;
                            }
                        }

                        // Triangulate
                        for (int i = 1; i < pCount - 1; i++)
                        {
                            targetTris.Add(faceIndices[0]);
                            targetTris.Add(faceIndices[i]);
                            targetTris.Add(faceIndices[i + 1]);
                        }
                    }
                }
            }

            // Create Mesh 0
            cachedMesh0 = new Mesh();
            cachedMesh0.name = "DavidBrown_Mesh0";
            cachedMesh0.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            cachedMesh0.SetVertices(m0_verts);
            cachedMesh0.SetUVs(0, m0_uvs);
            cachedMesh0.SetNormals(m0_normals);
            cachedMesh0.SetTriangles(m0_tris, 0);
            cachedMesh0.RecalculateBounds();

            // Create Mesh 1
            cachedMesh1 = new Mesh();
            cachedMesh1.name = "DavidBrown_Mesh1";
            cachedMesh1.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            cachedMesh1.SetVertices(m1_verts);
            cachedMesh1.SetUVs(0, m1_uvs);
            cachedMesh1.SetNormals(m1_normals);
            cachedMesh1.SetTriangles(m1_tris, 0);
            cachedMesh1.RecalculateBounds();

            Debug.Log($"[DavidBrownImporter] Loaded meshes successfully! Mesh0 verts: {cachedMesh0.vertexCount}, Bounds: {cachedMesh0.bounds} | Mesh1 verts: {cachedMesh1.vertexCount}, Bounds: {cachedMesh1.bounds}");

            return (cachedMesh0, cachedMesh1);
        }

        public static GameObject BuildDavidBrownTractor()
        {
            var (mesh0, mesh1) = LoadTractorMeshes(true);
            Material tractorMat = GetOrCreateTractorMaterial();

            // 1. Root GameObject
            GameObject root = new GameObject("IndianTractor_Bhootni");
            root.tag = "Player";

            // 2. Physics / Rigidbody setup
            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = 2400f; // Heavy agricultural tractor mass
            rb.centerOfMass = new Vector3(0f, 0.45f, -0.1f); // Low center of gravity
            rb.linearDamping = 0.4f;
            rb.angularDamping = 3.0f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // PhysicMaterial for wheels so it rolls effortlessly without catching
            PhysicsMaterial wheelPhysMat = new PhysicsMaterial("WheelPhysMat")
            {
                dynamicFriction = 0.08f,
                staticFriction = 0.15f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounciness = 0.0f
            };

            // Main chassis collider (high clearance so it never drags)
            BoxCollider mainCol = root.AddComponent<BoxCollider>();
            mainCol.center = new Vector3(0f, 1.25f, -0.1f);
            mainCol.size = new Vector3(1.2f, 1.1f, 2.6f);

            // Left rear wheel (large wheel, radius 0.68m, rests at Y=0)
            SphereCollider rearLCol = root.AddComponent<SphereCollider>();
            rearLCol.center = new Vector3(-0.65f, 0.68f, -0.9f);
            rearLCol.radius = 0.68f;
            rearLCol.sharedMaterial = wheelPhysMat;

            // Right rear wheel (large wheel, radius 0.68m, rests at Y=0)
            SphereCollider rearRCol = root.AddComponent<SphereCollider>();
            rearRCol.center = new Vector3(0.65f, 0.68f, -0.9f);
            rearRCol.radius = 0.68f;
            rearRCol.sharedMaterial = wheelPhysMat;

            // Left front wheel (small wheel, radius 0.45m, rests at Y=0)
            SphereCollider frontLCol = root.AddComponent<SphereCollider>();
            frontLCol.center = new Vector3(-0.55f, 0.45f, 1.05f);
            frontLCol.radius = 0.45f;
            frontLCol.sharedMaterial = wheelPhysMat;

            // Right front wheel (small wheel, radius 0.45m, rests at Y=0)
            SphereCollider frontRCol = root.AddComponent<SphereCollider>();
            frontRCol.center = new Vector3(0.55f, 0.45f, 1.05f);
            frontRCol.radius = 0.45f;
            frontRCol.sharedMaterial = wheelPhysMat;

            // 3. VisualModel container
            GameObject visualModel = new GameObject("VisualModel");
            visualModel.transform.SetParent(root.transform, false);

            GameObject dbTractor = new GameObject("DavidBrownTractor");
            dbTractor.transform.SetParent(visualModel.transform, false);

            // Submesh 0
            if (mesh0 != null)
            {
                GameObject part0 = new GameObject("Part_0");
                part0.transform.SetParent(dbTractor.transform, false);
                var mf0 = part0.AddComponent<MeshFilter>();
                mf0.sharedMesh = mesh0;
                var mr0 = part0.AddComponent<MeshRenderer>();
                mr0.sharedMaterial = tractorMat;
            }

            // Submesh 1
            if (mesh1 != null)
            {
                GameObject part1 = new GameObject("Part_1");
                part1.transform.SetParent(dbTractor.transform, false);
                var mf1 = part1.AddComponent<MeshFilter>();
                mf1.sharedMesh = mesh1;
                var mr1 = part1.AddComponent<MeshRenderer>();
                mr1.sharedMaterial = tractorMat;
            }

            // 4. Wheels Transforms (for steering and rotation animation)
            GameObject wheelsRoot = new GameObject("Wheels");
            wheelsRoot.transform.SetParent(root.transform, false);

            GameObject fl = new GameObject("FrontLeft");
            fl.transform.SetParent(wheelsRoot.transform, false);
            fl.transform.localPosition = new Vector3(-0.55f, 0.45f, 1.05f);

            GameObject fr = new GameObject("FrontRight");
            fr.transform.SetParent(wheelsRoot.transform, false);
            fr.transform.localPosition = new Vector3(0.55f, 0.45f, 1.05f);

            GameObject rl = new GameObject("RearLeft");
            rl.transform.SetParent(wheelsRoot.transform, false);
            rl.transform.localPosition = new Vector3(-0.65f, 0.68f, -0.9f);

            GameObject rr = new GameObject("RearRight");
            rr.transform.SetParent(wheelsRoot.transform, false);
            rr.transform.localPosition = new Vector3(0.65f, 0.68f, -0.9f);

            // 5. Lights
            GameObject lightsRoot = new GameObject("Lights");
            lightsRoot.transform.SetParent(root.transform, false);

            Light hlLeft = CreateHeadlight("HeadlightLeft", lightsRoot.transform, new Vector3(-0.42f, 1.15f, 1.35f));
            Light hlRight = CreateHeadlight("HeadlightRight", lightsRoot.transform, new Vector3(0.42f, 1.15f, 1.35f));
            Light tlLeft = CreateTaillight("RearLightLeft", lightsRoot.transform, new Vector3(-0.75f, 1.15f, -1.65f));
            Light tlRight = CreateTaillight("RearLightRight", lightsRoot.transform, new Vector3(0.75f, 1.15f, -1.65f));

            // 6. CameraTarget (positioned at tractor center, height 1.4m)
            GameObject camTarget = new GameObject("CameraTarget");
            camTarget.transform.SetParent(root.transform, false);
            camTarget.transform.localPosition = new Vector3(0f, 1.4f, 0f);

            // 7. Mud/Dust Splatter Particle System
            ParticleSystem mudParticles = CreateTractorMudParticles(root.transform);

            // 8. Attach TractorController & TractorAudio
            TractorController controller = root.AddComponent<TractorController>();
            TractorAudio tractorAudio = root.AddComponent<TractorAudio>();

            // Configure controller references
            controller.tractorBody = visualModel.transform;
            controller.frontLeftWheel = fl.transform;
            controller.frontRightWheel = fr.transform;
            controller.rearLeftWheel = rl.transform;
            controller.rearRightWheel = rr.transform;
            controller.mudSplatterParticles = mudParticles;

            // Link lights via reflection
            var fieldH = typeof(TractorController).GetField("headLights", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldH != null) fieldH.SetValue(controller, new Light[] { hlLeft, hlRight });

            var fieldT = typeof(TractorController).GetField("tailLights", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldT != null) fieldT.SetValue(controller, new Light[] { tlLeft, tlRight });

            return root;
        }

        private static Light CreateHeadlight(string name, Transform parent, Vector3 localPos)
        {
            GameObject hlObj = new GameObject(name);
            hlObj.transform.SetParent(parent, false);
            hlObj.transform.localPosition = localPos;

            Light lt = hlObj.AddComponent<Light>();
            lt.type = LightType.Spot;
            lt.range = 55f;
            lt.spotAngle = 65f;
            lt.innerSpotAngle = 40f;
            lt.intensity = 4.2f;
            lt.color = new Color(1.0f, 0.93f, 0.78f); // Warm Halogen tint
            lt.shadows = LightShadows.Hard;

            return lt;
        }

        private static Light CreateTaillight(string name, Transform parent, Vector3 localPos)
        {
            GameObject tlObj = new GameObject(name);
            tlObj.transform.SetParent(parent, false);
            tlObj.transform.localPosition = localPos;

            Light lt = tlObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.range = 5.0f;
            lt.intensity = 1.5f;
            lt.color = new Color(0.95f, 0.05f, 0.05f);

            return lt;
        }

        private static ParticleSystem CreateTractorMudParticles(Transform parent)
        {
            GameObject psObj = new GameObject("MudSplatterParticles");
            psObj.transform.SetParent(parent, false);
            psObj.transform.localPosition = new Vector3(0f, 0.2f, -1.0f);

            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 2.5f;
            main.startSize = 0.12f;
            main.startColor = new Color(0.24f, 0.16f, 0.10f, 0.8f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 25f;
            emission.enabled = false;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(1.4f, 0.1f, 0.4f);

            return ps;
        }
    }
}
