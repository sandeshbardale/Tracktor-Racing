using System.Collections.Generic;
using UnityEngine;

namespace BhootiyaRasta.Environment
{
    public class ProceduralRoad : MonoBehaviour
    {
        [Header("Road Parameters")]
        public float roadLength = 650f;
        public float roadWidth = 4.8f;
        public int segmentCount = 140;
        public float curveFrequency = 0.012f;
        public float curveAmplitude = 16f;

        [Header("Materials")]
        public Material roadMaterial;
        public Material groundFieldMaterial;
        public Material mudPuddleMat;
        public Material stoneMat;
        public Material grassTuftMat;

        [HideInInspector]
        public List<Vector3> roadPoints = new List<Vector3>();

        public void GenerateRoad(float length, float curvature, int seed)
        {
            roadLength = length;
            curveAmplitude = curvature;
            Random.InitState(seed);

            // Clean up children (rocks, grass, puddles)
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            roadPoints.Clear();
            float step = roadLength / segmentCount;

            // Compute undulating centerline with smooth S-curves and rural elevation dips
            for (int i = 0; i <= segmentCount; i++)
            {
                float z = i * step;
                float x = Mathf.Sin(z * curveFrequency + seed) * curveAmplitude +
                          Mathf.Sin(z * 0.032f + seed * 3f) * (curveAmplitude * 0.35f);
                // Elevation undulation: gentle rolls, dips, and small ruts
                float y = Mathf.PerlinNoise(seed * 0.2f, z * 0.008f) * 1.8f +
                          Mathf.Sin(z * 0.04f + seed) * 0.25f;

                roadPoints.Add(new Vector3(x, y, z));
            }

            BuildSculptedRoadMesh();
            BuildFarmlandTerrain();
            ScatterRoadsideDetails(seed);
        }

        private void BuildSculptedRoadMesh()
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null) mf = gameObject.AddComponent<MeshFilter>();

            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();

            MeshCollider mc = GetComponent<MeshCollider>();
            if (mc == null) mc = gameObject.AddComponent<MeshCollider>();

            Mesh mesh = new Mesh();
            mesh.name = "SculptedKacchaRoad";

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            // 7 cross-section vertices:
            // 0: Far Left Ditch Edge (-2.6m)
            // 1: Left Shoulder (-2.0m)
            // 2: Left Tire Rut (-1.0m, deep rut -0.12m)
            // 3: Crown Center Berm (0.0m, raised +0.06m)
            // 4: Right Tire Rut (+1.0m, deep rut -0.12m)
            // 5: Right Shoulder (+2.0m)
            // 6: Far Right Ditch Edge (+2.6m)
            float[] crossX = new float[] { -2.6f, -1.9f, -1.0f, 0.0f, 1.0f, 1.9f, 2.6f };
            float[] crossY = new float[] { -0.18f, 0.0f, -0.12f, 0.06f, -0.12f, 0.0f, -0.18f };
            float[] crossU = new float[] { 0.0f, 0.16f, 0.33f, 0.5f, 0.67f, 0.84f, 1.0f };

            int vertsPerRing = crossX.Length;

            for (int i = 0; i < roadPoints.Count; i++)
            {
                Vector3 pt = roadPoints[i];
                Vector3 forward = (i < roadPoints.Count - 1) ? (roadPoints[i + 1] - pt).normalized : (pt - roadPoints[i - 1]).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
                Vector3 up = Vector3.Cross(forward, right).normalized;

                float v = (float)i / roadPoints.Count * (roadLength / 8f);

                for (int c = 0; c < vertsPerRing; c++)
                {
                    // Add subtle Perlin bumpiness along the soil road
                    float bump = (Mathf.PerlinNoise(pt.x * 0.5f + c, pt.z * 0.5f) - 0.5f) * 0.05f;
                    Vector3 vertPos = pt + right * crossX[c] + up * (crossY[c] + bump);
                    vertices.Add(vertPos);
                    uvs.Add(new Vector2(crossU[c], v));
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
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;

            if (roadMaterial != null) mr.sharedMaterial = roadMaterial;
        }

        private void BuildFarmlandTerrain()
        {
            Transform existing = transform.Find("AgriculturalFarmland");
            if (existing != null) DestroyImmediate(existing.gameObject);

            GameObject fieldsObj = new GameObject("AgriculturalFarmland");
            fieldsObj.transform.SetParent(transform, false);

            MeshFilter mf = fieldsObj.AddComponent<MeshFilter>();
            MeshRenderer mr = fieldsObj.AddComponent<MeshRenderer>();
            MeshCollider mc = fieldsObj.AddComponent<MeshCollider>();

            Mesh mesh = new Mesh();
            mesh.name = "FarmlandFieldsMesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float fieldWidth = 85f;

            for (int i = 0; i < roadPoints.Count; i++)
            {
                Vector3 pt = roadPoints[i];
                Vector3 forward = (i < roadPoints.Count - 1) ? (roadPoints[i + 1] - pt).normalized : (pt - roadPoints[i - 1]).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

                // Far left boundary (distant hills slope)
                float leftRoll = Mathf.PerlinNoise(pt.z * 0.01f, 10f) * 4.5f - 0.5f;
                float rightRoll = Mathf.PerlinNoise(pt.z * 0.01f, 20f) * 4.5f - 0.5f;

                Vector3 farLeft = pt - right * fieldWidth + Vector3.up * leftRoll;
                Vector3 nearLeft = pt - right * 2.55f - Vector3.up * 0.18f;
                Vector3 nearRight = pt + right * 2.55f - Vector3.up * 0.18f;
                Vector3 farRight = pt + right * fieldWidth + Vector3.up * rightRoll;

                int baseIdx = verts.Count;
                verts.Add(farLeft);
                verts.Add(nearLeft);
                verts.Add(nearRight);
                verts.Add(farRight);

                float v = (float)i / roadPoints.Count * (roadLength / 18f);
                uvs.Add(new Vector2(0f, v));
                uvs.Add(new Vector2(0.48f, v));
                uvs.Add(new Vector2(0.52f, v));
                uvs.Add(new Vector2(1f, v));

                if (i < roadPoints.Count - 1)
                {
                    int nxt = baseIdx + 4;
                    // Left field strip
                    tris.Add(baseIdx); tris.Add(nxt); tris.Add(baseIdx + 1);
                    tris.Add(baseIdx + 1); tris.Add(nxt); tris.Add(nxt + 1);

                    // Right field strip
                    tris.Add(baseIdx + 2); tris.Add(nxt + 2); tris.Add(baseIdx + 3);
                    tris.Add(baseIdx + 3); tris.Add(nxt + 2); tris.Add(nxt + 3);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;

            if (groundFieldMaterial != null) mr.sharedMaterial = groundFieldMaterial;
        }

        private void ScatterRoadsideDetails(int seed)
        {
            Random.InitState(seed + 99);
            GameObject detailsRoot = new GameObject("RoadsideDetails");
            detailsRoot.transform.SetParent(transform, false);

            int count = roadPoints.Count;

            for (int i = 2; i < count - 2; i += 2)
            {
                Vector3 pt = roadPoints[i];
                Vector3 fwd = (roadPoints[i + 1] - pt).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

                // 1. Scattered 3D stones & rocks on roadside shoulders
                if (Random.value < 0.65f)
                {
                    float side = (Random.value > 0.5f) ? 1f : -1f;
                    float dist = Random.Range(1.8f, 2.7f);
                    Vector3 rockPos = pt + right * (side * dist) + fwd * Random.Range(-0.8f, 0.8f);
                    CreateRoadsideStone(detailsRoot.transform, rockPos, Random.Range(0.12f, 0.32f));
                }

                // 2. Roadside Grass Tufts along the muddy edge
                for (int side = -1; side <= 1; side += 2)
                {
                    if (Random.value < 0.85f)
                    {
                        Vector3 grassPos = pt + right * (side * Random.Range(2.1f, 3.2f)) + fwd * Random.Range(-0.5f, 0.5f);
                        CreateGrassTuft(detailsRoot.transform, grassPos);
                    }
                }

                // 3. Reflective Mud Puddle in the ruts
                if (i % 12 == 0 && Random.value < 0.75f)
                {
                    float rutX = (Random.value > 0.5f) ? -1.0f : 1.0f;
                    Vector3 puddlePos = pt + right * rutX + Vector3.up * -0.06f;
                    CreateMudPuddleInstance(detailsRoot.transform, puddlePos, fwd);
                }
            }
        }

        private void CreateRoadsideStone(Transform parent, Vector3 pos, float scale)
        {
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stone.name = "EarthenRock";
            stone.transform.SetParent(parent, true);
            stone.transform.position = pos + Vector3.up * (scale * 0.25f);
            stone.transform.localScale = new Vector3(scale * Random.Range(1f, 1.4f), scale * 0.65f, scale * Random.Range(0.9f, 1.5f));
            stone.transform.localRotation = Random.rotation;
            if (stoneMat != null) stone.GetComponent<Renderer>().sharedMaterial = stoneMat;
        }

        private void CreateGrassTuft(Transform parent, Vector3 pos)
        {
            GameObject tuft = new GameObject("GrassTuft");
            tuft.transform.SetParent(parent, true);
            tuft.transform.position = pos;

            // Two crossed planar quads for 3D foliage
            for (int i = 0; i < 2; i++)
            {
                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Quad);
                blade.transform.SetParent(tuft.transform, false);
                blade.transform.localPosition = new Vector3(0f, 0.28f, 0f);
                blade.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
                blade.transform.localRotation = Quaternion.Euler(0f, i * 90f + Random.Range(-25f, 25f), 0f);
                if (grassTuftMat != null) blade.GetComponent<Renderer>().sharedMaterial = grassTuftMat;
                DestroyImmediate(blade.GetComponent<Collider>());
            }
        }

        private void CreateMudPuddleInstance(Transform parent, Vector3 pos, Vector3 forward)
        {
            GameObject puddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            puddle.name = "MuddyWaterPuddle";
            puddle.transform.SetParent(parent, true);
            puddle.transform.position = pos;
            puddle.transform.rotation = Quaternion.LookRotation(forward);
            puddle.transform.localScale = new Vector3(1.2f, 0.015f, Random.Range(2.8f, 5.2f));

            if (mudPuddleMat != null) puddle.GetComponent<Renderer>().sharedMaterial = mudPuddleMat;

            var col = puddle.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            puddle.AddComponent<MudPuddle>();
        }

        public Vector3 GetPointAtDistance(float distance)
        {
            if (roadPoints == null || roadPoints.Count == 0) return Vector3.zero;
            float step = roadLength / segmentCount;
            int idx = Mathf.Clamp(Mathf.FloorToInt(distance / step), 0, roadPoints.Count - 1);
            return roadPoints[idx];
        }

        public Vector3 GetDestinationPoint()
        {
            if (roadPoints != null && roadPoints.Count > 0)
            {
                return roadPoints[roadPoints.Count - 1];
            }
            return Vector3.forward * roadLength;
        }
    }
}
