using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BhootiyaRasta.Environment
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class ImportedRoadBuilder : MonoBehaviour
    {
        [Header("Track Settings")]
        public float totalTrackLength = 1000f;
        public float roadWidth = 5.5f;
        public float terrainWidth = 80f;
        public int segmentCount = 200;
        public float curveFrequency = 0.008f;
        public float curveAmplitude = 18f;

        [Header("Materials")]
        public Material roadMaterial;
        public Material groundMaterial;

        [HideInInspector]
        public List<Vector3> roadPoints = new List<Vector3>();

        public List<Vector3> RoadPoints => roadPoints;

        public void BuildImportedRoadEnvironment()
        {
            // 1. Clean previous child objects (forest props, ground)
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            roadPoints.Clear();

            // 2. Setup PBR Materials
            SetupMaterials();

            // 3. Compute smooth continuous centerline points
            float step = totalTrackLength / segmentCount;
            for (int i = 0; i <= segmentCount; i++)
            {
                float z = i * step;
                float progress = (float)i / segmentCount;
                // Natural gentle rural Indian s-curves
                float x = Mathf.Sin(z * curveFrequency) * curveAmplitude +
                          Mathf.Sin(z * 0.022f) * (curveAmplitude * 0.4f);
                float y = 0.0f; // Smooth flat road elevation so no bumps or flipping

                roadPoints.Add(new Vector3(x, y, z));
            }

            // 4. Build Continuous Seamless Road Mesh & Collider
            BuildContinuousRoadMesh();

            // 5. Build Wide Continuous Terrain Ground & Collider (never fall into void)
            BuildContinuousTerrainMesh();

            // 6. Spawn Forest & Rural Environment Props
            SpawnEnvironmentDecorations();

            Debug.Log($"<color=#00FF00><b>[ImportedRoadBuilder]</b> Successfully generated continuous road track ({totalTrackLength}m) with full ground terrain colliders!</color>");
        }

        private void SetupMaterials()
        {
            if (roadMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");

                roadMaterial = new Material(shader);
                roadMaterial.name = "Mat_ContinuousGravelRoad";

                // Load newly imported high-resolution gravel road texture
                string texPath = "Assets/Models/Road/gravel_road_2_tex_0.jpg";
                if (!File.Exists(texPath)) texPath = "Assets/Models/Road/gravel_road_tex_0.jpg";
                Texture2D roadTex = null;
#if UNITY_EDITOR
                roadTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
#endif
                if (roadTex == null && File.Exists(texPath))
                {
                    byte[] bytes = File.ReadAllBytes(texPath);
                    roadTex = new Texture2D(2, 2, TextureFormat.RGB24, true);
                    roadTex.LoadImage(bytes);
                }

                if (roadTex != null)
                {
                    roadMaterial.mainTexture = roadTex;
                    if (roadMaterial.HasProperty("_BaseMap")) roadMaterial.SetTexture("_BaseMap", roadTex);
                }
                else
                {
                    roadMaterial.color = new Color(0.38f, 0.35f, 0.30f);
                }

                if (roadMaterial.HasProperty("_Smoothness")) roadMaterial.SetFloat("_Smoothness", 0.1f);
                if (roadMaterial.HasProperty("_Metallic")) roadMaterial.SetFloat("_Metallic", 0.0f);
            }

            if (groundMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");

                groundMaterial = new Material(shader);
                groundMaterial.name = "Mat_ContinuousFarmland";
                groundMaterial.color = new Color(0.14f, 0.12f, 0.09f); // Dark rural midnight soil

                string dirtTexPath = "Assets/Models/Road/ForestTextures/Ground_Dirt.png";
                Texture2D dirtTex = null;
#if UNITY_EDITOR
                dirtTex = AssetDatabase.LoadAssetAtPath<Texture2D>(dirtTexPath);
#endif
                if (dirtTex != null)
                {
                    groundMaterial.mainTexture = dirtTex;
                    if (groundMaterial.HasProperty("_BaseMap")) groundMaterial.SetTexture("_BaseMap", dirtTex);
                }

                if (groundMaterial.HasProperty("_Smoothness")) groundMaterial.SetFloat("_Smoothness", 0.05f);
            }
        }

        private void BuildContinuousRoadMesh()
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null) mf = gameObject.AddComponent<MeshFilter>();

            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();
            mr.sharedMaterial = roadMaterial;

            MeshCollider mc = GetComponent<MeshCollider>();
            if (mc == null) mc = gameObject.AddComponent<MeshCollider>();

            Mesh mesh = new Mesh();
            mesh.name = "ContinuousGravelRoadMesh";

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            // 5 cross-section points across road width:
            // 0: Left shoulder (-halfWidth)
            // 1: Left tire track (-halfWidth * 0.5)
            // 2: Center crown (0)
            // 3: Right tire track (+halfWidth * 0.5)
            // 4: Right shoulder (+halfWidth)
            float halfW = roadWidth * 0.5f;
            float[] crossX = new float[] { -halfW, -halfW * 0.5f, 0f, halfW * 0.5f, halfW };
            float[] crossY = new float[] { 0.01f, 0.03f, 0.05f, 0.03f, 0.01f }; // Slight convex crown
            float[] crossU = new float[] { 0f, 0.25f, 0.5f, 0.75f, 1f };

            int vertsPerRing = crossX.Length;

            for (int i = 0; i < roadPoints.Count; i++)
            {
                Vector3 pt = roadPoints[i];
                Vector3 forward;
                if (i < roadPoints.Count - 1)
                    forward = (roadPoints[i + 1] - pt).normalized;
                else
                    forward = (pt - roadPoints[i - 1]).normalized;

                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
                Vector3 up = Vector3.up;

                float v = ((float)i / roadPoints.Count) * (totalTrackLength / 6.0f); // Texture tiling

                for (int c = 0; c < vertsPerRing; c++)
                {
                    Vector3 vPos = pt + right * crossX[c] + up * crossY[c];
                    vertices.Add(vPos);
                    uvs.Add(new Vector2(crossU[c], v));
                    normals.Add(Vector3.up);
                }

                if (i < roadPoints.Count - 1)
                {
                    int baseIdx = i * vertsPerRing;
                    int nxtIdx = (i + 1) * vertsPerRing;

                    for (int s = 0; s < vertsPerRing - 1; s++)
                    {
                        triangles.Add(baseIdx + s);
                        triangles.Add(nxtIdx + s);
                        triangles.Add(baseIdx + s + 1);

                        triangles.Add(baseIdx + s + 1);
                        triangles.Add(nxtIdx + s);
                        triangles.Add(nxtIdx + s + 1);
                    }
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
            mc.sharedMesh = null;
            mc.sharedMesh = mesh;
        }

        private void BuildContinuousTerrainMesh()
        {
            GameObject terrainObj = new GameObject("ContinuousGroundTerrain");
            terrainObj.transform.SetParent(transform, false);

            MeshFilter tmf = terrainObj.AddComponent<MeshFilter>();
            MeshRenderer tmr = terrainObj.AddComponent<MeshRenderer>();
            tmr.sharedMaterial = groundMaterial;

            MeshCollider tmc = terrainObj.AddComponent<MeshCollider>();

            Mesh tMesh = new Mesh();
            tMesh.name = "ContinuousFarmlandMesh";

            List<Vector3> tVerts = new List<Vector3>();
            List<Vector2> tUvs = new List<Vector2>();
            List<Vector3> tNormals = new List<Vector3>();
            List<int> tTris = new List<int>();

            float halfTerrain = terrainWidth * 0.5f;
            float halfRoad = roadWidth * 0.5f;

            // 4 points per slice: FarLeft (-halfTerrain), LeftRoadEdge (-halfRoad), RightRoadEdge (+halfRoad), FarRight (+halfTerrain)
            float[] tCrossX = new float[] { -halfTerrain, -halfRoad, halfRoad, halfTerrain };
            float[] tCrossY = new float[] { -0.1f, 0.0f, 0.0f, -0.1f };

            int tVertsPerRing = tCrossX.Length;

            for (int i = 0; i < roadPoints.Count; i++)
            {
                Vector3 pt = roadPoints[i];
                Vector3 forward;
                if (i < roadPoints.Count - 1)
                    forward = (roadPoints[i + 1] - pt).normalized;
                else
                    forward = (pt - roadPoints[i - 1]).normalized;

                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
                float v = ((float)i / roadPoints.Count) * (totalTrackLength / 12.0f);

                for (int c = 0; c < tVertsPerRing; c++)
                {
                    Vector3 vPos = pt + right * tCrossX[c] + Vector3.up * tCrossY[c];
                    tVerts.Add(vPos);
                    tUvs.Add(new Vector2((float)c / (tVertsPerRing - 1) * 8f, v));
                    tNormals.Add(Vector3.up);
                }

                if (i < roadPoints.Count - 1)
                {
                    int baseIdx = i * tVertsPerRing;
                    int nxtIdx = (i + 1) * tVertsPerRing;

                    for (int s = 0; s < tVertsPerRing - 1; s++)
                    {
                        // Skip the middle segment directly under the road surface to prevent Z-fighting
                        if (s == 1) continue;

                        tTris.Add(baseIdx + s);
                        tTris.Add(nxtIdx + s);
                        tTris.Add(baseIdx + s + 1);

                        tTris.Add(baseIdx + s + 1);
                        tTris.Add(nxtIdx + s);
                        tTris.Add(nxtIdx + s + 1);
                    }
                }
            }

            tMesh.SetVertices(tVerts);
            tMesh.SetUVs(0, tUvs);
            tMesh.SetNormals(tNormals);
            tMesh.SetTriangles(tTris, 0);
            tMesh.RecalculateBounds();

            tmf.sharedMesh = tMesh;
            tmc.sharedMesh = null;
            tmc.sharedMesh = tMesh;
        }

        private void Awake()
        {
            if (roadPoints == null || roadPoints.Count == 0 || GetComponent<MeshFilter>()?.sharedMesh == null)
            {
                BuildImportedRoadEnvironment();
            }
        }

        private void SpawnEnvironmentDecorations()
        {
#if UNITY_EDITOR
            // Forest Pack Trees & Rocks along background
            GameObject treePack = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Background/low_poly_forest_tree_pack.glb");
            if (treePack != null)
            {
                for (int i = 3; i < roadPoints.Count - 3; i += 4)
                {
                    Vector3 pt = roadPoints[i];
                    Vector3 forward = (roadPoints[i + 1] - pt).normalized;
                    Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

                    // Left Trees (layered at different depths in background)
                    float distL1 = UnityEngine.Random.Range(5.5f, 14.0f);
                    float distL2 = UnityEngine.Random.Range(16.0f, 32.0f);

                    GameObject treeL1 = Instantiate(treePack, transform);
                    treeL1.name = $"ForestTree_L_{i}_near";
                    treeL1.transform.localPosition = pt - right * distL1;
                    treeL1.transform.localRotation = Quaternion.Euler(0f, (i * 59f) % 360f, 0f);
                    treeL1.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.8f, 1.3f);

                    GameObject treeL2 = Instantiate(treePack, transform);
                    treeL2.name = $"ForestTree_L_{i}_far";
                    treeL2.transform.localPosition = pt - right * distL2;
                    treeL2.transform.localRotation = Quaternion.Euler(0f, ((i + 19) * 83f) % 360f, 0f);
                    treeL2.transform.localScale = Vector3.one * UnityEngine.Random.Range(1.1f, 1.6f);

                    // Right Trees (layered at different depths in background)
                    float distR1 = UnityEngine.Random.Range(5.5f, 14.0f);
                    float distR2 = UnityEngine.Random.Range(16.0f, 32.0f);

                    GameObject treeR1 = Instantiate(treePack, transform);
                    treeR1.name = $"ForestTree_R_{i}_near";
                    treeR1.transform.localPosition = pt + right * distR1;
                    treeR1.transform.localRotation = Quaternion.Euler(0f, ((i + 17) * 73f) % 360f, 0f);
                    treeR1.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.8f, 1.3f);

                    GameObject treeR2 = Instantiate(treePack, transform);
                    treeR2.name = $"ForestTree_R_{i}_far";
                    treeR2.transform.localPosition = pt + right * distR2;
                    treeR2.transform.localRotation = Quaternion.Euler(0f, ((i + 31) * 97f) % 360f, 0f);
                    treeR2.transform.localScale = Vector3.one * UnityEngine.Random.Range(1.1f, 1.6f);
                }
            }
#endif
        }

        public Vector3 GetPointAtDistance(float distance)
        {
            if (roadPoints == null || roadPoints.Count == 0) return new Vector3(0, 0, distance);
            float step = totalTrackLength / Mathf.Max(1, roadPoints.Count - 1);
            int idx = Mathf.Clamp(Mathf.FloorToInt(distance / step), 0, roadPoints.Count - 1);
            return roadPoints[idx];
        }
    }
}
