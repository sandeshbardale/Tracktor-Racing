using System.Collections.Generic;
using UnityEngine;

namespace BhootiyaRasta.Horror
{
    public static class RealisticGhostModelGenerator
    {
        public static GameObject CreateRealisticGhost(
            Material skinMat,
            Material clothMat,
            Material hairMat,
            Material eyeGlowMat)
        {
            GameObject ghostRoot = new GameObject("IndianGhost_Chudail");
            var gc = ghostRoot.AddComponent<GhostController>();

            GameObject bodyContainer = new GameObject("GhostBodyRoot");
            bodyContainer.transform.SetParent(ghostRoot.transform, false);
            gc.ghostBody = bodyContainer.transform;

            // 1. Lower Body & Saree Drapery (Lofted flowing cloth mesh with torn edges)
            GameObject dress = CreateFlowingSareeMesh(clothMat);
            dress.transform.SetParent(bodyContainer.transform, false);
            dress.transform.localPosition = new Vector3(0f, 0.05f, 0f);

            // 2. Torso with unnatural contorted slouch
            GameObject torso = CreateTorsoMesh(clothMat, skinMat);
            torso.transform.SetParent(bodyContainer.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.05f, 0f);

            // 3. Unnerving Outstretched / Dangling Arms
            GameObject armL = CreateLimb(skinMat, clothMat, true);
            armL.transform.SetParent(torso.transform, false);
            armL.transform.localPosition = new Vector3(-0.28f, 0.32f, 0.05f);

            GameObject armR = CreateLimb(skinMat, clothMat, false);
            armR.transform.SetParent(torso.transform, false);
            armR.transform.localPosition = new Vector3(0.28f, 0.32f, 0.05f);

            // 4. Head with long streaming disheveled hair concealing face
            GameObject head = CreateGhostHead(skinMat, hairMat, eyeGlowMat, out Light eyeL, out Light eyeR);
            head.transform.SetParent(torso.transform, false);
            head.transform.localPosition = new Vector3(0f, 0.52f, 0.12f);
            gc.headTransform = head.transform;
            gc.eyeLightLeft = eyeL;
            gc.eyeLightRight = eyeR;

            // Collect all renderers for fade-in/fade-out
            gc.ghostRenderers = ghostRoot.GetComponentsInChildren<Renderer>();

            return ghostRoot;
        }

        private static GameObject CreateFlowingSareeMesh(Material clothMat)
        {
            GameObject saree = new GameObject("FlowingSaree");
            MeshFilter mf = saree.AddComponent<MeshFilter>();
            MeshRenderer mr = saree.AddComponent<MeshRenderer>();
            mr.sharedMaterial = clothMat;

            Mesh mesh = new Mesh();
            mesh.name = "SareeMesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int heightSegments = 8;
            int radialSegments = 16;
            float totalHeight = 1.15f;

            for (int h = 0; h <= heightSegments; h++)
            {
                float tH = (float)h / heightSegments;
                float y = Mathf.Lerp(0f, totalHeight, tH);

                // Saree expands outwards toward the bottom with wavy folds
                float baseRadius = Mathf.Lerp(0.55f, 0.24f, tH);

                for (int r = 0; r <= radialSegments; r++)
                {
                    float angle = (float)r / radialSegments * Mathf.PI * 2f;
                    // Natural cloth pleats / wrinkles
                    float pleat = Mathf.Sin(angle * 6f) * 0.045f * (1f - tH);
                    float rad = baseRadius + pleat;

                    // Slight back-to-front slant
                    float zOffset = Mathf.Lerp(0.12f, 0f, tH);

                    float x = Mathf.Cos(angle) * rad;
                    float z = Mathf.Sin(angle) * rad + zOffset;

                    verts.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2((float)r / radialSegments, tH));
                }
            }

            for (int h = 0; h < heightSegments; h++)
            {
                for (int r = 0; r < radialSegments; r++)
                {
                    int i0 = h * (radialSegments + 1) + r;
                    int i1 = i0 + 1;
                    int i2 = (h + 1) * (radialSegments + 1) + r;
                    int i3 = i2 + 1;

                    tris.Add(i0); tris.Add(i2); tris.Add(i1);
                    tris.Add(i1); tris.Add(i2); tris.Add(i3);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mf.sharedMesh = mesh;

            return saree;
        }

        private static GameObject CreateTorsoMesh(Material clothMat, Material skinMat)
        {
            GameObject torso = new GameObject("GhostTorso");

            // Upper Chest / Pallu Cloth Drape
            GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chest.name = "ChestDrape";
            chest.transform.SetParent(torso.transform, false);
            chest.transform.localPosition = new Vector3(0f, 0.2f, 0.04f);
            chest.transform.localScale = new Vector3(0.38f, 0.22f, 0.26f);
            chest.transform.localRotation = Quaternion.Euler(15f, 0f, 0f); // Unnatural forward slump
            chest.GetComponent<Renderer>().sharedMaterial = clothMat;
            Object.DestroyImmediate(chest.GetComponent<Collider>());

            // Neck
            GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neck.name = "Neck";
            neck.transform.SetParent(torso.transform, false);
            neck.transform.localPosition = new Vector3(0f, 0.42f, 0.08f);
            neck.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            neck.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            neck.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(neck.GetComponent<Collider>());

            return torso;
        }

        private static GameObject CreateLimb(Material skinMat, Material clothMat, bool isLeft)
        {
            float side = isLeft ? -1f : 1f;
            GameObject limb = new GameObject(isLeft ? "Arm_L" : "Arm_R");

            // Upper arm cloth sleeve
            GameObject upper = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            upper.transform.SetParent(limb.transform, false);
            upper.transform.localPosition = new Vector3(side * 0.08f, -0.18f, 0.04f);
            upper.transform.localScale = new Vector3(0.11f, 0.2f, 0.11f);
            upper.transform.localRotation = Quaternion.Euler(22f, 0f, side * 15f);
            upper.GetComponent<Renderer>().sharedMaterial = clothMat;
            Object.DestroyImmediate(upper.GetComponent<Collider>());

            // Gaunt Forearm & Dangling Pale Hands
            GameObject lower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lower.transform.SetParent(limb.transform, false);
            lower.transform.localPosition = new Vector3(side * 0.15f, -0.52f, 0.14f);
            lower.transform.localScale = new Vector3(0.07f, 0.22f, 0.07f);
            lower.transform.localRotation = Quaternion.Euler(38f, 0f, side * 10f);
            lower.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(lower.GetComponent<Collider>());

            // Pale emaciated Hand
            GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hand.transform.SetParent(lower.transform, false);
            hand.transform.localPosition = new Vector3(0f, -1.02f, 0f);
            hand.transform.localScale = new Vector3(0.9f, 0.35f, 1.4f);
            hand.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(hand.GetComponent<Collider>());

            return limb;
        }

        private static GameObject CreateGhostHead(Material skinMat, Material hairMat, Material eyeGlowMat, out Light eyeL, out Light eyeR)
        {
            GameObject head = new GameObject("GhostHead");

            // Pale Cranium
            GameObject cranium = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cranium.name = "PaleFace";
            cranium.transform.SetParent(head.transform, false);
            cranium.transform.localScale = new Vector3(0.32f, 0.38f, 0.34f);
            cranium.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(cranium.GetComponent<Collider>());

            // Long flowing disheveled hair covering front and sides
            // Front hair veil (streaming down over face)
            GameObject hairFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hairFront.name = "FrontHairVeil";
            hairFront.transform.SetParent(head.transform, false);
            hairFront.transform.localPosition = new Vector3(0f, -0.25f, 0.16f);
            hairFront.transform.localScale = new Vector3(0.38f, 0.72f, 0.08f);
            hairFront.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
            hairFront.GetComponent<Renderer>().sharedMaterial = hairMat;
            Object.DestroyImmediate(hairFront.GetComponent<Collider>());

            // Left & Right hair curtains
            GameObject hairL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hairL.transform.SetParent(head.transform, false);
            hairL.transform.localPosition = new Vector3(-0.16f, -0.32f, 0.04f);
            hairL.transform.localScale = new Vector3(0.09f, 0.85f, 0.28f);
            hairL.GetComponent<Renderer>().sharedMaterial = hairMat;
            Object.DestroyImmediate(hairL.GetComponent<Collider>());

            GameObject hairR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hairR.transform.SetParent(head.transform, false);
            hairR.transform.localPosition = new Vector3(0.16f, -0.32f, 0.04f);
            hairR.transform.localScale = new Vector3(0.09f, 0.85f, 0.28f);
            hairR.GetComponent<Renderer>().sharedMaterial = hairMat;
            Object.DestroyImmediate(hairR.GetComponent<Collider>());

            // Back Hair
            GameObject hairBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hairBack.transform.SetParent(head.transform, false);
            hairBack.transform.localPosition = new Vector3(0f, -0.38f, -0.14f);
            hairBack.transform.localScale = new Vector3(0.42f, 0.95f, 0.18f);
            hairBack.GetComponent<Renderer>().sharedMaterial = hairMat;
            Object.DestroyImmediate(hairBack.GetComponent<Collider>());

            // Eerie Glowing Eyes deep behind the hair
            GameObject eyeLeftObj = new GameObject("Eye_L");
            eyeLeftObj.transform.SetParent(head.transform, false);
            eyeLeftObj.transform.localPosition = new Vector3(-0.065f, 0.02f, 0.14f);
            eyeL = eyeLeftObj.AddComponent<Light>();
            eyeL.type = LightType.Point;
            eyeL.range = 1.4f;
            eyeL.intensity = 1.6f;
            eyeL.color = new Color(0.95f, 0.12f, 0.12f);

            GameObject eyeRightObj = new GameObject("Eye_R");
            eyeRightObj.transform.SetParent(head.transform, false);
            eyeRightObj.transform.localPosition = new Vector3(0.065f, 0.02f, 0.14f);
            eyeR = eyeRightObj.AddComponent<Light>();
            eyeR.type = LightType.Point;
            eyeR.range = 1.4f;
            eyeR.intensity = 1.6f;
            eyeR.color = new Color(0.95f, 0.12f, 0.12f);

            return head;
        }
    }
}
