using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using PackageManagerPackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace IndoorDrone.Editor
{
    public static class IndoorDroneSceneBuilder
    {
        internal const string GeneratedRoot = "Assets/IndoorDrone/Generated";

        [MenuItem("Tools/Indoor Drone/Create Demo Scene")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                EditorUtility.DisplayDialog("Indoor Drone", "Use a Built-in Render Pipeline project.", "OK");
                return;
            }

            EnsureFolder(GeneratedRoot);
            string folder = AssetDatabase.GenerateUniqueAssetPath(GeneratedRoot + "/Demo");
            AssetDatabase.CreateFolder(GeneratedRoot, Path.GetFileName(folder));
            Material floor = MaterialAsset(folder, "FloorStone", new Color(0.2f, 0.22f, 0.24f), shader, material =>
            {
                material.SetFloat("_Glossiness", 0.78f);
                material.SetFloat("_Metallic", 0.05f);
            });
            Material floorInset = MaterialAsset(folder, "FloorInset", new Color(0.73f, 0.73f, 0.7f), shader, material =>
            {
                material.SetFloat("_Glossiness", 0.72f);
                material.SetFloat("_Metallic", 0.02f);
            });
            Material cream = MaterialAsset(folder, "CreamStone", new Color(0.82f, 0.79f, 0.73f), shader, material =>
            {
                material.SetFloat("_Glossiness", 0.45f);
                material.SetFloat("_Metallic", 0.03f);
            });
            Material darkStone = MaterialAsset(folder, "DarkStone", new Color(0.15f, 0.15f, 0.17f), shader, material =>
            {
                material.SetFloat("_Glossiness", 0.6f);
                material.SetFloat("_Metallic", 0.08f);
            });
            Material glass = MaterialAsset(folder, "Glass", new Color(0.72f, 0.83f, 0.88f, 0.3f), shader, ConfigureGlassMaterial);
            Material goldStone = MaterialAsset(folder, "GoldPanel", new Color(0.62f, 0.47f, 0.28f), shader, material =>
            {
                material.SetFloat("_Glossiness", 0.52f);
                material.SetFloat("_Metallic", 0.16f);
            });
            Material eagleMetal = MaterialAsset(folder, "EagleMetal", new Color(0.14f, 0.14f, 0.16f), shader, material =>
            {
                material.SetFloat("_Glossiness", 0.88f);
                material.SetFloat("_Metallic", 0.82f);
            });
            Material planterWhite = MaterialAsset(folder, "Planter", new Color(0.93f, 0.93f, 0.93f), shader, material =>
            {
                material.SetFloat("_Glossiness", 0.63f);
                material.SetFloat("_Metallic", 0.02f);
            });
            Material foliage = MaterialAsset(folder, "Foliage", new Color(0.17f, 0.42f, 0.2f), shader, material =>
            {
                material.SetFloat("_Glossiness", 0.18f);
                material.SetFloat("_Metallic", 0f);
            });
            Material lightTrim = MaterialAsset(folder, "LightTrim", new Color(0.95f, 0.83f, 0.52f), shader, material =>
            {
                material.SetFloat("_Glossiness", 0.78f);
                material.SetFloat("_Metallic", 0.65f);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.22f, 0.18f, 0.08f));
            });
            Material downlight = MaterialAsset(folder, "Downlight", new Color(0.97f, 0.93f, 0.82f), shader, material =>
            {
                material.SetFloat("_Glossiness", 0.6f);
                material.SetFloat("_Metallic", 0f);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.35f, 0.32f, 0.26f));
            });
            Material frame = MaterialAsset(folder, "Drone", new Color(0.12f, 0.25f, 0.35f), shader);
            Material oldBulb = MaterialAsset(folder, "OldBulb", Color.gray, shader);
            Material newBulb = MaterialAsset(folder, "NewBulb", Color.yellow, shader);
            Material green = MaterialAsset(folder, "Supply", Color.green, shader);
            Material red = MaterialAsset(folder, "Recycle", new Color(0.85f, 0.25f, 0.2f), shader);
            Material blue = MaterialAsset(folder, "Home", Color.cyan, shader);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientLight = new Color(0.52f, 0.52f, 0.5f);
            Light lobbySun = new GameObject("Lobby ambient key").AddComponent<Light>();
            lobbySun.type = LightType.Directional;
            lobbySun.intensity = 0.65f;
            lobbySun.color = new Color(1f, 0.97f, 0.92f);
            lobbySun.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

            GameObject lobby = new GameObject("Lobby");
            Shape(lobby.transform, "Floor", PrimitiveType.Cube, new Vector3(0f, -0.1f, 0f), new Vector3(12f, 0.2f, 12f), floor);
            Shape(lobby.transform, "Floor inset", PrimitiveType.Cylinder, new Vector3(0f, 0.012f, 0.4f), new Vector3(3.6f, 0.012f, 3.6f), floorInset, false);
            Shape(lobby.transform, "Ceiling border", PrimitiveType.Cube, new Vector3(0f, 4.45f, 0f), new Vector3(12f, 0.16f, 12f), cream);
            Shape(lobby.transform, "Ceiling tray", PrimitiveType.Cube, new Vector3(0f, 4.28f, 0f), new Vector3(8.6f, 0.12f, 8.6f), cream);
            Shape(lobby.transform, "North lower wall", PrimitiveType.Cube, new Vector3(0f, 0.55f, 5.85f), new Vector3(12f, 1.1f, 0.3f), cream);
            Shape(lobby.transform, "North upper wall", PrimitiveType.Cube, new Vector3(0f, 4.08f, 5.85f), new Vector3(12f, 0.74f, 0.3f), cream);
            Shape(lobby.transform, "South wall", PrimitiveType.Cube, new Vector3(0f, 2.2f, -5.85f), new Vector3(12f, 4.4f, 0.3f), cream);
            Shape(lobby.transform, "West wall", PrimitiveType.Cube, new Vector3(-5.85f, 2.2f, 0f), new Vector3(0.3f, 4.4f, 12f), cream);
            Shape(lobby.transform, "East front wall", PrimitiveType.Cube, new Vector3(5.85f, 2.2f, -1.7f), new Vector3(0.3f, 4.4f, 8.3f), cream);
            Shape(lobby.transform, "East stone wall", PrimitiveType.Cube, new Vector3(5.15f, 2.2f, 2.8f), new Vector3(1.7f, 4.4f, 5.4f), darkStone);
            Shape(lobby.transform, "Stone column", PrimitiveType.Cube, new Vector3(4.35f, 2.2f, 3.35f), new Vector3(0.9f, 4.4f, 1.45f), darkStone);
            Shape(lobby.transform, "Door frame top", PrimitiveType.Cube, new Vector3(0f, 3.2f, 5.68f), new Vector3(8.8f, 0.16f, 0.16f), darkStone);
            Shape(lobby.transform, "Door frame left", PrimitiveType.Cube, new Vector3(-4.32f, 2f, 5.68f), new Vector3(0.16f, 2.16f, 0.16f), darkStone);
            Shape(lobby.transform, "Door frame right", PrimitiveType.Cube, new Vector3(4.32f, 2f, 5.68f), new Vector3(0.16f, 2.16f, 0.16f), darkStone);
            Shape(lobby.transform, "Door mullion left", PrimitiveType.Cube, new Vector3(-1.45f, 2f, 5.68f), new Vector3(0.12f, 2.16f, 0.12f), darkStone);
            Shape(lobby.transform, "Door mullion right", PrimitiveType.Cube, new Vector3(1.45f, 2f, 5.68f), new Vector3(0.12f, 2.16f, 0.12f), darkStone);
            Shape(lobby.transform, "Door glass left", PrimitiveType.Cube, new Vector3(-2.88f, 2f, 5.72f), new Vector3(2.72f, 2.16f, 0.05f), glass);
            Shape(lobby.transform, "Door glass center", PrimitiveType.Cube, new Vector3(0f, 2f, 5.72f), new Vector3(2.72f, 2.16f, 0.05f), glass);
            Shape(lobby.transform, "Door glass right", PrimitiveType.Cube, new Vector3(2.88f, 2f, 5.72f), new Vector3(2.72f, 2.16f, 0.05f), glass);
            Shape(lobby.transform, "Decor frame outer", PrimitiveType.Cube, new Vector3(0f, 2.1f, 5.54f), new Vector3(2.8f, 3.6f, 0.22f), darkStone);
            Shape(lobby.transform, "Decor frame inner", PrimitiveType.Cube, new Vector3(0f, 2.1f, 5.45f), new Vector3(2.2f, 3.12f, 0.08f), goldStone);
            Shape(lobby.transform, "Pedestal body", PrimitiveType.Cube, new Vector3(0f, 0.66f, 3.65f), new Vector3(1.2f, 1.32f, 1.2f), cream);
            Shape(lobby.transform, "Pedestal base trim", PrimitiveType.Cube, new Vector3(0f, 0.12f, 3.65f), new Vector3(1.32f, 0.08f, 1.32f), lightTrim);
            Shape(lobby.transform, "Pedestal top trim", PrimitiveType.Cube, new Vector3(0f, 1.24f, 3.65f), new Vector3(1.32f, 0.08f, 1.32f), lightTrim);
            CreateEagleStatue(lobby.transform, new Vector3(0f, 1.42f, 3.65f), eagleMetal);
            CreatePlanter(lobby.transform, "Left planter", new Vector3(-2.1f, 0f, 3.45f), planterWhite, foliage);
            CreatePlanter(lobby.transform, "Right planter", new Vector3(2.1f, 0f, 3.45f), planterWhite, foliage);
            CreatePlanter(lobby.transform, "Far right planter", new Vector3(3.45f, 0f, 2.45f), planterWhite, foliage);
            CreateCeilingDetails(lobby.transform, cream, darkStone, downlight);
            Shape("Landing pad", PrimitiveType.Cylinder, new Vector3(0f, 0.015f, -3f), new Vector3(1.4f, 0.015f, 1.4f), blue, false);

            GameObject drone = new GameObject("Drone");
            drone.transform.position = new Vector3(0f, 0.22f, -3f);
            GameObject bodyVisual = Shape("Body", PrimitiveType.Cube, drone.transform.position, new Vector3(0.5f, 0.18f, 0.5f), frame, false);
            bodyVisual.transform.SetParent(drone.transform, true);
            foreach (float x in new[] { -0.35f, 0.35f })
            foreach (float z in new[] { -0.35f, 0.35f })
            {
                GameObject rotor = Shape("Rotor guard", PrimitiveType.Cylinder,
                    drone.transform.position + new Vector3(x, 0f, z), new Vector3(0.32f, 0.025f, 0.32f), frame, false);
                rotor.transform.SetParent(drone.transform, true);
            }
            BoxCollider collider = drone.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.05f, 0.2f, 1.05f);
            Rigidbody body = drone.AddComponent<Rigidbody>();
            body.mass = 1.5f;
            body.drag = 0.25f;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            DroneController controller = drone.AddComponent<DroneController>();
            GameObject gripper = Shape("Simulated gripper", PrimitiveType.Cylinder,
                drone.transform.position + Vector3.up * 0.3f, new Vector3(0.08f, 0.2f, 0.08f), frame);
            gripper.transform.SetParent(drone.transform, true);
            GameObject payload = Shape("Carried bulb", PrimitiveType.Sphere,
                drone.transform.position + Vector3.up * 0.62f, Vector3.one * 0.2f, newBulb, false);
            payload.transform.SetParent(drone.transform, true);
            payload.SetActive(false);

            Shape("Task light trim", PrimitiveType.Cylinder, new Vector3(0f, 4.25f, 2f), new Vector3(0.42f, 0.035f, 0.42f), lightTrim);
            Shape("Socket stem", PrimitiveType.Cylinder, new Vector3(0f, 4.02f, 2f), new Vector3(0.16f, 0.22f, 0.16f), darkStone);
            GameObject socketBulb = Shape("Socket bulb", PrimitiveType.Sphere, new Vector3(0f, 3.67f, 2f), Vector3.one * 0.2f, oldBulb, false);
            GameObject supplyBulb = Shape("Spare bulb", PrimitiveType.Sphere, new Vector3(-3f, 1.57f, 2f), Vector3.one * 0.2f, newBulb, false);
            Shape("Supply cabinet", PrimitiveType.Cube, new Vector3(-3f, 0.58f, 2.95f), new Vector3(1.15f, 1.16f, 0.7f), green);
            Shape("Supply cabinet top", PrimitiveType.Cube, new Vector3(-3f, 1.19f, 2.95f), new Vector3(1.22f, 0.1f, 0.76f), cream);
            Shape("Recycle bin", PrimitiveType.Cube, new Vector3(3f, 0.38f, 2.95f), new Vector3(1.15f, 0.76f, 0.7f), red);
            Shape("Recycle lid", PrimitiveType.Cube, new Vector3(3f, 0.8f, 2.95f), new Vector3(1.22f, 0.08f, 0.76f), darkStone);
            Marker("SOCKET", new Vector3(0f, 3.05f, 2f), newBulb);
            Marker("SUPPLY", new Vector3(-3f, 0.95f, 2f), green);
            Marker("RECYCLE", new Vector3(3f, 0.95f, 2f), red);

            Light roomLight = new GameObject("Room light").AddComponent<Light>();
            roomLight.type = LightType.Point;
            roomLight.range = 12f;
            roomLight.intensity = 0f;
            roomLight.transform.position = new Vector3(0f, 3.82f, 2f);
            Camera camera = new GameObject("Follow camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.05f;
            camera.transform.position = new Vector3(0f, 2f, -4.5f);
            camera.gameObject.AddComponent<AudioListener>();
            camera.gameObject.AddComponent<DroneCamera>().Target = drone.transform;

            BulbReplacementMission mission = new GameObject("Mission").AddComponent<BulbReplacementMission>();
            mission.Drone = controller;
            mission.SocketBulb = socketBulb;
            mission.SupplyBulb = supplyBulb;
            mission.Payload = payload;
            mission.RoomLight = roomLight;
            mission.OldBulbMaterial = oldBulb;
            mission.NewBulbMaterial = newBulb;
            string scenePath = folder + "/IndoorDrone.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            RegisterBuildScene(scenePath);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = drone;
            EditorUtility.DisplayDialog("Indoor Drone",
                "Scene created and added to Build Settings. Press Play to start. See README for controls.", "OK");
        }

        private static void EnsureFolder(string assetPath)
        {
            string[] segments = assetPath.Split('/');
            string current = segments[0];
            for (int i = 1; i < segments.Length; i++)
            {
                string next = current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, segments[i]);
                current = next;
            }
        }

        private static Material MaterialAsset(string folder, string name, Color color, Shader shader, Action<Material> configure = null)
        {
            Material material = new Material(shader) { color = color };
            configure?.Invoke(material);
            AssetDatabase.CreateAsset(material, folder + "/" + name + ".mat");
            return material;
        }

        private static void RegisterBuildScene(string scenePath)
        {
            if (EditorBuildSettings.scenes.Any(scene => scene.path == scenePath))
                return;

            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static GameObject Shape(string name, PrimitiveType type, Vector3 position,
            Vector3 scale, Material material, bool solid = true)
        {
            GameObject result = GameObject.CreatePrimitive(type);
            result.name = name;
            result.transform.position = position;
            result.transform.localScale = scale;
            result.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid)
                UnityEngine.Object.DestroyImmediate(result.GetComponent<Collider>());
            return result;
        }

        private static GameObject Shape(Transform parent, string name, PrimitiveType type, Vector3 localPosition,
            Vector3 localScale, Material material, bool solid = true, Vector3? localEulerAngles = null)
        {
            GameObject result = Shape(name, type, Vector3.zero, localScale, material, solid);
            result.transform.SetParent(parent, false);
            result.transform.localPosition = localPosition;
            if (localEulerAngles.HasValue)
                result.transform.localRotation = Quaternion.Euler(localEulerAngles.Value);
            return result;
        }

        private static void Marker(string name, Vector3 position, Material material)
        {
            Shape(name + " hover target", PrimitiveType.Sphere, position, Vector3.one * 0.08f, material, false);
            TextMesh label = new GameObject(name + " label").AddComponent<TextMesh>();
            label.text = name;
            label.characterSize = 0.12f;
            label.fontSize = 48;
            label.anchor = TextAnchor.MiddleCenter;
            label.transform.position = position + new Vector3(0f, 0.3f, 0.35f);
        }

        private static void ConfigureGlassMaterial(Material material)
        {
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            material.SetFloat("_Glossiness", 0.92f);
            material.SetFloat("_Metallic", 0f);
        }

        private static void CreateCeilingDetails(Transform parent, Material cream, Material darkStone, Material downlight)
        {
            foreach (float x in new[] { -3.4f, -1.2f, 1.2f, 3.4f })
            foreach (float z in new[] { -2.7f, 0f, 2.7f })
            {
                Shape(parent, "Downlight trim", PrimitiveType.Cylinder, new Vector3(x, 4.24f, z),
                    new Vector3(0.24f, 0.02f, 0.24f), cream, false);
                Shape(parent, "Downlight lens", PrimitiveType.Cylinder, new Vector3(x, 4.215f, z),
                    new Vector3(0.16f, 0.012f, 0.16f), downlight, false);
            }

            Shape(parent, "Ventilation grille main", PrimitiveType.Cube, new Vector3(0f, 4.215f, -3.85f),
                new Vector3(2.2f, 0.02f, 0.52f), darkStone, false);
            Shape(parent, "Ventilation grille side", PrimitiveType.Cube, new Vector3(2.95f, 4.215f, -3.85f),
                new Vector3(1.15f, 0.02f, 0.52f), darkStone, false);
        }

        private static void CreatePlanter(Transform parent, string name, Vector3 basePosition, Material planterWhite, Material foliage)
        {
            GameObject planter = new GameObject(name);
            planter.transform.SetParent(parent, false);
            planter.transform.localPosition = basePosition;

            Shape(planter.transform, "Pot", PrimitiveType.Cylinder, new Vector3(0f, 0.42f, 0f),
                new Vector3(0.5f, 0.42f, 0.5f), planterWhite);
            Shape(planter.transform, "Soil", PrimitiveType.Cylinder, new Vector3(0f, 0.78f, 0f),
                new Vector3(0.4f, 0.02f, 0.4f), foliage, false);
            Shape(planter.transform, "Leaf cluster", PrimitiveType.Sphere, new Vector3(0f, 1.32f, 0f),
                new Vector3(0.68f, 0.88f, 0.68f), foliage, false);
            Shape(planter.transform, "Leaf left", PrimitiveType.Sphere, new Vector3(-0.22f, 1.04f, 0.08f),
                new Vector3(0.34f, 0.48f, 0.34f), foliage, false);
            Shape(planter.transform, "Leaf right", PrimitiveType.Sphere, new Vector3(0.24f, 1.1f, -0.06f),
                new Vector3(0.38f, 0.54f, 0.38f), foliage, false);
        }

        private static void CreateEagleStatue(Transform parent, Vector3 localPosition, Material eagleMetal)
        {
            GameObject eagle = new GameObject("Eagle statue");
            eagle.transform.SetParent(parent, false);
            eagle.transform.localPosition = localPosition;

            Shape(eagle.transform, "Body", PrimitiveType.Capsule, new Vector3(0f, 0f, 0f),
                new Vector3(0.34f, 0.42f, 0.28f), eagleMetal, true, new Vector3(90f, 0f, 0f));
            Shape(eagle.transform, "Head", PrimitiveType.Sphere, new Vector3(0f, 0.28f, 0.1f),
                new Vector3(0.16f, 0.16f, 0.16f), eagleMetal);
            Shape(eagle.transform, "Tail", PrimitiveType.Cube, new Vector3(0f, -0.14f, -0.22f),
                new Vector3(0.16f, 0.08f, 0.3f), eagleMetal, true, new Vector3(28f, 0f, 0f));
            Shape(eagle.transform, "Wing left upper", PrimitiveType.Cube, new Vector3(-0.56f, 0.16f, 0f),
                new Vector3(0.94f, 0.08f, 0.38f), eagleMetal, true, new Vector3(0f, 8f, 28f));
            Shape(eagle.transform, "Wing right upper", PrimitiveType.Cube, new Vector3(0.56f, 0.16f, 0f),
                new Vector3(0.94f, 0.08f, 0.38f), eagleMetal, true, new Vector3(0f, -8f, -28f));
            Shape(eagle.transform, "Wing left lower", PrimitiveType.Cube, new Vector3(-0.9f, -0.02f, -0.06f),
                new Vector3(0.72f, 0.07f, 0.32f), eagleMetal, true, new Vector3(0f, 10f, 8f));
            Shape(eagle.transform, "Wing right lower", PrimitiveType.Cube, new Vector3(0.9f, -0.02f, -0.06f),
                new Vector3(0.72f, 0.07f, 0.32f), eagleMetal, true, new Vector3(0f, -10f, -8f));
        }
    }

    [InitializeOnLoad]
    public static class IndoorDroneProjectValidator
    {
        private const string ValidationMenu = "Tools/Indoor Drone/Validate Development Environment";
        private const string ValidationShownKey = "IndoorDrone.ProjectValidator.Shown";
        private const string ExpectedUnityVersion = "2022.3.62f3";
        private const string ExpectedVisualStudioPackageVersion = "2.0.22";
        private const string VisualStudioPackagePath = "Packages/com.unity.ide.visualstudio";

        static IndoorDroneProjectValidator()
        {
            EditorApplication.delayCall += ValidateOnEditorStartup;
        }

        [MenuItem(ValidationMenu)]
        public static void ValidateDevelopmentEnvironment()
        {
            ValidationReport report = BuildReport();
            LogReport(report);
            EditorUtility.DisplayDialog("Indoor Drone", report.DialogMessage, "OK");
        }

        private static void ValidateOnEditorStartup()
        {
            if (Application.isBatchMode || SessionState.GetBool(ValidationShownKey, false))
                return;

            SessionState.SetBool(ValidationShownKey, true);
            ValidationReport report = BuildReport();
            LogReport(report);
            if (report.HasIssues)
                EditorUtility.DisplayDialog("Indoor Drone", report.DialogMessage, "OK");
        }

        private static ValidationReport BuildReport()
        {
            List<string> summary = new List<string>();
            List<string> nextSteps = new List<string>();
            bool hasIssues = false;

            if (Application.unityVersion == ExpectedUnityVersion)
            {
                summary.Add("Unity Editor 版本：符合 2022.3.62f3。");
            }
            else
            {
                hasIssues = true;
                summary.Add("Unity Editor 版本：" + Application.unityVersion + "（預期為 2022.3.62f3）。");
                nextSteps.Add("請在 Unity Hub 使用 2022.3.62f3 開啟專案，再重新驗證。");
            }

            PackageManagerPackageInfo visualStudioPackage = PackageManagerPackageInfo.FindForAssetPath(VisualStudioPackagePath);
            if (visualStudioPackage == null)
            {
                hasIssues = true;
                summary.Add("Visual Studio Editor 套件：尚未還原或 Unity Package Manager 尚在匯入。");
                nextSteps.Add("請等待 Package Manager 完成，確認 com.unity.ide.visualstudio 2.0.22 已還原。");
            }
            else if (visualStudioPackage.version == ExpectedVisualStudioPackageVersion)
            {
                summary.Add("Visual Studio Editor 套件：符合 2.0.22。");
            }
            else
            {
                hasIssues = true;
                summary.Add("Visual Studio Editor 套件：" + visualStudioPackage.version + "（預期為 2.0.22）。");
                nextSteps.Add("請確認 Packages/manifest.json 未被本機覆寫，並讓 Unity 重新還原 2.0.22。");
            }

            string inputHandling = DescribeInputHandling(out bool inputSupported, out bool inputDetected);
            if (!inputDetected)
            {
                hasIssues = true;
                summary.Add("Active Input Handling：無法自動讀取。");
                nextSteps.Add("請到 Edit → Project Settings → Player → Other Settings，確認 Active Input Handling 為 Input Manager (Old) 或 Both。");
            }
            else if (inputSupported)
            {
                summary.Add("Active Input Handling：" + inputHandling + "。");
            }
            else
            {
                hasIssues = true;
                summary.Add("Active Input Handling：" + inputHandling + "（目前不支援舊式鍵盤輸入）。");
                nextSteps.Add("請改成 Input Manager (Old) 或 Both，讓 WASD／Space／E／P／L 可於模擬內運作。");
            }

            string[] demoScenes = AssetDatabase.FindAssets("t:Scene", new[] { IndoorDroneSceneBuilder.GeneratedRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith("/IndoorDrone.unity", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (demoScenes.Length == 0)
            {
                hasIssues = true;
                summary.Add("示範場景：尚未產生。");
                nextSteps.Add("請執行 Tools → Indoor Drone → Create Demo Scene；工具會建立場景並自動加入 Build Settings。");
            }
            else
            {
                summary.Add("示範場景：已找到 " + demoScenes.Length + " 個 IndoorDrone.unity。");
                if (EditorBuildSettings.scenes.Any(scene => scene.enabled && demoScenes.Contains(scene.path)))
                {
                    summary.Add("Build Settings：已包含至少一個啟用中的示範場景。");
                    if (demoScenes.Length > 1)
                        nextSteps.Add("若保留多個 Generated Demo，建置前請在 Build Settings 只勾選這次要啟動的場景。");
                }
                else
                {
                    hasIssues = true;
                    summary.Add("Build Settings：尚未啟用任何示範場景。");
                    nextSteps.Add("請重新執行 Create Demo Scene，或到 File → Build Settings 將要使用的 IndoorDrone.unity 加入並勾選。");
                }
            }

            if (nextSteps.Count == 0)
                nextSteps.Add("目前可從 Unity 開啟產生的場景、按 Play 驗證操作，並在 VS2022 重新產生方案後 Attach to Unity 偵錯。");

            return new ValidationReport(
                hasIssues,
                "開發環境檢查結果：\n- " + string.Join("\n- ", summary) + "\n\n後續動作：\n- " + string.Join("\n- ", nextSteps),
                "[Indoor Drone] Development environment validation\n- " + string.Join("\n- ", summary) + "\n- Next: " + string.Join(" | ", nextSteps));
        }

        private static void LogReport(ValidationReport report)
        {
            if (report.HasIssues)
                Debug.LogWarning(report.LogMessage);
            else
                Debug.Log(report.LogMessage);
        }

        private static string DescribeInputHandling(out bool inputSupported, out bool inputDetected)
        {
            inputSupported = false;
            inputDetected = false;

            try
            {
                PropertyInfo property = typeof(PlayerSettings).GetProperty("activeInputHandler",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (property == null)
                    return "unknown";

                object value = property.GetValue(null, null);
                if (value == null)
                    return "unknown";

                inputDetected = true;
                string text = value.ToString() ?? string.Empty;
                if (int.TryParse(text, out int numeric))
                {
                    switch (numeric)
                    {
                        case 0:
                            inputSupported = true;
                            return "Input Manager (Old)";
                        case 1:
                            return "Input System Package (New)";
                        case 2:
                            inputSupported = true;
                            return "Both";
                    }
                }

                if (text.IndexOf("Both", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    inputSupported = true;
                    return "Both";
                }

                if (text.IndexOf("Old", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    inputSupported = true;
                    return "Input Manager (Old)";
                }

                if (text.IndexOf("New", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "Input System Package (New)";

                return text;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Indoor Drone] Failed to read Active Input Handling: " + exception.Message);
                return "unknown";
            }
        }

        private sealed class ValidationReport
        {
            public ValidationReport(bool hasIssues, string dialogMessage, string logMessage)
            {
                HasIssues = hasIssues;
                DialogMessage = dialogMessage;
                LogMessage = logMessage;
            }

            public bool HasIssues { get; }
            public string DialogMessage { get; }
            public string LogMessage { get; }
        }
    }
}
