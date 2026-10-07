#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using BhootiyaRasta.Core;
using BhootiyaRasta.Vehicle;
using BhootiyaRasta.CameraControl;
using BhootiyaRasta.Horror;
using BhootiyaRasta.Environment;
using BhootiyaRasta.Audio;
using BhootiyaRasta.UI;

namespace BhootiyaRasta
{
    public class BhootiyaGameBuilder : MonoBehaviour
    {
#if UNITY_EDITOR
        // Bhootiya Rasta menu disabled in favor of pure Tractor Racing
        // [InitializeOnLoadMethod]
        // [MenuItem("Bhootiya Rasta/Setup Complete Game Scene")]
        public static void SetupCompleteGameScene()
        {
            // 1. Generate PBR Textures
            Texture2D roadTex = TextureGenerator.CreateDirtRoadTexture(512, 512);
            Texture2D roadNormal = TextureGenerator.CreateRoadNormalMap(512, 512);
            Texture2D farmSoilTex = TextureGenerator.CreateFarmlandSoilTexture(256, 256);
            Texture2D tractorPaintTex = TextureGenerator.CreateTractorPaintTexture(256, 256);
            Texture2D tireRubberTex = TextureGenerator.CreateTireRubberTexture(256, 256);
            Texture2D clayWallTex = TextureGenerator.CreateClayMudWallTexture(256, 256);
            Texture2D thatchRoofTex = TextureGenerator.CreateThatchStrawTexture(256, 256);
            Texture2D woodTex = TextureGenerator.CreateWeatheredWoodTexture(256, 256);
            Texture2D stoneTex = TextureGenerator.CreateAncientStoneTexture(256, 256);
            Texture2D ghostClothTex = TextureGenerator.CreateGhostClothTexture(256, 256);

            // 2. Create Realistic Materials
            Material tractorRedMat = CreateTexturedMaterial("Mat_Tractor_Red", tractorPaintTex, 0.35f, 0.55f);
            Material tractorMetalMat = CreateStandardMaterial("Mat_Tractor_Metal", new Color(0.24f, 0.25f, 0.28f), 0.75f, 0.45f);
            Material tireRubberMat = CreateTexturedMaterial("Mat_Tractor_TireRubber", tireRubberTex, 0.0f, 0.15f);
            Material rimSteelMat = CreateStandardMaterial("Mat_Tractor_RimSteel", new Color(0.85f, 0.82f, 0.45f), 0.5f, 0.4f); // Classic farm yellow-cream wheel rims
            Material glassMat = CreateStandardMaterial("Mat_Tractor_Glass", new Color(0.9f, 0.95f, 1f, 0.6f), 0.1f, 0.95f);
            Material headlightGlowMat = CreateEmissiveMaterial("Mat_Headlight_Glow", new Color(1.0f, 0.92f, 0.75f));
            Material taillightGlowMat = CreateEmissiveMaterial("Mat_Taillight_Glow", new Color(0.95f, 0.05f, 0.05f));

            Material roadDirtMat = CreateTexturedMaterial("Mat_KacchaRoad_Dirt", roadTex, 0.0f, 0.15f, roadNormal);
            Material fieldGroundMat = CreateTexturedMaterial("Mat_Rural_Fields", farmSoilTex, 0.0f, 0.05f);
            Material woodMat = CreateTexturedMaterial("Mat_Rural_Wood", woodTex, 0.0f, 0.1f);
            Material thatchMat = CreateTexturedMaterial("Mat_Rural_Thatch", thatchRoofTex, 0.0f, 0.05f);
            Material clayWallMat = CreateTexturedMaterial("Mat_Rural_ClayWall", clayWallTex, 0.0f, 0.05f);
            Material stoneMat = CreateTexturedMaterial("Mat_Rural_Stone", stoneTex, 0.0f, 0.1f);
            Material leafMat = CreateStandardMaterial("Mat_Rural_Foliage", new Color(0.12f, 0.24f, 0.12f), 0.0f, 0.05f);
            Material cropMat = CreateStandardMaterial("Mat_Rural_Crops", new Color(0.18f, 0.32f, 0.12f), 0.0f, 0.05f);
            Material mudMat = CreateStandardMaterial("Mat_Mud_Puddle", new Color(0.12f, 0.08f, 0.05f), 0.2f, 0.92f);
            Material wireMat = CreateStandardMaterial("Mat_Electric_Wire", new Color(0.08f, 0.08f, 0.09f), 0.6f, 0.2f);

            Material ghostSkinMat = CreateStandardMaterial("Mat_Ghost_Skin", new Color(0.82f, 0.86f, 0.92f), 0.0f, 0.1f);
            Material ghostClothMat = CreateTexturedMaterial("Mat_Ghost_Cloth", ghostClothTex, 0.0f, 0.05f);
            Material ghostHairMat = CreateStandardMaterial("Mat_Ghost_Hair", new Color(0.02f, 0.02f, 0.03f), 0.0f, 0.05f);
            Material eyeGlowMat = CreateEmissiveMaterial("Mat_Ghost_EyeGlow", new Color(0.95f, 0.12f, 0.12f));

            if (!System.IO.Directory.Exists("Assets/Materials")) System.IO.Directory.CreateDirectory("Assets/Materials");
            if (!System.IO.Directory.Exists("Assets/Prefabs")) System.IO.Directory.CreateDirectory("Assets/Prefabs");

            // Clean previous objects if any
            CleanExistingSceneObjects();

            // 3. Build Realistic Indian Tractor with 4 Rotating Tyres
            GameObject tractorObj = IndianTractorImporter.BuildIndianTractor();
            PrefabUtility.SaveAsPrefabAsset(tractorObj, "Assets/Prefabs/IndianTractor_Bhootni.prefab");

            // 4. Build Realistic Ghost Face (Bhoot / Chudail)
            GameObject ghostObj = GhostFaceImporter.BuildGhostFace();
            PrefabUtility.SaveAsPrefabAsset(ghostObj, "Assets/Prefabs/IndianGhost_Bhootni.prefab");
            ghostObj.SetActive(false);

            // 5. Setup 1 AM Rural Night Directional Moonlight
            Light moonLight = SetupMoonlight();

            // 6. Setup Camera with realistic medium-distance follow
            SetupCamera(tractorObj.transform);

            // 7. Setup Environment, Imported Road & Obstacles
            GameObject envManagerObj = new GameObject("EnvironmentManager");
            var envMgr = envManagerObj.AddComponent<EnvironmentManager>();
            envMgr.moonLight = moonLight;

            GameObject roadObj = new GameObject("ImportedRoad");
            var importedRoad = roadObj.AddComponent<ImportedRoadBuilder>();
            importedRoad.BuildImportedRoadEnvironment();

            GameObject propObj = new GameObject("RuralPropGenerator");
            var propGen = propObj.AddComponent<RuralPropGenerator>();
            propGen.clayWallMat = clayWallMat;
            propGen.thatchRoofMat = thatchMat;
            propGen.weatheredWoodMat = woodMat;
            propGen.stoneMasonryMat = stoneMat;
            propGen.foliageLeafMat = leafMat;
            propGen.cropLeafMat = cropMat;
            propGen.rustedMetalMat = tractorMetalMat;
            propGen.electricalWireMat = wireMat;

            GameObject obsObj = new GameObject("RuralObstacleGenerator");
            var obsGen = obsObj.AddComponent<RuralObstacleGenerator>();
            obsGen.weatheredWoodMat = woodMat;
            obsGen.stoneMasonryMat = stoneMat;
            obsGen.mudPuddleMat = mudMat;
            obsGen.thatchStrawMat = thatchMat;
            obsGen.rustedMetalMat = tractorMetalMat;

            // 8. Setup Audio & Ghost Chase System
            GameObject audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();

            GameObject chaseObj = new GameObject("GhostChaseManager");
            var ghostChase = chaseObj.AddComponent<GhostChaseManager>();
            ghostChase.tractor = tractorObj.GetComponent<TractorController>();
            ghostChase.ghost = ghostObj.GetComponent<GhostController>();
            ghostChase.road = importedRoad;

            GameObject horrorObj = new GameObject("GhostEncounterManager");
            var ghostMgr = horrorObj.AddComponent<GhostEncounterManager>();
            ghostMgr.tractor = tractorObj.GetComponent<TractorController>();
            ghostMgr.ghostPrefab = ghostObj.GetComponent<GhostController>();

            GameObject horrorEventObj = new GameObject("HorrorEventManager");
            horrorEventObj.AddComponent<HorrorEventManager>();

            // 9. Setup Core Managers
            GameObject checkObj = new GameObject("CheckpointManager");
            var checkMgr = checkObj.AddComponent<CheckpointManager>();
            checkMgr.tractor = tractorObj.GetComponent<TractorController>();

            GameObject lvlObj = new GameObject("LevelManager");
            var lvlMgr = lvlObj.AddComponent<LevelManager>();
            lvlMgr.importedRoad = importedRoad;
            lvlMgr.propGenerator = propGen;
            lvlMgr.obstacleGenerator = obsGen;
            lvlMgr.environmentManager = envMgr;
            lvlMgr.ghostChaseManager = ghostChase;
            lvlMgr.tractor = tractorObj.GetComponent<TractorController>();

            GameObject uiObj = new GameObject("UIManager");
            uiObj.AddComponent<UIManager>();

            GameObject gameMgrObj = new GameObject("GameManager");
            gameMgrObj.AddComponent<GameManager>();

            // Load Level 1 into the world (Generates road, obstacles, destination gate, and starts 60s countdown)
            lvlMgr.LoadLevel(1);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("<color=#00FF00><b>[Bhootiya Rasta]</b> Complete 60-Second Challenge Game (Tractor, Kaccha Road, Obstacles, Ghost Chase & Destination Gate) Built Successfully!</color>");
        }

        // [MenuItem("Bhootiya Rasta/Rebuild Tractor Prefab Only")]
        public static void RebuildTractorPrefabOnly()
        {
            if (!System.IO.Directory.Exists("Assets/Prefabs")) System.IO.Directory.CreateDirectory("Assets/Prefabs");
            GameObject tractorObj = IndianTractorImporter.BuildIndianTractor();
            PrefabUtility.SaveAsPrefabAsset(tractorObj, "Assets/Prefabs/IndianTractor_Bhootni.prefab");
            Object.DestroyImmediate(tractorObj);
            AssetDatabase.Refresh();
            Debug.Log("<color=#00FF00><b>[Bhootiya Rasta]</b> Successfully built and saved Assets/Prefabs/IndianTractor_Bhootni.prefab!</color>");
        }

        // [MenuItem("Bhootiya Rasta/Delete Pink and Duplicate Tractors")]
        public static void DeletePinkAndDuplicateTractors()
        {
            string[] namesToRemove = new string[] {
                "IndianTractor_Mahindra",
                "IndianTractor_Bhootiya",
                "Tractor_Old",
                "TractorBody_Mesh",
                "EngineHood_Mesh",
                "ChassisVisual",
                "IndianTractor_Bhootni_Old",
                "Tractor_Pink"
            };

            int removedCount = 0;
            foreach (var n in namesToRemove)
            {
                var objs = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (var obj in objs)
                {
                    if (obj != null && obj.name == n)
                    {
                        Object.DestroyImmediate(obj);
                        removedCount++;
                    }
                }
            }

            // Also check for multiple Player tagged objects
            var players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length > 1)
            {
                // Keep the first valid one, remove extras
                for (int i = 1; i < players.Length; i++)
                {
                    Object.DestroyImmediate(players[i]);
                    removedCount++;
                }
            }

            // Fix Camera Target
            var activePlayer = GameObject.FindWithTag("Player") ?? GameObject.Find("IndianTractor_Bhootni");
            var cam = Camera.main;
            if (cam != null && activePlayer != null)
            {
                var cc = cam.GetComponent<CameraController>();
                if (cc == null) cc = cam.gameObject.AddComponent<CameraController>();
                cc.SetTarget(activePlayer.transform);
                cc.SnapToTarget();
            }

            Debug.Log($"<color=#00FF00><b>[Bhootiya Rasta]</b> Cleaned up {removedCount} duplicate/old tractor objects. Camera re-locked to active Player tractor!</color>");
        }

        private static void CleanExistingSceneObjects()
        {
            string[] toDelete = new string[] {
                "IndianTractor_Bhootni",
                "IndianTractor_Bhootiya",
                "IndianTractor_Mahindra",
                "Tractor_Racing",
                "Tractor_Old",
                "TractorBody_Mesh",
                "EngineHood_Mesh",
                "ChassisVisual",
                "IndianGhost_Bhootni",
                "IndianGhost_Chudail",
                "ImportedRoad",
                "ProceduralRoad",
                "RuralPropGenerator",
                "RuralObstacleGenerator",
                "EnvironmentManager",
                "AudioManager",
                "GhostChaseManager",
                "GhostEncounterManager",
                "HorrorEventManager",
                "CheckpointManager",
                "LevelManager",
                "UIManager",
                "GameManager"
            };

            foreach (var name in toDelete)
            {
                var allObjs = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (var go in allObjs)
                {
                    if (go != null && go.name == name)
                    {
                        Object.DestroyImmediate(go);
                    }
                }
            }
        }

        private static Material CreateTexturedMaterial(string name, Texture2D mainTex, float metallic, float smoothness, Texture2D normalMap = null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.name = name;
            mat.mainTexture = mainTex;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            if (normalMap != null && mat.HasProperty("_BumpMap"))
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", normalMap);
            }

            string path = $"Assets/Materials/{name}.mat";
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static Material CreateStandardMaterial(string name, Color color, float metallic, float smoothness)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.name = name;
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            string path = $"Assets/Materials/{name}.mat";
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static Material CreateEmissiveMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.name = name;
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 2.8f);
            }

            string path = $"Assets/Materials/{name}.mat";
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static Light SetupMoonlight()
        {
            GameObject lightObj = GameObject.Find("Directional Light");
            if (lightObj == null) lightObj = new GameObject("Directional Light");

            Light lt = lightObj.GetComponent<Light>();
            if (lt == null) lt = lightObj.AddComponent<Light>();

            lt.type = LightType.Directional;
            lt.color = new Color(0.72f, 0.82f, 0.98f); // Luminous realistic silvery-blue moonlight
            lt.intensity = 0.85f; // Bright, clear, atmospheric night illumination
            lt.shadows = LightShadows.Soft;
            lt.shadowStrength = 0.85f;

            lightObj.transform.rotation = Quaternion.Euler(48f, -38f, 0f);
            return lt;
        }

        private static void SetupCamera(Transform target)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.17f, 0.27f);
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.15f;
            cam.farClipPlane = 260f;

            var cc = cam.GetComponent<CameraController>();
            if (cc == null) cc = cam.gameObject.AddComponent<CameraController>();
            cc.SetTarget(target);
        }
#endif
    }
}
