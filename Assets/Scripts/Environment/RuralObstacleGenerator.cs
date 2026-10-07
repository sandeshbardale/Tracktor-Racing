using System.Collections.Generic;
using UnityEngine;

namespace BhootiyaRasta.Environment
{
    public class RuralObstacleGenerator : MonoBehaviour
    {
        [Header("PBR Materials")]
        public Material weatheredWoodMat;
        public Material stoneMasonryMat;
        public Material mudPuddleMat;
        public Material thatchStrawMat;
        public Material rustedMetalMat;

        public void GenerateObstaclesForLevel(List<Vector3> roadPoints, int level)
        {
            // Clear existing obstacles
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            if (roadPoints == null || roadPoints.Count < 10) return;

            int totalPts = roadPoints.Count;
            Random.InitState(level * 999 + 77);

            // Number of obstacles scaling with level (Level 1: 4, Level 2: 6, Level 3: 8, Level 4: 10, Level 5: 13)
            int obstacleCount = Mathf.Clamp(3 + level * 2, 4, 14);
            float stepIndex = (float)(totalPts - 14) / (obstacleCount + 1);

            for (int o = 0; o < obstacleCount; o++)
            {
                int ptIdx = Mathf.Clamp(Mathf.RoundToInt(6 + o * stepIndex + Random.Range(-1.5f, 1.5f)), 4, totalPts - 6);
                Vector3 pt = roadPoints[ptIdx];
                Vector3 nextPt = roadPoints[ptIdx + 1];
                Vector3 forward = (nextPt - pt).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

                // Alternate obstacle types with clear navigable bypass corridors
                int type = (o + level) % 6;
                float side = (o % 2 == 0) ? 1.0f : -1.0f;

                switch (type)
                {
                    case 0:
                        // Fallen Tree Trunk / Log across one lane (leaves other lane open)
                        CreateFallenLog(pt + right * (side * 1.3f), forward, side);
                        break;
                    case 1:
                        // Large Road Boulder / Granite Rock on edge
                        CreateGraniteBoulder(pt + right * (side * 1.5f), Random.Range(1.1f, 1.5f));
                        break;
                    case 2:
                        // Mud Puddle (Kaccha muddy water puddle)
                        CreateMudPuddle(pt + right * (side * 0.7f), forward);
                        break;
                    case 3:
                        // Broken Bamboo Fence protruding into road
                        CreateBrokenFenceObstacle(pt + right * (side * 1.7f), forward, side);
                        break;
                    case 4:
                        // Abandoned Cart / Broken Wheel Obstacle
                        CreateAbandonedWheelWreck(pt + right * (side * 1.4f), forward);
                        break;
                    case 5:
                        // Hay Bale Pile on lane
                        CreateHayBaleObstacle(pt + right * (side * 1.3f));
                        break;
                }
            }

            // Always create the illuminated Destination Village Gate at the road end!
            Vector3 endPt = roadPoints[totalPts - 1];
            Vector3 prevEnd = roadPoints[totalPts - 2];
            Vector3 endFwd = (endPt - prevEnd).normalized;
            CreateDestinationVillageGate(endPt, endFwd);
        }

        private void CreateFallenLog(Vector3 pos, Vector3 roadForward, float side)
        {
            GameObject log = new GameObject("Obstacle_FallenLog");
            log.transform.SetParent(transform, true);
            log.transform.position = pos + Vector3.up * 0.28f;

            // Angle across road
            Quaternion rot = Quaternion.LookRotation(roadForward) * Quaternion.Euler(0f, side * 38f, 0f);
            log.transform.rotation = rot;

            GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.name = "LogTrunk";
            cyl.transform.SetParent(log.transform, false);
            cyl.transform.localScale = new Vector3(0.55f, 2.4f, 0.55f);
            cyl.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            if (weatheredWoodMat != null) cyl.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

            // Tag as obstacle
            cyl.tag = "Finish"; // or obstacle tag
        }

        private void CreateGraniteBoulder(Vector3 pos, float scale)
        {
            GameObject boulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            boulder.name = "Obstacle_GraniteBoulder";
            boulder.transform.SetParent(transform, true);
            boulder.transform.position = pos + Vector3.up * (scale * 0.42f);
            boulder.transform.localScale = new Vector3(scale * 1.2f, scale * 0.85f, scale);
            if (stoneMasonryMat != null) boulder.GetComponent<Renderer>().sharedMaterial = stoneMasonryMat;
        }

        private void CreateMudPuddle(Vector3 pos, Vector3 forward)
        {
            GameObject mud = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mud.name = "Obstacle_MudPuddle";
            mud.transform.SetParent(transform, true);
            mud.transform.position = pos + Vector3.up * 0.02f;
            mud.transform.rotation = Quaternion.LookRotation(forward);
            mud.transform.localScale = new Vector3(2.6f, 0.05f, 4.2f);
            if (mudPuddleMat != null) mud.GetComponent<Renderer>().sharedMaterial = mudPuddleMat;

            // Add MudPuddle trigger component so it slows tractor
            var col = mud.GetComponent<Collider>();
            col.isTrigger = true;
            mud.AddComponent<MudPuddle>();
        }

        private void CreateBrokenFenceObstacle(Vector3 pos, Vector3 forward, float side)
        {
            GameObject fence = new GameObject("Obstacle_BrokenFence");
            fence.transform.SetParent(transform, true);
            fence.transform.position = pos;
            fence.transform.rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0f, -side * 25f, 0f);

            for (int p = 0; p < 2; p++)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.transform.SetParent(fence.transform, false);
                post.transform.localPosition = new Vector3(0f, 0.6f, p * 1.6f);
                post.transform.localScale = new Vector3(0.08f, 0.6f, 0.08f);
                post.transform.localRotation = Quaternion.Euler(Random.Range(-12f, 12f), 0f, Random.Range(-15f, 15f));
                if (weatheredWoodMat != null) post.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;
            }

            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.transform.SetParent(fence.transform, false);
            rail.transform.localPosition = new Vector3(0f, 0.45f, 0.8f);
            rail.transform.localScale = new Vector3(0.06f, 0.08f, 2.2f);
            rail.transform.localRotation = Quaternion.Euler(Random.Range(-5f, 5f), 0f, 0f);
            if (weatheredWoodMat != null) rail.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;
        }

        private void CreateAbandonedWheelWreck(Vector3 pos, Vector3 forward)
        {
            GameObject wreck = new GameObject("Obstacle_AbandonedWheelWreck");
            wreck.transform.SetParent(transform, true);
            wreck.transform.position = pos + Vector3.up * 0.35f;
            wreck.transform.rotation = Quaternion.LookRotation(forward);

            // Broken wheel rim
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.transform.SetParent(wreck.transform, false);
            wheel.transform.localScale = new Vector3(1.1f, 0.15f, 1.1f);
            wheel.transform.localRotation = Quaternion.Euler(45f, 25f, 0f);
            if (rustedMetalMat != null) wheel.GetComponent<Renderer>().sharedMaterial = rustedMetalMat;

            // Broken axle shaft
            GameObject axle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            axle.transform.SetParent(wreck.transform, false);
            axle.transform.localScale = new Vector3(0.12f, 0.12f, 1.8f);
            axle.transform.localRotation = Quaternion.Euler(15f, -30f, 0f);
            if (weatheredWoodMat != null) axle.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;
        }

        private void CreateHayBaleObstacle(Vector3 pos)
        {
            GameObject hay = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hay.name = "Obstacle_HayBale";
            hay.transform.SetParent(transform, true);
            hay.transform.position = pos + Vector3.up * 0.55f;
            hay.transform.localScale = new Vector3(1.4f, 0.7f, 1.4f);
            hay.transform.localRotation = Quaternion.Euler(90f, 35f, 0f);
            if (thatchStrawMat != null) hay.GetComponent<Renderer>().sharedMaterial = thatchStrawMat;
        }

        private void CreateDestinationVillageGate(Vector3 pos, Vector3 forward)
        {
            GameObject gate = new GameObject("Destination_VillageGate");
            gate.transform.SetParent(transform, true);
            gate.transform.position = pos;
            gate.transform.rotation = Quaternion.LookRotation(forward);

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            // Left Wooden Column
            GameObject colL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            colL.name = "GateColumn_L";
            colL.transform.SetParent(gate.transform, false);
            colL.transform.localPosition = new Vector3(-3.2f, 2.5f, 0f);
            colL.transform.localScale = new Vector3(0.35f, 2.5f, 0.35f);
            if (weatheredWoodMat != null) colL.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

            // Right Wooden Column
            GameObject colR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            colR.name = "GateColumn_R";
            colR.transform.SetParent(gate.transform, false);
            colR.transform.localPosition = new Vector3(3.2f, 2.5f, 0f);
            colR.transform.localScale = new Vector3(0.35f, 2.5f, 0.35f);
            if (weatheredWoodMat != null) colR.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

            // Top Arch Beam
            GameObject arch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arch.name = "GateArchBeam";
            arch.transform.SetParent(gate.transform, false);
            arch.transform.localPosition = new Vector3(0f, 4.8f, 0f);
            arch.transform.localScale = new Vector3(7.2f, 0.45f, 0.45f);
            if (weatheredWoodMat != null) arch.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

            // Ornamental Thatched Roof Peak
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            roof.name = "GateThatchedRoof";
            roof.transform.SetParent(gate.transform, false);
            roof.transform.localPosition = new Vector3(0f, 5.35f, 0f);
            roof.transform.localScale = new Vector3(7.8f, 0.45f, 1.4f);
            roof.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            if (thatchStrawMat != null) roof.GetComponent<Renderer>().sharedMaterial = thatchStrawMat;

            // Left & Right Warm Glowing Lanterns (Safe Haven visual beacon)
            for (int s = -1; s <= 1; s += 2)
            {
                GameObject lantern = new GameObject($"Lantern_{(s == -1 ? "L" : "R")}");
                lantern.transform.SetParent(gate.transform, false);
                lantern.transform.localPosition = new Vector3(s * 2.8f, 3.8f, 0f);

                // Lantern casing
                GameObject lampCase = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lampCase.transform.SetParent(lantern.transform, false);
                lampCase.transform.localScale = new Vector3(0.35f, 0.45f, 0.35f);
                if (rustedMetalMat != null) lampCase.GetComponent<Renderer>().sharedMaterial = rustedMetalMat;

                // Glowing Point Light
                Light ptLight = lantern.AddComponent<Light>();
                ptLight.type = LightType.Point;
                ptLight.range = 16f;
                ptLight.intensity = 2.8f;
                ptLight.color = new Color(1.0f, 0.85f, 0.45f); // Warm safe haven golden amber
                ptLight.shadows = LightShadows.Soft;

                lantern.AddComponent<FlickeringLight>();
            }

            // Destination Checkpoint Trigger Zone
            GameObject triggerObj = new GameObject("DestinationTriggerZone");
            triggerObj.transform.SetParent(gate.transform, false);
            triggerObj.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            BoxCollider bc = triggerObj.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(8.0f, 4.0f, 4.0f);
        }
    }
}
