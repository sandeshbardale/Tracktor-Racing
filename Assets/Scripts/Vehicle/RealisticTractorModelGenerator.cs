using System.Collections.Generic;
using UnityEngine;

namespace BhootiyaRasta.Vehicle
{
    public static class RealisticTractorModelGenerator
    {
        public static GameObject CreateRealisticTractor(
            Material bodyPaintMat,
            Material engineMetalMat,
            Material tireRubberMat,
            Material rimSteelMat,
            Material headlightGlowMat,
            Material taillightGlowMat,
            Material glassMat)
        {
            GameObject tractorRoot = new GameObject("IndianTractor_Mahindra");
            tractorRoot.tag = "Player";

            var rb = tractorRoot.AddComponent<Rigidbody>();
            rb.mass = 2300f; // ~45 HP Indian tractor mass (e.g. Mahindra 575 DI / Swaraj 744)
            rb.centerOfMass = new Vector3(0f, -0.2f, 0.1f);
            rb.linearDamping = 0.8f;
            rb.angularDamping = 3.0f;

            // Main vehicle collider for physics
            var col = tractorRoot.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.95f, 0.15f);
            col.size = new Vector3(2.1f, 1.9f, 3.8f);

            var controller = tractorRoot.AddComponent<TractorController>();
            tractorRoot.AddComponent<TractorAudio>();

            // Chassis & Visual Parent
            GameObject chassis = new GameObject("ChassisVisual");
            chassis.transform.SetParent(tractorRoot.transform, false);
            controller.tractorBody = chassis.transform;

            // 1. Sleek Indian Tractor Bonnet / Hood
            GameObject hood = CreateBonnetMesh(bodyPaintMat, engineMetalMat);
            hood.transform.SetParent(chassis.transform, false);
            hood.transform.localPosition = new Vector3(0f, 0.85f, 0.75f);

            // 2. Detailed Diesel Engine & Transmission Block
            GameObject engine = CreateEngineBlock(engineMetalMat);
            engine.transform.SetParent(chassis.transform, false);
            engine.transform.localPosition = new Vector3(0f, 0.45f, 0.5f);

            // 3. Driver Station (Footboards, Pedals, Console, Seat)
            GameObject cockpit = CreateDriverStation(engineMetalMat, bodyPaintMat, tireRubberMat, out Transform steeringWheelTf);
            cockpit.transform.SetParent(chassis.transform, false);
            cockpit.transform.localPosition = new Vector3(0f, 0.55f, -0.65f);
            controller.steeringWheel = steeringWheelTf;

            // 4. Curved Agricultural Mudguards (Fenders)
            GameObject fenders = CreateRearMudguards(bodyPaintMat, engineMetalMat);
            fenders.transform.SetParent(chassis.transform, false);
            fenders.transform.localPosition = new Vector3(0f, 1.0f, -0.75f);

            // 5. Vertical Exhaust Chimney with Rain Flapper
            GameObject exhaust = CreateExhaustChimney(engineMetalMat);
            exhaust.transform.SetParent(chassis.transform, false);
            exhaust.transform.localPosition = new Vector3(0.48f, 1.25f, 1.25f);

            // 6. Rear 3-Point Hydraulic Hitch & PTO Assembly
            GameObject hitch = CreateThreePointHitch(engineMetalMat);
            hitch.transform.SetParent(chassis.transform, false);
            hitch.transform.localPosition = new Vector3(0f, 0.5f, -1.85f);

            // 7. Front Cast Axle & Heavy Counterweight Bumper
            GameObject frontBumper = CreateFrontAxleAndBumper(engineMetalMat);
            frontBumper.transform.SetParent(chassis.transform, false);
            frontBumper.transform.localPosition = new Vector3(0f, 0.35f, 1.8f);

            // 8. Realistic Agricultural Wheels with Chevron V-Tread
            // Giant Rear Wheels (14.9-28 Ag Tires)
            GameObject rearL = CreateAgTire(true, tireRubberMat, rimSteelMat);
            rearL.name = "RearWheel_Left";
            rearL.transform.SetParent(chassis.transform, false);
            rearL.transform.localPosition = new Vector3(-1.0f, 0.72f, -0.75f);
            controller.rearLeftWheel = rearL.transform;

            GameObject rearR = CreateAgTire(true, tireRubberMat, rimSteelMat);
            rearR.name = "RearWheel_Right";
            rearR.transform.SetParent(chassis.transform, false);
            rearR.transform.localPosition = new Vector3(1.0f, 0.72f, -0.75f);
            rearR.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            controller.rearRightWheel = rearR.transform;

            // Smaller Front Steering Wheels (6.00-16 Ribbed Steer Tires)
            GameObject frontL = CreateAgTire(false, tireRubberMat, rimSteelMat);
            frontL.name = "FrontWheel_Left";
            frontL.transform.SetParent(chassis.transform, false);
            frontL.transform.localPosition = new Vector3(-0.82f, 0.44f, 1.35f);
            controller.frontLeftWheel = frontL.transform;

            GameObject frontR = CreateAgTire(false, tireRubberMat, rimSteelMat);
            frontR.name = "FrontWheel_Right";
            frontR.transform.SetParent(chassis.transform, false);
            frontR.transform.localPosition = new Vector3(0.82f, 0.44f, 1.35f);
            frontR.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            controller.frontRightWheel = frontR.transform;

            // 9. Working Dual Headlights (Spotlights + Lenses)
            Light hlLeft = CreateTractorHeadlight("Headlight_L", chassis.transform, new Vector3(-0.42f, 1.15f, 1.82f), headlightGlowMat, glassMat);
            Light hlRight = CreateTractorHeadlight("Headlight_R", chassis.transform, new Vector3(0.42f, 1.15f, 1.82f), headlightGlowMat, glassMat);

            // 10. Rear Safety Lights
            Light tlLeft = CreateTractorTaillight("Taillight_L", fenders.transform, new Vector3(-0.95f, 0.35f, -0.72f), taillightGlowMat);
            Light tlRight = CreateTractorTaillight("Taillight_R", fenders.transform, new Vector3(0.95f, 0.35f, -0.72f), taillightGlowMat);

            // Link lights to TractorController via Reflection
            var fieldH = typeof(TractorController).GetField("headLights", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldH != null) fieldH.SetValue(controller, new Light[] { hlLeft, hlRight });

            var fieldT = typeof(TractorController).GetField("tailLights", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldT != null) fieldT.SetValue(controller, new Light[] { tlLeft, tlRight });

            return tractorRoot;
        }

        private static GameObject CreateBonnetMesh(Material paintMat, Material grillMat)
        {
            GameObject bonnet = new GameObject("TractorBonnet");

            // Main curved red hood body
            GameObject hoodBody = new GameObject("HoodBody");
            hoodBody.transform.SetParent(bonnet.transform, false);
            MeshFilter mf = hoodBody.AddComponent<MeshFilter>();
            MeshRenderer mr = hoodBody.AddComponent<MeshRenderer>();
            mr.sharedMaterial = paintMat;

            Mesh m = new Mesh();
            m.name = "BonnetLoft";

            // Procedural tapered cross-sections along Z (front to back)
            // Section 0: Front grille nose (narrower, rounded)
            // Section 1: Mid bonnet
            // Section 2: Rear dashboard cowl (wider)
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int rings = 8;
            int ringSegments = 12;

            for (int r = 0; r < rings; r++)
            {
                float tZ = (float)r / (rings - 1);
                float z = Mathf.Lerp(1.1f, -0.85f, tZ);
                float width = Mathf.Lerp(0.52f, 0.65f, tZ);
                float height = Mathf.Lerp(0.55f, 0.65f, tZ);
                float topY = Mathf.Lerp(0.48f, 0.58f, tZ);

                for (int s = 0; s < ringSegments; s++)
                {
                    float angle = Mathf.PI * (float)s / (ringSegments - 1);
                    float x = Mathf.Cos(angle) * width;
                    float y = topY - Mathf.Sin(angle) * height;
                    verts.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2((float)s / (ringSegments - 1), tZ));
                }
            }

            for (int r = 0; r < rings - 1; r++)
            {
                for (int s = 0; s < ringSegments - 1; s++)
                {
                    int i0 = r * ringSegments + s;
                    int i1 = i0 + 1;
                    int i2 = (r + 1) * ringSegments + s;
                    int i3 = i2 + 1;

                    tris.Add(i0); tris.Add(i2); tris.Add(i1);
                    tris.Add(i1); tris.Add(i2); tris.Add(i3);
                }
            }

            m.SetVertices(verts);
            m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            mf.sharedMesh = m;

            // Front Radiator Grille (Dark textured honeycomb inset)
            GameObject grille = GameObject.CreatePrimitive(PrimitiveType.Quad);
            grille.name = "RadiatorGrille";
            grille.transform.SetParent(bonnet.transform, false);
            grille.transform.localPosition = new Vector3(0f, 0.22f, 1.11f);
            grille.transform.localScale = new Vector3(0.95f, 0.65f, 1f);
            grille.GetComponent<Renderer>().sharedMaterial = grillMat;
            Object.DestroyImmediate(grille.GetComponent<Collider>());

            // Chrome Trim / Brand Badge Bar
            GameObject badge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            badge.name = "BrandEmblemBar";
            badge.transform.SetParent(bonnet.transform, false);
            badge.transform.localPosition = new Vector3(0f, 0.44f, 1.12f);
            badge.transform.localScale = new Vector3(0.7f, 0.08f, 0.04f);
            badge.GetComponent<Renderer>().sharedMaterial = grillMat;
            Object.DestroyImmediate(badge.GetComponent<Collider>());

            return bonnet;
        }

        private static GameObject CreateEngineBlock(Material metalMat)
        {
            GameObject engine = new GameObject("DieselEngineBlock");

            // Main Crankcase & Cylinder Block
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.transform.SetParent(engine.transform, false);
            block.transform.localPosition = new Vector3(0f, 0.0f, 0.2f);
            block.transform.localScale = new Vector3(0.75f, 0.7f, 1.5f);
            block.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(block.GetComponent<Collider>());

            // Oil Pan (Sump beneath engine)
            GameObject sump = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sump.transform.SetParent(engine.transform, false);
            sump.transform.localPosition = new Vector3(0f, -0.38f, 0.2f);
            sump.transform.localScale = new Vector3(0.55f, 0.22f, 1.3f);
            sump.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(sump.GetComponent<Collider>());

            // Cylinder Head & Valve Cover
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(engine.transform, false);
            head.transform.localPosition = new Vector3(0f, 0.38f, 0.2f);
            head.transform.localScale = new Vector3(0.65f, 0.15f, 1.35f);
            head.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(head.GetComponent<Collider>());

            // Starter Motor & Alternator
            GameObject alternator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            alternator.transform.SetParent(engine.transform, false);
            alternator.transform.localPosition = new Vector3(0.42f, 0.05f, 0.4f);
            alternator.transform.localScale = new Vector3(0.18f, 0.22f, 0.18f);
            alternator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            alternator.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(alternator.GetComponent<Collider>());

            // Transmission Gearbox housing under cockpit
            GameObject gearbox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gearbox.transform.SetParent(engine.transform, false);
            gearbox.transform.localPosition = new Vector3(0f, -0.05f, -0.75f);
            gearbox.transform.localScale = new Vector3(0.68f, 0.65f, 0.95f);
            gearbox.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(gearbox.GetComponent<Collider>());

            return engine;
        }

        private static GameObject CreateDriverStation(Material metalMat, Material paintMat, Material rubberMat, out Transform steeringTf)
        {
            GameObject station = new GameObject("DriverStation");

            // Diamond-plate footboard platform
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.transform.SetParent(station.transform, false);
            platform.transform.localPosition = new Vector3(0f, 0f, 0f);
            platform.transform.localScale = new Vector3(1.65f, 0.08f, 1.35f);
            platform.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(platform.GetComponent<Collider>());

            // Steering Column Pedestal Console
            GameObject console = GameObject.CreatePrimitive(PrimitiveType.Cube);
            console.transform.SetParent(station.transform, false);
            console.transform.localPosition = new Vector3(0f, 0.48f, 0.45f);
            console.transform.localScale = new Vector3(0.48f, 0.85f, 0.35f);
            console.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
            console.GetComponent<Renderer>().sharedMaterial = paintMat;
            Object.DestroyImmediate(console.GetComponent<Collider>());

            // 3-Spoke Dished Steering Wheel
            GameObject steerObj = new GameObject("TractorSteeringWheel");
            steerObj.transform.SetParent(console.transform, false);
            steerObj.transform.localPosition = new Vector3(0f, 0.52f, 0.05f);
            steerObj.transform.localRotation = Quaternion.Euler(32f, 0f, 0f);
            steeringTf = steerObj.transform;

            GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.transform.SetParent(steerObj.transform, false);
            rim.transform.localScale = new Vector3(0.46f, 0.025f, 0.46f);
            rim.GetComponent<Renderer>().sharedMaterial = rubberMat;
            Object.DestroyImmediate(rim.GetComponent<Collider>());

            // Center horn boss
            GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            boss.transform.SetParent(steerObj.transform, false);
            boss.transform.localScale = new Vector3(0.14f, 0.045f, 0.14f);
            boss.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(boss.GetComponent<Collider>());

            // Authentic Sprung Pan Tractor Seat
            GameObject seat = new GameObject("TractorSeatAssembly");
            seat.transform.SetParent(station.transform, false);
            seat.transform.localPosition = new Vector3(0f, 0.38f, -0.32f);

            // Seat suspension spring arm
            GameObject springArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            springArm.transform.SetParent(seat.transform, false);
            springArm.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            springArm.transform.localScale = new Vector3(0.12f, 0.3f, 0.28f);
            springArm.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            springArm.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(springArm.GetComponent<Collider>());

            // Ergonomic curved tractor pan cushion
            GameObject cushion = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cushion.transform.SetParent(seat.transform, false);
            cushion.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            cushion.transform.localScale = new Vector3(0.55f, 0.08f, 0.48f);
            cushion.GetComponent<Renderer>().sharedMaterial = rubberMat;
            Object.DestroyImmediate(cushion.GetComponent<Collider>());

            // Backrest
            GameObject backrest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backrest.transform.SetParent(seat.transform, false);
            backrest.transform.localPosition = new Vector3(0f, 0.26f, -0.22f);
            backrest.transform.localScale = new Vector3(0.48f, 0.32f, 0.08f);
            backrest.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
            backrest.GetComponent<Renderer>().sharedMaterial = rubberMat;
            Object.DestroyImmediate(backrest.GetComponent<Collider>());

            // Pedals & Gear Levers
            GameObject gearLever = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gearLever.transform.SetParent(station.transform, false);
            gearLever.transform.localPosition = new Vector3(0.08f, 0.25f, 0.15f);
            gearLever.transform.localScale = new Vector3(0.025f, 0.25f, 0.025f);
            gearLever.transform.localRotation = Quaternion.Euler(15f, 10f, 0f);
            gearLever.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(gearLever.GetComponent<Collider>());

            return station;
        }

        private static GameObject CreateRearMudguards(Material paintMat, Material metalMat)
        {
            GameObject mudguards = new GameObject("CurvedMudguards");

            // Left Fender
            GameObject leftFender = CreateCurvedFenderMesh(paintMat, true);
            leftFender.transform.SetParent(mudguards.transform, false);
            leftFender.transform.localPosition = new Vector3(-0.95f, 0f, 0f);

            // Right Fender
            GameObject rightFender = CreateCurvedFenderMesh(paintMat, false);
            rightFender.transform.SetParent(mudguards.transform, false);
            rightFender.transform.localPosition = new Vector3(0.95f, 0f, 0f);

            return mudguards;
        }

        private static GameObject CreateCurvedFenderMesh(Material mat, bool isLeft)
        {
            GameObject fender = new GameObject(isLeft ? "Fender_L" : "Fender_R");
            MeshFilter mf = fender.AddComponent<MeshFilter>();
            MeshRenderer mr = fender.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;

            Mesh m = new Mesh();
            m.name = "CurvedFenderLoft";

            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int arcSegments = 16;
            float radius = 0.88f;
            float width = 0.44f;
            float sideX = isLeft ? -width * 0.5f : width * 0.5f;

            // Arc from rear to top to front
            for (int i = 0; i <= arcSegments; i++)
            {
                float angle = Mathf.Lerp(-Mathf.PI * 0.45f, Mathf.PI * 0.45f, (float)i / arcSegments);
                float y = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                // Inner rim & Outer rolled edge
                verts.Add(new Vector3(sideX - width * 0.5f, y, z));
                verts.Add(new Vector3(sideX + width * 0.5f, y, z));

                float v = (float)i / arcSegments;
                uvs.Add(new Vector2(0f, v));
                uvs.Add(new Vector2(1f, v));

                if (i < arcSegments)
                {
                    int b = i * 2;
                    tris.Add(b); tris.Add(b + 1); tris.Add(b + 2);
                    tris.Add(b + 1); tris.Add(b + 3); tris.Add(b + 2);
                }
            }

            m.SetVertices(verts);
            m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            mf.sharedMesh = m;

            // Passenger safety grab rail on top of fender
            GameObject grabRail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            grabRail.transform.SetParent(fender.transform, false);
            grabRail.transform.localPosition = new Vector3(sideX, radius + 0.08f, 0f);
            grabRail.transform.localScale = new Vector3(0.035f, 0.4f, 0.035f);
            grabRail.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            grabRail.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(grabRail.GetComponent<Collider>());

            return fender;
        }

        private static GameObject CreateExhaustChimney(Material metalMat)
        {
            GameObject chimney = new GameObject("VerticalExhaustChimney");

            // Curving manifold base pipe
            GameObject pipeLower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipeLower.transform.SetParent(chimney.transform, false);
            pipeLower.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            pipeLower.transform.localScale = new Vector3(0.09f, 0.38f, 0.09f);
            pipeLower.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(pipeLower.GetComponent<Collider>());

            // Expansion chamber muffler
            GameObject muffler = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            muffler.transform.SetParent(chimney.transform, false);
            muffler.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            muffler.transform.localScale = new Vector3(0.18f, 0.32f, 0.18f);
            muffler.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(muffler.GetComponent<Collider>());

            // Upper outlet pipe
            GameObject pipeUpper = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipeUpper.transform.SetParent(chimney.transform, false);
            pipeUpper.transform.localPosition = new Vector3(0f, 1.45f, 0f);
            pipeUpper.transform.localScale = new Vector3(0.08f, 0.28f, 0.08f);
            pipeUpper.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(pipeUpper.GetComponent<Collider>());

            // Rain Cap / Counterbalanced Flapper (Iconic Indian tractor flap)
            GameObject flapper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            flapper.name = "RainCapFlapper";
            flapper.transform.SetParent(chimney.transform, false);
            flapper.transform.localPosition = new Vector3(0.02f, 1.74f, 0f);
            flapper.transform.localScale = new Vector3(0.14f, 0.02f, 0.14f);
            flapper.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            flapper.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(flapper.GetComponent<Collider>());

            return chimney;
        }

        private static GameObject CreateThreePointHitch(Material metalMat)
        {
            GameObject hitch = new GameObject("ThreePointHitchAssembly");

            // Top Link Turnbuckle
            GameObject topLink = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            topLink.transform.SetParent(hitch.transform, false);
            topLink.transform.localPosition = new Vector3(0f, 0.45f, -0.2f);
            topLink.transform.localScale = new Vector3(0.04f, 0.32f, 0.04f);
            topLink.transform.localRotation = Quaternion.Euler(75f, 0f, 0f);
            topLink.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(topLink.GetComponent<Collider>());

            // Lower Draft Arms
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject draftArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                draftArm.transform.SetParent(hitch.transform, false);
                draftArm.transform.localPosition = new Vector3(side * 0.35f, 0.12f, -0.25f);
                draftArm.transform.localScale = new Vector3(0.05f, 0.06f, 0.65f);
                draftArm.transform.localRotation = Quaternion.Euler(-10f, side * 5f, 0f);
                draftArm.GetComponent<Renderer>().sharedMaterial = metalMat;
                Object.DestroyImmediate(draftArm.GetComponent<Collider>());
            }

            // PTO (Power Take-Off) shaft shield
            GameObject pto = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pto.transform.SetParent(hitch.transform, false);
            pto.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            pto.transform.localScale = new Vector3(0.12f, 0.14f, 0.12f);
            pto.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            pto.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(pto.GetComponent<Collider>());

            // Towing Clevis Hitch
            GameObject clevis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clevis.transform.SetParent(hitch.transform, false);
            clevis.transform.localPosition = new Vector3(0f, -0.05f, -0.4f);
            clevis.transform.localScale = new Vector3(0.12f, 0.1f, 0.35f);
            clevis.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(clevis.GetComponent<Collider>());

            return hitch;
        }

        private static GameObject CreateFrontAxleAndBumper(Material metalMat)
        {
            GameObject front = new GameObject("FrontAxleBumperAssembly");

            // Heavy Cast Iron Counterweight Bumper Block
            GameObject weight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            weight.name = "FrontCounterweight";
            weight.transform.SetParent(front.transform, false);
            weight.transform.localPosition = new Vector3(0f, 0.32f, 0.15f);
            weight.transform.localScale = new Vector3(1.15f, 0.35f, 0.38f);
            weight.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(weight.GetComponent<Collider>());

            // Transverse Cast Axle Beam
            GameObject axle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            axle.transform.SetParent(front.transform, false);
            axle.transform.localPosition = new Vector3(0f, 0.1f, -0.35f);
            axle.transform.localScale = new Vector3(1.5f, 0.14f, 0.14f);
            axle.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(axle.GetComponent<Collider>());

            return front;
        }

        public static GameObject CreateAgTire(bool isRear, Material rubberMat, Material rimMat)
        {
            GameObject wheelRoot = new GameObject(isRear ? "AgTire_Rear" : "AgTire_Front");

            float tireRadius = isRear ? 0.76f : 0.44f;
            float tireWidth = isRear ? 0.46f : 0.26f;
            int circumferenceSegments = 24;

            // 1. Toroidal Tire Carcass
            GameObject carcass = new GameObject("TireCarcass");
            carcass.transform.SetParent(wheelRoot.transform, false);
            MeshFilter mf = carcass.AddComponent<MeshFilter>();
            MeshRenderer mr = carcass.AddComponent<MeshRenderer>();
            mr.sharedMaterial = rubberMat;

            Mesh mesh = new Mesh();
            mesh.name = isRear ? "RearTireMesh" : "FrontTireMesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int radialSegments = 10;
            float rimRadius = tireRadius * 0.52f;

            for (int c = 0; c <= circumferenceSegments; c++)
            {
                float theta = (float)c / circumferenceSegments * Mathf.PI * 2f;
                float cosT = Mathf.Cos(theta);
                float sinT = Mathf.Sin(theta);

                for (int r = 0; r <= radialSegments; r++)
                {
                    float phi = (float)r / radialSegments * Mathf.PI;
                    float sinP = Mathf.Sin(phi);
                    float cosP = Mathf.Cos(phi);

                    float rad = Mathf.Lerp(rimRadius, tireRadius, sinP);
                    float x = cosP * (tireWidth * 0.5f);
                    float y = cosT * rad;
                    float z = sinT * rad;

                    verts.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2((float)c / circumferenceSegments * 8f, (float)r / radialSegments));
                }
            }

            for (int c = 0; c < circumferenceSegments; c++)
            {
                for (int r = 0; r < radialSegments; r++)
                {
                    int i0 = c * (radialSegments + 1) + r;
                    int i1 = i0 + 1;
                    int i2 = (c + 1) * (radialSegments + 1) + r;
                    int i3 = i2 + 1;

                    tris.Add(i0); tris.Add(i1); tris.Add(i2);
                    tris.Add(i1); tris.Add(i3); tris.Add(i2);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mf.sharedMesh = mesh;

            // 2. Pronounced Chevron V-Tread Lugs for Rear Tires!
            if (isRear)
            {
                int lugCount = 18;
                for (int i = 0; i < lugCount; i++)
                {
                    float angle = (float)i / lugCount * 360f;
                    Quaternion rot = Quaternion.Euler(angle, 0f, 0f);

                    // Left Chevron Bar (angled 45°)
                    GameObject lugL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    lugL.transform.SetParent(carcass.transform, false);
                    lugL.transform.localPosition = rot * new Vector3(-tireWidth * 0.22f, tireRadius + 0.035f, 0f);
                    lugL.transform.localRotation = rot * Quaternion.Euler(0f, 45f, 0f);
                    lugL.transform.localScale = new Vector3(0.045f, 0.06f, tireWidth * 0.55f);
                    lugL.GetComponent<Renderer>().sharedMaterial = rubberMat;
                    Object.DestroyImmediate(lugL.GetComponent<Collider>());

                    // Right Chevron Bar (angled -45°)
                    GameObject lugR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    lugR.transform.SetParent(carcass.transform, false);
                    lugR.transform.localPosition = rot * new Vector3(tireWidth * 0.22f, tireRadius + 0.035f, 0.04f);
                    lugR.transform.localRotation = rot * Quaternion.Euler(0f, -45f, 0f);
                    lugR.transform.localScale = new Vector3(0.045f, 0.06f, tireWidth * 0.55f);
                    lugR.GetComponent<Renderer>().sharedMaterial = rubberMat;
                    Object.DestroyImmediate(lugR.GetComponent<Collider>());
                }
            }
            else
            {
                // Front 3-Rib Steer Tire Lugs
                for (int rib = -1; rib <= 1; rib++)
                {
                    GameObject ribObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    ribObj.transform.SetParent(carcass.transform, false);
                    ribObj.transform.localPosition = new Vector3(rib * 0.07f, 0f, 0f);
                    ribObj.transform.localScale = new Vector3(tireRadius * 2.05f, 0.025f, tireRadius * 2.05f);
                    ribObj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    ribObj.GetComponent<Renderer>().sharedMaterial = rubberMat;
                    Object.DestroyImmediate(ribObj.GetComponent<Collider>());
                }
            }

            // 3. Stamped Steel Rim Dish & Wheel Hub
            GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "SteelRim";
            rim.transform.SetParent(wheelRoot.transform, false);
            rim.transform.localScale = new Vector3(rimRadius * 2f, tireWidth * 0.35f, rimRadius * 2f);
            rim.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            rim.GetComponent<Renderer>().sharedMaterial = rimMat;
            Object.DestroyImmediate(rim.GetComponent<Collider>());

            // Axle Hub Spindle & Bolts
            GameObject hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = "AxleHub";
            hub.transform.SetParent(wheelRoot.transform, false);
            hub.transform.localScale = new Vector3(rimRadius * 0.65f, tireWidth * 0.55f, rimRadius * 0.65f);
            hub.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            hub.GetComponent<Renderer>().sharedMaterial = rimMat;
            Object.DestroyImmediate(hub.GetComponent<Collider>());

            return wheelRoot;
        }

        private static Light CreateTractorHeadlight(string name, Transform parent, Vector3 localPos, Material glowMat, Material glassMat)
        {
            GameObject hl = new GameObject(name);
            hl.transform.SetParent(parent, false);
            hl.transform.localPosition = localPos;

            // Chrome Bucket Housing
            GameObject bucket = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bucket.name = "ChromeHousing";
            bucket.transform.SetParent(hl.transform, false);
            bucket.transform.localScale = new Vector3(0.24f, 0.24f, 0.18f);
            bucket.GetComponent<Renderer>().sharedMaterial = glowMat;
            Object.DestroyImmediate(bucket.GetComponent<Collider>());

            // Glass Lens
            GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lens.name = "GlassLens";
            lens.transform.SetParent(hl.transform, false);
            lens.transform.localPosition = new Vector3(0f, 0f, 0.08f);
            lens.transform.localScale = new Vector3(0.22f, 0.015f, 0.22f);
            lens.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            lens.GetComponent<Renderer>().sharedMaterial = glassMat;
            Object.DestroyImmediate(lens.GetComponent<Collider>());

            // Spot Light
            Light lt = hl.AddComponent<Light>();
            lt.type = LightType.Spot;
            lt.range = 48f;
            lt.spotAngle = 60f;
            lt.innerSpotAngle = 38f;
            lt.intensity = 3.6f;
            lt.color = new Color(1.0f, 0.93f, 0.78f); // Warm Halogen tint
            lt.shadows = LightShadows.Hard;

            // Forward Volumetric-Style Mist Glow Cone
            CreateVolumetricCone(hl.transform);

            return lt;
        }

        private static void CreateVolumetricCone(Transform lightTf)
        {
            GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cone.name = "LightMistCone";
            cone.transform.SetParent(lightTf, false);
            cone.transform.localPosition = new Vector3(0f, 0f, 10f);
            cone.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            cone.transform.localScale = new Vector3(6f, 10f, 6f);

            // Translucent soft additive shader
            Shader s = Shader.Find("Universal Render Pipeline/Unlit");
            if (s == null) s = Shader.Find("Mobile/Particles/Additive");
            Material coneMat = new Material(s);
            Color softGlow = new Color(1f, 0.95f, 0.8f, 0.045f);
            coneMat.color = softGlow;
            if (coneMat.HasProperty("_BaseColor")) coneMat.SetColor("_BaseColor", softGlow);

            cone.GetComponent<Renderer>().sharedMaterial = coneMat;
            Object.DestroyImmediate(cone.GetComponent<Collider>());
        }

        private static Light CreateTractorTaillight(string name, Transform parent, Vector3 localPos, Material redGlowMat)
        {
            GameObject tl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tl.name = name;
            tl.transform.SetParent(parent, false);
            tl.transform.localPosition = localPos;
            tl.transform.localScale = new Vector3(0.12f, 0.04f, 0.12f);
            tl.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            tl.GetComponent<Renderer>().sharedMaterial = redGlowMat;
            Object.DestroyImmediate(tl.GetComponent<Collider>());

            Light lt = tl.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.range = 5.5f;
            lt.intensity = 1.2f;
            lt.color = new Color(0.95f, 0.05f, 0.05f);

            return lt;
        }
    }
}
