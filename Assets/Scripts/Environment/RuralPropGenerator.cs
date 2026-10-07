using System.Collections.Generic;
using UnityEngine;

namespace BhootiyaRasta.Environment
{
    public class RuralPropGenerator : MonoBehaviour
    {
        [Header("Materials")]
        public Material clayWallMat;
        public Material thatchRoofMat;
        public Material weatheredWoodMat;
        public Material stoneMasonryMat;
        public Material foliageLeafMat;
        public Material cropLeafMat;
        public Material rustedMetalMat;
        public Material electricalWireMat;

        public void PopulatePropsAlongRoad(ProceduralRoad road, int level)
        {
            // Clear existing props
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            if (road == null || road.roadPoints.Count < 4) return;

            int count = road.roadPoints.Count;
            Random.InitState(level * 777 + 42);

            List<Vector3> polePositions = new List<Vector3>();

            // 1. Continuous Tall Crops (Sugarcane / Jowar) along both sides
            for (int i = 2; i < count - 2; i += 2)
            {
                Vector3 pt = road.roadPoints[i];
                Vector3 fwd = (road.roadPoints[i + 1] - pt).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

                // Crop rows on left and right borders
                CreateCropRow(pt - right * Random.Range(3.8f, 5.2f), fwd, 6);
                CreateCropRow(pt + right * Random.Range(3.8f, 5.2f), fwd, 6);
            }

            // 2. Majestic Old Trees (Banyan with Aerial Roots & Neem Trees)
            int treeSpacing = (level >= 4) ? 3 : 5;
            for (int i = 3; i < count - 4; i += treeSpacing)
            {
                Vector3 pt = road.roadPoints[i];
                Vector3 fwd = (road.roadPoints[i + 1] - pt).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

                // Left Tree
                float distL = Random.Range(5.5f, 15.0f);
                Vector3 posL = pt - right * distL + fwd * Random.Range(-2f, 2f);
                if (i % 2 == 0) CreateIndianBanyanTree(posL, Random.Range(2.2f, 3.4f));
                else CreateNeemTree(posL, Random.Range(2.0f, 3.0f));

                // Right Tree
                float distR = Random.Range(5.5f, 15.0f);
                Vector3 posR = pt + right * distR + fwd * Random.Range(-2f, 2f);
                if ((i + 1) % 2 == 0) CreateIndianBanyanTree(posR, Random.Range(2.2f, 3.4f));
                else CreateNeemTree(posR, Random.Range(2.0f, 3.0f));
            }

            // 3. Rural Electric Poles with Sagging Electrical Catenary Wires
            for (int i = 4; i < count - 4; i += 7)
            {
                Vector3 pt = road.roadPoints[i];
                Vector3 fwd = (road.roadPoints[i + 1] - pt).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

                Vector3 polePos = pt + right * 3.6f;
                polePositions.Add(polePos);
                CreateElectricPole(polePos, fwd, (i % 14 == 0));
            }

            // String sagging electrical wires between poles
            for (int p = 0; p < polePositions.Count - 1; p++)
            {
                CreateSaggingWire(polePositions[p] + Vector3.up * 6.6f, polePositions[p + 1] + Vector3.up * 6.6f);
            }

            // 4. Broken Bamboo Fences
            for (int i = 4; i < count - 6; i += 5)
            {
                Vector3 pt = road.roadPoints[i];
                Vector3 fwd = (road.roadPoints[i + 1] - pt).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

                float side = (Random.value > 0.5f) ? 1f : -1f;
                Vector3 fencePos = pt + right * (side * 3.1f);
                CreateCrookedFence(fencePos, Quaternion.LookRotation(fwd));
            }

            // 5. Authentic Indian Mud Huts (Kaccha Ghar)
            int hutCount = (level == 3) ? 8 : ((level >= 4) ? 5 : 3);
            for (int h = 0; h < hutCount; h++)
            {
                int idx = Random.Range(8, count - 12);
                Vector3 pt = road.roadPoints[idx];
                Vector3 fwd = (road.roadPoints[idx + 1] - pt).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

                float side = (h % 2 == 0) ? 1f : -1f;
                Vector3 hutPos = pt + right * (side * Random.Range(9f, 18f));
                CreateIndianMudHut(hutPos, Quaternion.LookRotation(-right * side + fwd * Random.Range(-0.2f, 0.2f)));

                // Add Haystack beside hut
                CreateHaystack(hutPos + right * (side * 4.5f) + fwd * 3f);
            }

            // 6. Ancient Village Stone Well (Purana Kuan)
            int wellIdx = count / 3;
            if (wellIdx < count - 2)
            {
                Vector3 pt = road.roadPoints[wellIdx];
                Vector3 fwd = (road.roadPoints[wellIdx + 1] - pt).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
                CreateAncientStoneWell(pt + right * 6.5f);
            }

            // 7. Indian Hand Pump (Nalka)
            int pumpIdx = count / 2;
            if (pumpIdx < count - 2)
            {
                Vector3 pt = road.roadPoints[pumpIdx];
                Vector3 fwd = (road.roadPoints[pumpIdx + 1] - pt).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
                CreateIndianHandPump(pt - right * 4.5f);
            }

            // 8. Abandoned Bullock Cart / Trailer Wreckage
            int cartIdx = (count * 2) / 3;
            if (cartIdx < count - 2)
            {
                Vector3 pt = road.roadPoints[cartIdx];
                Vector3 fwd = (road.roadPoints[cartIdx + 1] - pt).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
                CreateAbandonedCartWreckage(pt - right * 6.0f);
            }

            // 9. Far-layer Environment Depth: Distant Mountain / Hill Silhouettes
            CreateDistantHills(road);
        }

        private void CreateIndianBanyanTree(Vector3 pos, float scale)
        {
            GameObject tree = new GameObject("IndianBanyanTree");
            tree.transform.SetParent(transform, true);
            tree.transform.position = pos;
            tree.transform.localScale = Vector3.one * scale;

            // Gnarled main trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "BanyanTrunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            trunk.transform.localScale = new Vector3(0.75f, 1.8f, 0.75f);
            if (weatheredWoodMat != null) trunk.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

            // Large spreading branches
            for (int b = 0; b < 4; b++)
            {
                float angle = b * 90f + Random.Range(-15f, 15f);
                Quaternion rot = Quaternion.Euler(Random.Range(35f, 50f), angle, 0f);

                GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                branch.transform.SetParent(tree.transform, false);
                branch.transform.localPosition = new Vector3(0f, 2.8f, 0f);
                branch.transform.localRotation = rot;
                branch.transform.localScale = new Vector3(0.28f, 1.4f, 0.28f);
                if (weatheredWoodMat != null) branch.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

                // Iconic Dangling Aerial Roots dropping to the ground!
                Vector3 branchTip = branch.transform.position + branch.transform.up * (1.4f * scale);
                Vector3 localTip = tree.transform.InverseTransformPoint(branchTip);

                GameObject aerialRoot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                aerialRoot.name = "DanglingAerialRoot";
                aerialRoot.transform.SetParent(tree.transform, false);
                aerialRoot.transform.localPosition = new Vector3(localTip.x, localTip.y * 0.5f, localTip.z);
                aerialRoot.transform.localScale = new Vector3(0.065f, localTip.y * 0.5f, 0.065f);
                if (weatheredWoodMat != null) aerialRoot.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;
            }

            // Broad canopy clusters
            for (int c = 0; c < 3; c++)
            {
                GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopy.name = "BanyanCanopy";
                canopy.transform.SetParent(tree.transform, false);
                canopy.transform.localPosition = new Vector3(Random.Range(-0.8f, 0.8f), 3.8f + c * 0.4f, Random.Range(-0.8f, 0.8f));
                canopy.transform.localScale = new Vector3(3.6f, 2.2f, 3.6f);
                if (foliageLeafMat != null) canopy.GetComponent<Renderer>().sharedMaterial = foliageLeafMat;
            }
        }

        private void CreateNeemTree(Vector3 pos, float scale)
        {
            GameObject tree = new GameObject("IndianNeemTree");
            tree.transform.SetParent(transform, true);
            tree.transform.position = pos;
            tree.transform.localScale = Vector3.one * scale;

            // Slender crooked trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "NeemTrunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            trunk.transform.localScale = new Vector3(0.42f, 2.1f, 0.42f);
            trunk.transform.localRotation = Quaternion.Euler(Random.Range(-5f, 5f), 0f, Random.Range(-6f, 6f));
            if (weatheredWoodMat != null) trunk.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

            // Dense upper foliage crown
            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "NeemCrown";
            crown.transform.SetParent(tree.transform, false);
            crown.transform.localPosition = new Vector3(0f, 4.2f, 0f);
            crown.transform.localScale = new Vector3(2.8f, 2.6f, 2.8f);
            if (foliageLeafMat != null) crown.GetComponent<Renderer>().sharedMaterial = foliageLeafMat;
        }

        private void CreateCropRow(Vector3 startPos, Vector3 forward, int stalks)
        {
            GameObject row = new GameObject("SugarcaneRow");
            row.transform.SetParent(transform, true);
            row.transform.position = startPos;
            row.transform.rotation = Quaternion.LookRotation(forward);

            for (int i = 0; i < stalks; i++)
            {
                // Tall sugarcane/jowar stalk
                GameObject stalk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stalk.transform.SetParent(row.transform, false);
                stalk.transform.localPosition = new Vector3(Random.Range(-0.25f, 0.25f), 1.15f, i * 0.7f);
                stalk.transform.localScale = new Vector3(0.06f, 1.15f, 0.06f);
                stalk.transform.localRotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(0f, 360f), Random.Range(-8f, 8f));
                if (cropLeafMat != null) stalk.GetComponent<Renderer>().sharedMaterial = cropLeafMat;
                DestroyImmediate(stalk.GetComponent<Collider>());

                // Top leafy plume
                GameObject plume = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                plume.transform.SetParent(stalk.transform, false);
                plume.transform.localPosition = new Vector3(0f, 1.0f, 0f);
                plume.transform.localScale = new Vector3(10f, 6f, 10f);
                if (cropLeafMat != null) plume.GetComponent<Renderer>().sharedMaterial = cropLeafMat;
                DestroyImmediate(plume.GetComponent<Collider>());
            }
        }

        private void CreateElectricPole(Vector3 pos, Vector3 roadForward, bool hasBulb)
        {
            GameObject pole = new GameObject("RuralElectricPole");
            pole.transform.SetParent(transform, true);
            pole.transform.position = pos;
            pole.transform.rotation = Quaternion.LookRotation(roadForward);

            // Tall wooden mast
            GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mast.transform.SetParent(pole.transform, false);
            mast.transform.localPosition = new Vector3(0f, 3.6f, 0f);
            mast.transform.localScale = new Vector3(0.24f, 3.6f, 0.24f);
            if (weatheredWoodMat != null) mast.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

            // Cross arm beam
            GameObject cross = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cross.transform.SetParent(pole.transform, false);
            cross.transform.localPosition = new Vector3(0f, 6.7f, 0f);
            cross.transform.localScale = new Vector3(1.9f, 0.14f, 0.14f);
            if (weatheredWoodMat != null) cross.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

            // Ceramic Insulator Cups
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject ins = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ins.transform.SetParent(cross.transform, false);
                ins.transform.localPosition = new Vector3(side * 0.82f, 0.12f, 0f);
                ins.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
                if (stoneMasonryMat != null) ins.GetComponent<Renderer>().sharedMaterial = stoneMasonryMat;
            }

            if (hasBulb)
            {
                GameObject lightObj = new GameObject("FlickeringBulb");
                lightObj.transform.SetParent(pole.transform, false);
                lightObj.transform.localPosition = new Vector3(0f, 6.2f, 0.25f);

                Light pt = lightObj.AddComponent<Light>();
                pt.type = LightType.Point;
                pt.range = 14f;
                pt.intensity = 1.6f;
                pt.color = new Color(1.0f, 0.85f, 0.5f);
                lightObj.AddComponent<FlickeringLight>();
            }
        }

        private void CreateSaggingWire(Vector3 start, Vector3 end)
        {
            GameObject wire = new GameObject("SaggingWire");
            wire.transform.SetParent(transform, true);

            LineRenderer lr = wire.AddComponent<LineRenderer>();
            lr.startWidth = 0.035f;
            lr.endWidth = 0.035f;
            lr.positionCount = 8;
            lr.useWorldSpace = true;
            if (electricalWireMat != null) lr.sharedMaterial = electricalWireMat;

            // Catenary sag
            for (int i = 0; i < 8; i++)
            {
                float t = (float)i / 7f;
                Vector3 p = Vector3.Lerp(start, end, t);
                float sag = Mathf.Sin(t * Mathf.PI) * 0.75f;
                p.y -= sag;
                lr.SetPosition(i, p);
            }
        }

        private void CreateCrookedFence(Vector3 pos, Quaternion rot)
        {
            GameObject fence = new GameObject("CrookedBambooFence");
            fence.transform.SetParent(transform, true);
            fence.transform.position = pos;
            fence.transform.rotation = rot;

            for (int i = 0; i < 3; i++)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.transform.SetParent(fence.transform, false);
                post.transform.localPosition = new Vector3(0f, 0.65f, i * 1.5f);
                post.transform.localScale = new Vector3(0.09f, 0.65f, 0.09f);
                post.transform.localRotation = Quaternion.Euler(Random.Range(-8f, 8f), 0f, Random.Range(-10f, 10f));
                if (weatheredWoodMat != null) post.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;
            }

            // Horizontal bamboo rails
            for (int r = 0; r < 2; r++)
            {
                GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rail.transform.SetParent(fence.transform, false);
                rail.transform.localPosition = new Vector3(0f, 0.45f + r * 0.4f, 1.5f);
                rail.transform.localScale = new Vector3(0.06f, 0.07f, 3.2f);
                if (weatheredWoodMat != null) rail.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;
            }
        }

        private void CreateIndianMudHut(Vector3 pos, Quaternion rot)
        {
            GameObject hut = new GameObject("IndianMudHut");
            hut.transform.SetParent(transform, true);
            hut.transform.position = pos;
            hut.transform.rotation = rot;

            // Clay Walls
            GameObject walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walls.name = "PlasteredClayWalls";
            walls.transform.SetParent(hut.transform, false);
            walls.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            walls.transform.localScale = new Vector3(4.6f, 2.7f, 3.8f);
            if (clayWallMat != null) walls.GetComponent<Renderer>().sharedMaterial = clayWallMat;

            // Wooden Doorway Frame
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "WoodenDoorFrame";
            door.transform.SetParent(hut.transform, false);
            door.transform.localPosition = new Vector3(0f, 0.95f, 1.91f);
            door.transform.localScale = new Vector3(1.1f, 1.9f, 0.08f);
            if (weatheredWoodMat != null) door.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

            // Overhanging Thatched Straw Roof (Chhappar)
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            roof.name = "ThatchedChhappar";
            roof.transform.SetParent(hut.transform, false);
            roof.transform.localPosition = new Vector3(0f, 3.1f, 0f);
            roof.transform.localScale = new Vector3(5.4f, 0.85f, 4.6f);
            if (thatchRoofMat != null) roof.GetComponent<Renderer>().sharedMaterial = thatchRoofMat;
        }

        private void CreateHaystack(Vector3 pos)
        {
            GameObject haystack = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            haystack.name = "StrawHaystack";
            haystack.transform.SetParent(transform, true);
            haystack.transform.position = pos + Vector3.up * 0.9f;
            haystack.transform.localScale = new Vector3(2.6f, 1.8f, 2.6f);
            if (thatchRoofMat != null) haystack.GetComponent<Renderer>().sharedMaterial = thatchRoofMat;
        }

        private void CreateAncientStoneWell(Vector3 pos)
        {
            GameObject well = new GameObject("PuranaKuan_StoneWell");
            well.transform.SetParent(transform, true);
            well.transform.position = pos;

            // Round masonry wall
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wall.transform.SetParent(well.transform, false);
            wall.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            wall.transform.localScale = new Vector3(2.8f, 0.55f, 2.8f);
            if (stoneMasonryMat != null) wall.GetComponent<Renderer>().sharedMaterial = stoneMasonryMat;

            // Wooden A-Frame Hoist & Crossbeam
            GameObject postL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            postL.transform.SetParent(well.transform, false);
            postL.transform.localPosition = new Vector3(-1.15f, 1.55f, 0f);
            postL.transform.localScale = new Vector3(0.16f, 2.0f, 0.16f);
            if (weatheredWoodMat != null) postL.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

            GameObject postR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            postR.transform.SetParent(well.transform, false);
            postR.transform.localPosition = new Vector3(1.15f, 1.55f, 0f);
            postR.transform.localScale = new Vector3(0.16f, 2.0f, 0.16f);
            if (weatheredWoodMat != null) postR.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.transform.SetParent(well.transform, false);
            beam.transform.localPosition = new Vector3(0f, 2.55f, 0f);
            beam.transform.localScale = new Vector3(2.6f, 0.16f, 0.16f);
            if (weatheredWoodMat != null) beam.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

            // Iron Pulley Wheel & Dangling Bucket
            GameObject pulley = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pulley.transform.SetParent(beam.transform, false);
            pulley.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            pulley.transform.localScale = new Vector3(0.28f, 0.05f, 0.28f);
            pulley.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            if (rustedMetalMat != null) pulley.GetComponent<Renderer>().sharedMaterial = rustedMetalMat;

            GameObject bucket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bucket.name = "WaterBucket";
            bucket.transform.SetParent(well.transform, false);
            bucket.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            bucket.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            if (rustedMetalMat != null) bucket.GetComponent<Renderer>().sharedMaterial = rustedMetalMat;
        }

        private void CreateIndianHandPump(Vector3 pos)
        {
            GameObject pump = new GameObject("IndianNalkaHandpump");
            pump.transform.SetParent(transform, true);
            pump.transform.position = pos;

            // Raised octagonal concrete platform
            GameObject basePlat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePlat.transform.SetParent(pump.transform, false);
            basePlat.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            basePlat.transform.localScale = new Vector3(2.4f, 0.1f, 2.4f);
            if (stoneMasonryMat != null) basePlat.GetComponent<Renderer>().sharedMaterial = stoneMasonryMat;

            // Cast Iron Pump Cylinder
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.transform.SetParent(pump.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            body.transform.localScale = new Vector3(0.26f, 0.75f, 0.26f);
            if (rustedMetalMat != null) body.GetComponent<Renderer>().sharedMaterial = rustedMetalMat;

            // Curved Spout
            GameObject spout = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spout.transform.SetParent(pump.transform, false);
            spout.transform.localPosition = new Vector3(0f, 0.75f, 0.28f);
            spout.transform.localScale = new Vector3(0.08f, 0.22f, 0.08f);
            spout.transform.localRotation = Quaternion.Euler(65f, 0f, 0f);
            if (rustedMetalMat != null) spout.GetComponent<Renderer>().sharedMaterial = rustedMetalMat;

            // Long Lever Handle
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.transform.SetParent(pump.transform, false);
            handle.transform.localPosition = new Vector3(0.48f, 1.22f, 0f);
            handle.transform.localScale = new Vector3(0.95f, 0.05f, 0.05f);
            handle.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
            if (rustedMetalMat != null) handle.GetComponent<Renderer>().sharedMaterial = rustedMetalMat;
        }

        private void CreateAbandonedCartWreckage(Vector3 pos)
        {
            GameObject cart = new GameObject("AbandonedBullockCartWreckage");
            cart.transform.SetParent(transform, true);
            cart.transform.position = pos;
            cart.transform.localRotation = Quaternion.Euler(12f, Random.Range(20f, 60f), -8f);

            // Broken wooden bed chassis
            GameObject bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bed.transform.SetParent(cart.transform, false);
            bed.transform.localScale = new Vector3(1.8f, 0.12f, 2.8f);
            if (weatheredWoodMat != null) bed.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;

            // Wooden Spoke Wheel
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.transform.SetParent(cart.transform, false);
            wheel.transform.localPosition = new Vector3(1.05f, 0.45f, 0f);
            wheel.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f);
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 82f);
            if (weatheredWoodMat != null) wheel.GetComponent<Renderer>().sharedMaterial = weatheredWoodMat;
        }

        private void CreateDistantHills(ProceduralRoad road)
        {
            GameObject hills = new GameObject("DistantHillsSilhouettes");
            hills.transform.SetParent(transform, false);

            float length = road.roadLength;
            int hillCount = 10;

            for (int i = 0; i < hillCount; i++)
            {
                float z = (float)i / hillCount * length;
                Vector3 pt = road.GetPointAtDistance(z);

                // Left distant ridge
                GameObject hillL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hillL.transform.SetParent(hills.transform, false);
                hillL.transform.position = pt + new Vector3(-130f, 14f, Random.Range(-25f, 25f));
                hillL.transform.localScale = new Vector3(110f, 40f, 110f);
                if (stoneMasonryMat != null) hillL.GetComponent<Renderer>().sharedMaterial = stoneMasonryMat;
                DestroyImmediate(hillL.GetComponent<Collider>());

                // Right distant ridge
                GameObject hillR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hillR.transform.SetParent(hills.transform, false);
                hillR.transform.position = pt + new Vector3(130f, 14f, Random.Range(-25f, 25f));
                hillR.transform.localScale = new Vector3(110f, 40f, 110f);
                if (stoneMasonryMat != null) hillR.GetComponent<Renderer>().sharedMaterial = stoneMasonryMat;
                DestroyImmediate(hillR.GetComponent<Collider>());
            }
        }
    }

    public class FlickeringLight : MonoBehaviour
    {
        private Light lt;
        private float originalIntensity;

        private void Start()
        {
            lt = GetComponent<Light>();
            if (lt != null) originalIntensity = lt.intensity;
        }

        private void Update()
        {
            if (lt == null) return;
            if (Random.value < 0.15f)
            {
                lt.intensity = (Random.value < 0.4f) ? 0.1f : (originalIntensity * Random.Range(0.6f, 1.3f));
            }
        }
    }
}
