#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TractorRacing;

namespace TractorRacing.Editor
{
    [InitializeOnLoad]
    public static class TractorRacingBuilder
    {
        static TractorRacingBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists("Assets/Materials/Mat_Tractor_PBR.mat") || !File.Exists("Assets/Prefabs/Tractor_Racing.prefab"))
                {
                    BuildAll();
                }
            };
        }

        [MenuItem("Tractor Racing/Build All Scenes and Prefabs", false, 1)]
        public static void BuildAll()
        {
            EnsureDirectories();
            CreateSavedMaterials();

            GameObject tractorPrefab = BuildTractorPrefab();
            GameObject checkpointPrefab = BuildCheckpointPrefab();
            GameObject finishLinePrefab = BuildFinishLinePrefab();

            BuildMainMenuScene();
            BuildLevelSelectScene();

            BuildLevelScene("Level1", 1, "Village Road (Pehla Kadam)", 360f, 3, tractorPrefab, checkpointPrefab, finishLinePrefab);
            BuildLevelScene("Level2", 2, "Mud Track (Sunsaan Khet)", 420f, 4, tractorPrefab, checkpointPrefab, finishLinePrefab);
            BuildLevelScene("Level3", 3, "Mountain Road (Ujda Gaon)", 480f, 4, tractorPrefab, checkpointPrefab, finishLinePrefab);
            BuildLevelScene("Level4", 4, "Obstacle Track (Ghanghor Jungle)", 540f, 5, tractorPrefab, checkpointPrefab, finishLinePrefab);
            BuildLevelScene("Level5", 5, "Tractor Challenge (Bhootiya Rasta)", 620f, 5, tractorPrefab, checkpointPrefab, finishLinePrefab);

            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Open MainMenu scene so pressing Play starts directly on Main Menu
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

            Debug.Log("<color=#00FF00><b>[Tractor Racing]</b> Successfully generated all Scenes with Obstacles, Off-Road Warnings, Checkpoints, and Victory Screens!</color>");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists("Assets/Prefabs")) Directory.CreateDirectory("Assets/Prefabs");
            if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
            if (!Directory.Exists("Assets/Materials")) Directory.CreateDirectory("Assets/Materials");
        }

        private static Shader GetSafeShader()
        {
            return Shader.Find("Universal Render Pipeline/Lit") ??
                   Shader.Find("URP/Lit") ??
                   Shader.Find("Universal Render Pipeline/Simple Lit") ??
                   Shader.Find("Standard") ??
                   Shader.Find("Mobile/Diffuse");
        }

        public static void CreateSavedMaterials()
        {
            Shader litShader = GetSafeShader();

            // 1. Tractor PBR Material with 4K Textures
            Texture2D diffTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Models/Tractor/tractor_texture_0.jpg");
            Texture2D normTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Models/Tractor/tractor_texture_2.png");
            Texture2D metalTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Models/Tractor/tractor_texture_1.png");

            Material tractorMat = new Material(litShader) { name = "Mat_Tractor_PBR" };
            if (diffTex != null)
            {
                tractorMat.mainTexture = diffTex;
                if (tractorMat.HasProperty("_BaseMap")) tractorMat.SetTexture("_BaseMap", diffTex);
            }
            if (normTex != null && tractorMat.HasProperty("_BumpMap"))
            {
                tractorMat.EnableKeyword("_NORMALMAP");
                tractorMat.SetTexture("_BumpMap", normTex);
            }
            if (metalTex != null && tractorMat.HasProperty("_MetallicGlossMap"))
            {
                tractorMat.EnableKeyword("_METALLICSPECGLOSSMAP");
                tractorMat.SetTexture("_MetallicGlossMap", metalTex);
            }
            if (tractorMat.HasProperty("_Smoothness")) tractorMat.SetFloat("_Smoothness", 0.55f);
            if (tractorMat.HasProperty("_Metallic")) tractorMat.SetFloat("_Metallic", 0.50f);
            SaveOrUpdateMaterial(tractorMat, "Assets/Materials/Mat_Tractor_PBR.mat");

            // 2. Road Dirt Material
            Material roadMat = new Material(litShader) { name = "Mat_Road_Dirt", color = new Color(0.38f, 0.28f, 0.18f) };
            if (roadMat.HasProperty("_BaseColor")) roadMat.SetColor("_BaseColor", new Color(0.38f, 0.28f, 0.18f));
            if (roadMat.HasProperty("_Smoothness")) roadMat.SetFloat("_Smoothness", 0.15f);
            SaveOrUpdateMaterial(roadMat, "Assets/Materials/Mat_Road_Dirt.mat");

            // 3. Farmland Grass Material
            Material farmMat = new Material(litShader) { name = "Mat_Farmland_Grass", color = new Color(0.22f, 0.38f, 0.14f) };
            if (farmMat.HasProperty("_BaseColor")) farmMat.SetColor("_BaseColor", new Color(0.22f, 0.38f, 0.14f));
            if (farmMat.HasProperty("_Smoothness")) farmMat.SetFloat("_Smoothness", 0.10f);
            SaveOrUpdateMaterial(farmMat, "Assets/Materials/Mat_Farmland_Grass.mat");

            // 4. Wood Material (Fences & Tree Trunks)
            Material woodMat = new Material(litShader) { name = "Mat_Rural_Wood", color = new Color(0.44f, 0.29f, 0.16f) };
            if (woodMat.HasProperty("_BaseColor")) woodMat.SetColor("_BaseColor", new Color(0.44f, 0.29f, 0.16f));
            SaveOrUpdateMaterial(woodMat, "Assets/Materials/Mat_Rural_Wood.mat");

            // 5. Foliage Material (Trees)
            Material foliageMat = new Material(litShader) { name = "Mat_Rural_Foliage", color = new Color(0.15f, 0.42f, 0.12f) };
            if (foliageMat.HasProperty("_BaseColor")) foliageMat.SetColor("_BaseColor", new Color(0.15f, 0.42f, 0.12f));
            SaveOrUpdateMaterial(foliageMat, "Assets/Materials/Mat_Rural_Foliage.mat");

            // 6. Checkpoint Post Material
            Material cpMat = new Material(litShader) { name = "Mat_Checkpoint_Post", color = new Color(1.0f, 0.62f, 0.1f) };
            if (cpMat.HasProperty("_BaseColor")) cpMat.SetColor("_BaseColor", new Color(1.0f, 0.62f, 0.1f));
            if (cpMat.HasProperty("_EmissionColor"))
            {
                cpMat.EnableKeyword("_EMISSION");
                cpMat.SetColor("_EmissionColor", new Color(0.8f, 0.45f, 0.05f));
            }
            SaveOrUpdateMaterial(cpMat, "Assets/Materials/Mat_Checkpoint_Post.mat");

            // 7. Finish Line Material
            Material finishMat = new Material(litShader) { name = "Mat_Finish_Banner", color = new Color(0.1f, 0.85f, 0.25f) };
            if (finishMat.HasProperty("_BaseColor")) finishMat.SetColor("_BaseColor", new Color(0.1f, 0.85f, 0.25f));
            if (finishMat.HasProperty("_EmissionColor"))
            {
                finishMat.EnableKeyword("_EMISSION");
                finishMat.SetColor("_EmissionColor", new Color(0.05f, 0.6f, 0.15f));
            }
            SaveOrUpdateMaterial(finishMat, "Assets/Materials/Mat_Finish_Banner.mat");

            // 8. Wet Mud Patch Material
            Material mudMat = new Material(litShader) { name = "Mat_Mud_Puddle", color = new Color(0.25f, 0.17f, 0.10f) };
            if (mudMat.HasProperty("_BaseColor")) mudMat.SetColor("_BaseColor", new Color(0.25f, 0.17f, 0.10f));
            if (mudMat.HasProperty("_Smoothness")) mudMat.SetFloat("_Smoothness", 0.75f); // Wet look
            SaveOrUpdateMaterial(mudMat, "Assets/Materials/Mat_Mud_Puddle.mat");
        }

        private static Material SaveOrUpdateMaterial(Material mat, string path)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                existing.shader = mat.shader;
                existing.CopyPropertiesFromMaterial(mat);
                EditorUtility.SetDirty(existing);
                return existing;
            }
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        public static GameObject BuildTractorPrefab()
        {
            Material tractorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Tractor_PBR.mat");

            GameObject tractorRoot = new GameObject("Tractor_Racing");
            tractorRoot.tag = "Player";

            // Physics Material for ultra-smooth rolling without ground catching
            PhysicsMaterial physMat = new PhysicsMaterial("PhysMat_TractorWheel")
            {
                dynamicFriction = 0.05f,
                staticFriction = 0.05f,
                bounciness = 0.0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };

            // 1. Rigidbody
            Rigidbody rb = tractorRoot.AddComponent<Rigidbody>();
            rb.mass = 2200f;
            rb.linearDamping = 0.25f;
            rb.angularDamping = 3.5f;
            rb.centerOfMass = new Vector3(0f, -0.35f, 0.05f);
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // 2. Tractor Physics Controller & Track Boundary System
            TractorPhysics tp = tractorRoot.AddComponent<TractorPhysics>();
            tp.mass = 2200f;
            tp.maxForwardSpeed = 18f;
            tp.maxReverseSpeed = 10f;
            tp.acceleration = 35f;
            tp.brakeForce = 45f;
            tp.turnSpeed = 65f;

            tractorRoot.AddComponent<TrackBoundarySystem>();

            // 3. Chassis Box Collider (Safely elevated so bottom never scrapes ground)
            BoxCollider col = tractorRoot.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.30f, 0.05f);
            col.size = new Vector3(1.30f, 0.80f, 2.60f);
            col.sharedMaterial = physMat;

            float visualScale = 0.48f;

            // 4. Visual Container & Chassis Mesh
            GameObject visualContainer = new GameObject("VisualModel");
            visualContainer.transform.SetParent(tractorRoot.transform, false);

            GameObject chassisMeshObj = LoadModelMesh("Assets/Models/Tractor/IndianTractor_Chassis.obj", "TractorChassisMesh");
            if (chassisMeshObj != null)
            {
                chassisMeshObj.transform.SetParent(visualContainer.transform, false);
                chassisMeshObj.transform.localScale = Vector3.one * visualScale;
                ApplyMaterialToRenderers(chassisMeshObj, tractorMat);
            }

            // Steering Wheel Mesh
            GameObject steerObj = LoadModelMesh("Assets/Models/Tractor/IndianTractor_SteeringWheel.obj", "SteeringWheel_Mesh");
            if (steerObj != null)
            {
                steerObj.transform.SetParent(visualContainer.transform, false);
                steerObj.transform.localPosition = new Vector3(0.0f, 1.64f, -0.09f);
                steerObj.transform.localScale = Vector3.one * visualScale;
                ApplyMaterialToRenderers(steerObj, tractorMat);
                tp.steeringWheel = steerObj.transform;
            }

            // 5. Dual Halogen Headlights (Warm Yellow Glow)
            CreateHeadlight(visualContainer.transform, new Vector3(-0.48f, 1.28f, 1.62f), "Headlight_L");
            CreateHeadlight(visualContainer.transform, new Vector3( 0.48f, 1.28f, 1.62f), "Headlight_R");

            // Red Taillights
            CreateTaillight(visualContainer.transform, new Vector3(-0.65f, 1.10f, -1.65f), "Taillight_L");
            CreateTaillight(visualContainer.transform, new Vector3( 0.65f, 1.10f, -1.65f), "Taillight_R");

            // 6. 4 Wheels Setup with Sphere Colliders
            Vector3 flHub = new Vector3(-0.615f, 0.740f, 0.879f);
            Vector3 frHub = new Vector3( 0.615f, 0.740f, 0.879f);
            Vector3 rlHub = new Vector3(-0.646f, 0.740f, -0.879f);
            Vector3 rrHub = new Vector3( 0.646f, 0.740f, -0.879f);

            Transform flWheel = CreateWheelWithMeshAndCollider(tractorRoot.transform, "Wheel_FrontLeft", flHub, 0.58f, visualScale, "Assets/Models/Tractor/IndianTractor_Wheel_FL.obj", tractorMat, physMat);
            Transform frWheel = CreateWheelWithMeshAndCollider(tractorRoot.transform, "Wheel_FrontRight", frHub, 0.58f, visualScale, "Assets/Models/Tractor/IndianTractor_Wheel_FR.obj", tractorMat, physMat);
            Transform rlWheel = CreateWheelWithMeshAndCollider(tractorRoot.transform, "Wheel_RearLeft", rlHub, 0.72f, visualScale, "Assets/Models/Tractor/IndianTractor_Wheel_RL.obj", tractorMat, physMat);
            Transform rrWheel = CreateWheelWithMeshAndCollider(tractorRoot.transform, "Wheel_RearRight", rrHub, 0.72f, visualScale, "Assets/Models/Tractor/IndianTractor_Wheel_RR.obj", tractorMat, physMat);

            tp.frontLeftWheel = flWheel;
            tp.frontRightWheel = frWheel;
            tp.rearLeftWheel = rlWheel;
            tp.rearRightWheel = rrWheel;

            string prefabPath = "Assets/Prefabs/Tractor_Racing.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tractorRoot, prefabPath);
            Object.DestroyImmediate(tractorRoot);
            return prefab;
        }

        private static Transform CreateWheelWithMeshAndCollider(Transform parent, string name, Vector3 localPos, float radius, float scale, string meshPath, Material mat, PhysicsMaterial physMat)
        {
            GameObject wheelObj = new GameObject(name);
            wheelObj.transform.SetParent(parent, false);
            wheelObj.transform.localPosition = localPos;
            wheelObj.tag = "Player";

            SphereCollider sc = wheelObj.AddComponent<SphereCollider>();
            sc.radius = radius;
            sc.center = Vector3.zero;
            sc.sharedMaterial = physMat;

            GameObject meshChild = LoadModelMesh(meshPath, name + "_Mesh");
            if (meshChild != null)
            {
                meshChild.transform.SetParent(wheelObj.transform, false);
                meshChild.transform.localPosition = Vector3.zero;
                meshChild.transform.localScale = Vector3.one * scale;
                ApplyMaterialToRenderers(meshChild, mat);
            }

            return wheelObj.transform;
        }

        private static void CreateHeadlight(Transform parent, Vector3 localPos, string name)
        {
            GameObject hl = new GameObject(name);
            hl.transform.SetParent(parent, false);
            hl.transform.localPosition = localPos;
            hl.transform.localRotation = Quaternion.Euler(4f, 0f, 0f);
            Light lt = hl.AddComponent<Light>();
            lt.type = LightType.Spot;
            lt.range = 80f;
            lt.spotAngle = 60f;
            lt.intensity = 4.5f;
            lt.color = new Color(1.0f, 0.94f, 0.80f);
            lt.shadows = LightShadows.Soft;
        }

        private static void CreateTaillight(Transform parent, Vector3 localPos, string name)
        {
            GameObject tl = new GameObject(name);
            tl.transform.SetParent(parent, false);
            tl.transform.localPosition = localPos;
            Light lt = tl.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.range = 5.0f;
            lt.intensity = 2.0f;
            lt.color = new Color(1.0f, 0.08f, 0.08f);
        }

        private static GameObject LoadModelMesh(string relativePath, string gameObjectName)
        {
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
            if (modelPrefab != null)
            {
                GameObject inst = Object.Instantiate(modelPrefab);
                inst.name = gameObjectName;
                return inst;
            }
            Debug.LogWarning($"[TractorRacingBuilder] Could not load model at {relativePath}");
            return null;
        }

        private static void ApplyMaterialToRenderers(GameObject root, Material mat)
        {
            if (mat == null) return;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                r.receiveShadows = true;
                int count = r.sharedMaterials.Length > 0 ? r.sharedMaterials.Length : 1;
                Material[] mats = new Material[count];
                for (int i = 0; i < count; i++) mats[i] = mat;
                r.sharedMaterials = mats;
            }
        }

        public static GameObject BuildCheckpointPrefab()
        {
            Material cpMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Checkpoint_Post.mat");

            GameObject cp = new GameObject("Checkpoint_Racing");
            BoxCollider bc = cp.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(14f, 6f, 3f);
            bc.center = new Vector3(0f, 3f, 0f);

            Checkpoint cComp = cp.AddComponent<Checkpoint>();
            cComp.triggerCollider = bc;

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "Post_" + (side == -1 ? "L" : "R");
                post.transform.SetParent(cp.transform, false);
                post.transform.localPosition = new Vector3(side * 5.8f, 3f, 0f);
                post.transform.localScale = new Vector3(0.45f, 3f, 0.45f);
                Object.DestroyImmediate(post.GetComponent<Collider>());
                post.GetComponent<Renderer>().sharedMaterial = cpMat;
            }

            string prefabPath = "Assets/Prefabs/Checkpoint_Racing.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cp, prefabPath);
            Object.DestroyImmediate(cp);
            return prefab;
        }

        public static GameObject BuildFinishLinePrefab()
        {
            Material finishMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Finish_Banner.mat");
            Material cpMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Checkpoint_Post.mat");

            GameObject fl = new GameObject("FinishLine_Racing");
            BoxCollider bc = fl.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(14f, 6f, 4f);
            bc.center = new Vector3(0f, 3f, 0f);

            fl.AddComponent<FinishLine>();

            // Left and Right Posts
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "FinishPost_" + (side == -1 ? "L" : "R");
                post.transform.SetParent(fl.transform, false);
                post.transform.localPosition = new Vector3(side * 5.8f, 3f, 0f);
                post.transform.localScale = new Vector3(0.5f, 3f, 0.5f);
                Object.DestroyImmediate(post.GetComponent<Collider>());
                post.GetComponent<Renderer>().sharedMaterial = cpMat;
            }

            GameObject archBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            archBeam.name = "FinishBanner";
            archBeam.transform.SetParent(fl.transform, false);
            archBeam.transform.localPosition = new Vector3(0f, 5.5f, 0f);
            archBeam.transform.localScale = new Vector3(14f, 0.9f, 0.6f);
            Object.DestroyImmediate(archBeam.GetComponent<Collider>());
            archBeam.GetComponent<Renderer>().sharedMaterial = finishMat;

            string prefabPath = "Assets/Prefabs/FinishLine_Racing.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(fl, prefabPath);
            Object.DestroyImmediate(fl);
            return prefab;
        }

        private static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCameraAndLight(new Vector3(0f, 2f, -8f), Quaternion.Euler(10f, 0f, 0f));
            CreateEventSystem();

            GameObject canvasObj = CreateCanvas("MainMenuCanvas");
            MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();

            CreateText(canvasObj.transform, "TitleText", "TRACTOR RACING", new Vector2(0f, 160f), 54, Color.yellow, TextAnchor.MiddleCenter);
            CreateText(canvasObj.transform, "SubTitleText", "RURAL OFF-ROAD CHAMPIONSHIP", new Vector2(0f, 110f), 22, Color.white, TextAnchor.MiddleCenter);

            Button playBtn = CreateButton(canvasObj.transform, "PlayButton", "START GAME", new Vector2(0f, 10f), new Vector2(240f, 60f));
            Button levelSelectBtn = CreateButton(canvasObj.transform, "LevelSelectButton", "SELECT TRACK", new Vector2(0f, -60f), new Vector2(240f, 50f));
            Button exitBtn = CreateButton(canvasObj.transform, "ExitButton", "EXIT", new Vector2(0f, -125f), new Vector2(240f, 45f));

            SerializedObject so = new SerializedObject(menuUI);
            so.FindProperty("mainMenuCanvas").objectReferenceValue = canvasObj.GetComponent<Canvas>();
            so.FindProperty("playButton").objectReferenceValue = playBtn;
            so.FindProperty("levelSelectButton").objectReferenceValue = levelSelectBtn;
            so.FindProperty("exitButton").objectReferenceValue = exitBtn;
            so.ApplyModifiedProperties();

            GameObject mgrs = new GameObject("CoreManagers");
            mgrs.AddComponent<GameStateManager>();
            mgrs.AddComponent<SaveManager>();
            mgrs.AddComponent<LevelManager>();
            mgrs.AddComponent<TractorRacing.AudioManager>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        }

        private static void BuildLevelSelectScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCameraAndLight(new Vector3(0f, 2f, -8f), Quaternion.Euler(10f, 0f, 0f));
            CreateEventSystem();

            GameObject canvasObj = CreateCanvas("LevelSelectCanvas");
            LevelSelectUI lsUI = canvasObj.AddComponent<LevelSelectUI>();

            CreateText(canvasObj.transform, "Header", "SELECT LEVEL", new Vector2(0f, 180f), 44, Color.yellow, TextAnchor.MiddleCenter);

            Button[] lvlBtns = new Button[5];
            Text[] lvlInfos = new Text[5];

            string[] names = { "Level 1: Village Road", "Level 2: Mud Track", "Level 3: Mountain Road", "Level 4: Obstacle Track", "Level 5: Tractor Challenge" };

            for (int i = 0; i < 5; i++)
            {
                float yPos = 100f - i * 55f;
                lvlBtns[i] = CreateButton(canvasObj.transform, $"Level{i + 1}Button", names[i], new Vector2(-60f, yPos), new Vector2(340f, 45f));
                lvlInfos[i] = CreateText(canvasObj.transform, $"Level{i + 1}Info", "Time: --:--", new Vector2(170f, yPos), 18, Color.white, TextAnchor.MiddleLeft);
            }

            Button backBtn = CreateButton(canvasObj.transform, "BackButton", "BACK TO MAIN MENU", new Vector2(0f, -200f), new Vector2(260f, 45f));

            SerializedObject so = new SerializedObject(lsUI);
            so.FindProperty("levelSelectCanvas").objectReferenceValue = canvasObj.GetComponent<Canvas>();
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.FindProperty("level1Button").objectReferenceValue = lvlBtns[0];
            so.FindProperty("level2Button").objectReferenceValue = lvlBtns[1];
            so.FindProperty("level3Button").objectReferenceValue = lvlBtns[2];
            so.FindProperty("level4Button").objectReferenceValue = lvlBtns[3];
            so.FindProperty("level5Button").objectReferenceValue = lvlBtns[4];
            so.FindProperty("level1Info").objectReferenceValue = lvlInfos[0];
            so.FindProperty("level2Info").objectReferenceValue = lvlInfos[1];
            so.FindProperty("level3Info").objectReferenceValue = lvlInfos[2];
            so.FindProperty("level4Info").objectReferenceValue = lvlInfos[3];
            so.FindProperty("level5Info").objectReferenceValue = lvlInfos[4];
            so.ApplyModifiedProperties();

            GameObject mgrs = new GameObject("CoreManagers");
            mgrs.AddComponent<GameStateManager>();
            mgrs.AddComponent<SaveManager>();
            mgrs.AddComponent<LevelManager>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/LevelSelect.unity");
        }

        private static void BuildLevelScene(string sceneName, int levelIndex, string levelTitle, float trackLength, int cpCount,
            GameObject tractorPrefab, GameObject checkpointPrefab, GameObject finishLinePrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material roadMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Road_Dirt.mat");
            Material farmMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Farmland_Grass.mat");
            Material woodMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Rural_Wood.mat");
            Material foliageMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Rural_Foliage.mat");
            Material mudMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Mud_Puddle.mat");
            Material cpMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Checkpoint_Post.mat");

            // 1. Directional Sun Lighting
            GameObject lightObj = new GameObject("Directional Light");
            Light lt = lightObj.AddComponent<Light>();
            lt.type = LightType.Directional;
            lt.color = new Color(1f, 0.96f, 0.88f);
            lt.intensity = 1.35f;
            lt.shadows = LightShadows.Soft;
            lt.shadowStrength = 0.85f;
            lightObj.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            // 2. Vast Surrounding Countryside Landscape (300m wide terrain)
            GameObject countryTerrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
            countryTerrain.name = "CountrysideTerrain";
            countryTerrain.transform.position = new Vector3(0f, -0.05f, trackLength * 0.5f);
            countryTerrain.transform.localScale = new Vector3(30f, 1f, (trackLength * 0.1f) + 12f);
            countryTerrain.GetComponent<Renderer>().sharedMaterial = farmMat;

            // 3. Dirt Road Track
            GameObject roadTrack = GameObject.CreatePrimitive(PrimitiveType.Plane);
            roadTrack.name = "DirtRoadTrack";
            roadTrack.transform.position = new Vector3(0f, 0.01f, trackLength * 0.5f);
            roadTrack.transform.localScale = new Vector3(1.1f, 1f, trackLength * 0.1f);
            roadTrack.GetComponent<Renderer>().sharedMaterial = roadMat;

            // 4. Roadside Wooden Fences, Milestone Markers, Trees, & Obstacles
            GameObject envContainer = new GameObject("RuralEnvironment");
            GameObject obstacleContainer = new GameObject("TrackObstacles");

            int propSteps = (int)(trackLength / 20f);
            for (int p = 1; p <= propSteps; p++)
            {
                float zPos = p * 20f;

                // Fences on left & right
                for (int side = -1; side <= 1; side += 2)
                {
                    GameObject fencePost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    fencePost.name = $"FencePost_{side}_{p}";
                    fencePost.transform.SetParent(envContainer.transform, false);
                    fencePost.transform.position = new Vector3(side * 5.5f, 0.75f, zPos);
                    fencePost.transform.localScale = new Vector3(0.18f, 0.75f, 0.18f);
                    Object.DestroyImmediate(fencePost.GetComponent<Collider>());
                    fencePost.GetComponent<Renderer>().sharedMaterial = woodMat;
                }

                // Countryside Trees
                if (p % 2 == 0)
                {
                    float tSide = (p % 4 == 0) ? -1f : 1f;
                    GameObject tree = new GameObject($"RuralTree_{p}");
                    tree.transform.SetParent(envContainer.transform, false);
                    tree.transform.position = new Vector3(tSide * (8.5f + (p % 3) * 2f), 0f, zPos + (p % 5));

                    // Trunk
                    GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    trunk.transform.SetParent(tree.transform, false);
                    trunk.transform.localPosition = new Vector3(0f, 2.5f, 0f);
                    trunk.transform.localScale = new Vector3(0.6f, 2.5f, 0.6f);
                    Object.DestroyImmediate(trunk.GetComponent<Collider>());
                    trunk.GetComponent<Renderer>().sharedMaterial = woodMat;

                    // Foliage
                    GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    crown.transform.SetParent(tree.transform, false);
                    crown.transform.localPosition = new Vector3(0f, 5.5f, 0f);
                    crown.transform.localScale = new Vector3(3.8f, 3.2f, 3.8f);
                    Object.DestroyImmediate(crown.GetComponent<Collider>());
                    crown.GetComponent<Renderer>().sharedMaterial = foliageMat;
                }

                // Haystacks in fields
                if (p % 5 == 0)
                {
                    GameObject haystack = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    haystack.name = $"Haystack_{p}";
                    haystack.transform.SetParent(envContainer.transform, false);
                    haystack.transform.position = new Vector3((p % 2 == 0 ? 14f : -14f), 1.2f, zPos);
                    haystack.transform.localScale = new Vector3(3.5f, 2.2f, 3.5f);
                    Object.DestroyImmediate(haystack.GetComponent<Collider>());
                    haystack.GetComponent<Renderer>().sharedMaterial = roadMat;
                }

                // Obstacles on Road: Roadblocks & Mud Pits
                if (p % 3 == 0 && zPos < trackLength - 30f)
                {
                    float obstacleSide = (p % 6 == 0) ? -1.8f : 1.8f;

                    // 1. Wooden Roadblock Barricade
                    GameObject roadblock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    roadblock.name = $"Roadblock_{p}";
                    roadblock.transform.SetParent(obstacleContainer.transform, false);
                    roadblock.transform.position = new Vector3(obstacleSide, 0.6f, zPos);
                    roadblock.transform.localScale = new Vector3(2.6f, 1.2f, 0.45f);
                    roadblock.GetComponent<Renderer>().sharedMaterial = woodMat;

                    TrackObstacle obs = roadblock.AddComponent<TrackObstacle>();
                    obs.obstacleType = ObstacleType.Roadblock;

                    // 2. Mud Patch nearby
                    GameObject mudPatch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    mudPatch.name = $"MudPatch_{p}";
                    mudPatch.transform.SetParent(obstacleContainer.transform, false);
                    mudPatch.transform.position = new Vector3(-obstacleSide, 0.02f, zPos + 8f);
                    mudPatch.transform.localScale = new Vector3(3.5f, 0.05f, 5.5f);
                    mudPatch.GetComponent<Renderer>().sharedMaterial = mudMat ?? roadMat;

                    BoxCollider mc = mudPatch.GetComponent<BoxCollider>();
                    mc.isTrigger = true;

                    TrackObstacle mudObs = mudPatch.AddComponent<TrackObstacle>();
                    mudObs.obstacleType = ObstacleType.MudPatch;
                }
            }

            // 5. Instantiate Imported Tractor
            GameObject tractorInstance = (GameObject)PrefabUtility.InstantiatePrefab(tractorPrefab);
            tractorInstance.name = "Tractor";
            tractorInstance.transform.position = new Vector3(0f, 0.25f, 0f);
            tractorInstance.transform.rotation = Quaternion.identity;

            // 6. Instantiate Checkpoints
            List<Transform> cpTransforms = new List<Transform>();
            float cpStep = trackLength / (cpCount + 1);

            for (int i = 1; i <= cpCount; i++)
            {
                GameObject cpInstance = (GameObject)PrefabUtility.InstantiatePrefab(checkpointPrefab);
                cpInstance.name = $"Checkpoint_{i}";
                cpInstance.transform.position = new Vector3(0f, 0f, i * cpStep);

                Checkpoint cpComp = cpInstance.GetComponent<Checkpoint>();
                if (cpComp != null) cpComp.checkpointIndex = i;

                cpTransforms.Add(cpInstance.transform);
            }

            // 7. Finish Line
            GameObject finishInstance = (GameObject)PrefabUtility.InstantiatePrefab(finishLinePrefab);
            finishInstance.name = "FinishLine";
            finishInstance.transform.position = new Vector3(0f, 0f, trackLength);

            // 8. Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 2.8f, -5.5f);
            camObj.AddComponent<AudioListener>();
            TractorRacingCameraController camCtrl = camObj.AddComponent<TractorRacingCameraController>();
            camCtrl.SetTarget(tractorInstance.transform);

            // 9. In-Game UI HUD
            CreateEventSystem();
            GameObject hudCanvas = CreateCanvas("InGameHUD");

            CreateText(hudCanvas.transform, "LevelText", $"Level {levelIndex}: {levelTitle}", new Vector2(-280f, 190f), 22, Color.yellow, TextAnchor.MiddleLeft);
            CreateText(hudCanvas.transform, "TimerText", "02:00", new Vector2(0f, 190f), 32, Color.white, TextAnchor.MiddleCenter);
            CreateText(hudCanvas.transform, "SpeedText", "Speed: 0 km/h", new Vector2(280f, 190f), 22, Color.cyan, TextAnchor.MiddleRight);
            CreateText(hudCanvas.transform, "CheckpointText", $"Checkpoint: 1/{cpCount}", new Vector2(0f, 150f), 20, Color.white, TextAnchor.MiddleCenter);

            hudCanvas.AddComponent<HUDController>();
            hudCanvas.AddComponent<RaceTimer>();

            // Pause Menu Canvas
            GameObject pauseCanvas = CreateCanvas("PauseMenuCanvas");
            PauseMenu pauseMenu = pauseCanvas.AddComponent<PauseMenu>();
            CreateText(pauseCanvas.transform, "PauseTitle", "PAUSED", new Vector2(0f, 120f), 40, Color.yellow, TextAnchor.MiddleCenter);
            Button resumeBtn = CreateButton(pauseCanvas.transform, "ResumeBtn", "RESUME", new Vector2(0f, 40f), new Vector2(200f, 45f));
            Button restartBtn = CreateButton(pauseCanvas.transform, "RestartBtn", "RESTART", new Vector2(0f, -15f), new Vector2(200f, 45f));
            Button pauseLvlBtn = CreateButton(pauseCanvas.transform, "LevelSelectBtn", "LEVEL SELECT", new Vector2(0f, -70f), new Vector2(200f, 45f));
            Button pauseMenuBtn = CreateButton(pauseCanvas.transform, "MainMenuBtn", "MAIN MENU", new Vector2(0f, -125f), new Vector2(200f, 45f));
            pauseCanvas.SetActive(false);

            SerializedObject soPause = new SerializedObject(pauseMenu);
            soPause.FindProperty("pauseCanvas").objectReferenceValue = pauseCanvas.GetComponent<Canvas>();
            soPause.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
            soPause.FindProperty("restartButton").objectReferenceValue = restartBtn;
            soPause.FindProperty("levelSelectButton").objectReferenceValue = pauseLvlBtn;
            soPause.FindProperty("mainMenuButton").objectReferenceValue = pauseMenuBtn;
            soPause.ApplyModifiedProperties();

            // Level Complete Canvas
            GameObject winCanvas = CreateCanvas("LevelCompleteCanvas");
            LevelCompleteUI winUI = winCanvas.AddComponent<LevelCompleteUI>();
            CreateText(winCanvas.transform, "WinTitle", "LEVEL COMPLETE!", new Vector2(0f, 140f), 42, Color.green, TextAnchor.MiddleCenter);
            Text winLvlName = CreateText(winCanvas.transform, "WinLvlName", levelTitle, new Vector2(0f, 90f), 24, Color.white, TextAnchor.MiddleCenter);
            Text winTime = CreateText(winCanvas.transform, "WinTime", "Time: 00:00", new Vector2(0f, 50f), 22, Color.yellow, TextAnchor.MiddleCenter);
            Text winBest = CreateText(winCanvas.transform, "WinBestTime", "Best: 00:00", new Vector2(0f, 20f), 20, Color.cyan, TextAnchor.MiddleCenter);
            Button nextBtn = CreateButton(winCanvas.transform, "NextBtn", "NEXT LEVEL", new Vector2(0f, -40f), new Vector2(220f, 45f));
            Button winLvlSelectBtn = CreateButton(winCanvas.transform, "WinLvlSelectBtn", "LEVEL SELECT", new Vector2(0f, -95f), new Vector2(220f, 45f));
            Button winMenuBtn = CreateButton(winCanvas.transform, "WinMenuBtn", "MAIN MENU", new Vector2(0f, -150f), new Vector2(220f, 45f));
            winCanvas.SetActive(false);

            SerializedObject soWin = new SerializedObject(winUI);
            soWin.FindProperty("levelCompleteCanvas").objectReferenceValue = winCanvas.GetComponent<Canvas>();
            soWin.FindProperty("levelNameText").objectReferenceValue = winLvlName;
            soWin.FindProperty("timeText").objectReferenceValue = winTime;
            soWin.FindProperty("bestTimeText").objectReferenceValue = winBest;
            soWin.FindProperty("nextLevelButton").objectReferenceValue = nextBtn;
            soWin.FindProperty("levelSelectButton").objectReferenceValue = winLvlSelectBtn;
            soWin.FindProperty("mainMenuButton").objectReferenceValue = winMenuBtn;
            soWin.ApplyModifiedProperties();

            // 10. Managers
            GameObject mgrs = new GameObject("LevelManagers");
            mgrs.AddComponent<GameStateManager>();
            mgrs.AddComponent<SaveManager>();
            mgrs.AddComponent<LevelManager>();
            CheckpointManager cpm = mgrs.AddComponent<CheckpointManager>();
            cpm.tractor = tractorInstance.GetComponent<TractorPhysics>();
            cpm.InitializeCheckpoints(cpTransforms.ToArray());

            RespawnSystem respawn = mgrs.AddComponent<RespawnSystem>();
            respawn.tractor = tractorInstance.GetComponent<TractorPhysics>();

            mgrs.AddComponent<TractorRacing.AudioManager>();
            mgrs.AddComponent<TractorRacing.GameManager>();

            EditorSceneManager.SaveScene(scene, $"Assets/Scenes/{sceneName}.unity");
        }

        private static void CreateSceneCameraAndLight(Vector3 camPos, Quaternion camRot)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            camObj.transform.position = camPos;
            camObj.transform.rotation = camRot;
            camObj.AddComponent<AudioListener>();

            GameObject lightObj = new GameObject("Directional Light");
            Light lt = lightObj.AddComponent<Light>();
            lt.type = LightType.Directional;
            lt.color = Color.white;
            lt.intensity = 1.0f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateEventSystem()
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();

            System.Type inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputModuleType != null)
            {
                esObj.AddComponent(inputModuleType);
            }
            else
            {
                esObj.AddComponent<StandaloneInputModule>();
            }
        }

        private static GameObject CreateCanvas(string name)
        {
            GameObject canvasObj = new GameObject(name);
            Canvas c = canvasObj.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler cs = canvasObj.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920f, 1080f);
            canvasObj.AddComponent<GraphicRaycaster>();
            return canvasObj;
        }

        private static Text CreateText(Transform parent, string name, string text, Vector2 anchoredPos, int fontSize, Color color, TextAnchor anchor)
        {
            GameObject txtObj = new GameObject(name);
            txtObj.transform.SetParent(parent, false);
            RectTransform rt = txtObj.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(400f, 60f);

            Text t = txtObj.AddComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = anchor;
            return t;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            cb.pressedColor = new Color(0.6f, 0.6f, 0.2f, 1f);
            btn.colors = cb;

            Text txt = CreateText(btnObj.transform, "BtnText", label, Vector2.zero, 18, Color.white, TextAnchor.MiddleCenter);
            txt.rectTransform.sizeDelta = size;

            return btn;
        }

        [MenuItem("Tractor Racing/Setup Build Settings Scenes", false, 3)]
        public static void ConfigureBuildSettings()
        {
            string[] scenePaths = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/LevelSelect.unity",
                "Assets/Scenes/Level1.unity",
                "Assets/Scenes/Level2.unity",
                "Assets/Scenes/Level3.unity",
                "Assets/Scenes/Level4.unity",
                "Assets/Scenes/Level5.unity",
                "Assets/Scenes/SampleScene.unity"
            };

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
            foreach (var path in scenePaths)
            {
                if (File.Exists(path))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(path, true));
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log($"<color=#00FF00>[Tractor Racing] Configured {buildScenes.Count} scenes in EditorBuildSettings.</color>");
        }
    }
}
#endif
