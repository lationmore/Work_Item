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

namespace IndoorDrone.Editor
{
    public static class IndoorDroneSceneBuilder
    {
        private const string GeneratedRoot = "Assets/IndoorDrone/Generated";

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
            Material floor = MaterialAsset(folder, "Room", new Color(0.55f, 0.6f, 0.65f), shader);
            Material frame = MaterialAsset(folder, "Drone", new Color(0.12f, 0.25f, 0.35f), shader);
            Material oldBulb = MaterialAsset(folder, "OldBulb", Color.gray, shader);
            Material newBulb = MaterialAsset(folder, "NewBulb", Color.yellow, shader);
            Material green = MaterialAsset(folder, "Supply", Color.green, shader);
            Material red = MaterialAsset(folder, "Recycle", new Color(0.85f, 0.25f, 0.2f), shader);
            Material blue = MaterialAsset(folder, "Home", Color.cyan, shader);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.6f);
            Shape("Floor", PrimitiveType.Cube, new Vector3(0f, -0.1f, 0f), new Vector3(10f, 0.2f, 10f), floor);
            Shape("Ceiling", PrimitiveType.Cube, new Vector3(0f, 4.4f, 0f), new Vector3(10f, 0.2f, 10f), floor);
            Shape("North wall", PrimitiveType.Cube, new Vector3(0f, 2.15f, 5f), new Vector3(10f, 4.3f, 0.2f), floor);
            Shape("South wall", PrimitiveType.Cube, new Vector3(0f, 2.15f, -5f), new Vector3(10f, 4.3f, 0.2f), floor);
            Shape("East wall", PrimitiveType.Cube, new Vector3(5f, 2.15f, 0f), new Vector3(0.2f, 4.3f, 10f), floor);
            Shape("West wall", PrimitiveType.Cube, new Vector3(-5f, 2.15f, 0f), new Vector3(0.2f, 4.3f, 10f), floor);
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

            Shape("Socket stem", PrimitiveType.Cylinder, new Vector3(0f, 4.05f, 2f), new Vector3(0.2f, 0.25f, 0.2f), frame);
            GameObject socketBulb = Shape("Socket bulb", PrimitiveType.Sphere, new Vector3(0f, 3.67f, 2f), Vector3.one * 0.2f, oldBulb, false);
            GameObject supplyBulb = Shape("Spare bulb", PrimitiveType.Sphere, new Vector3(-3f, 1.57f, 2f), Vector3.one * 0.2f, newBulb, false);
            Shape("Supply stand", PrimitiveType.Cube, new Vector3(-3f, 0.55f, 2.9f), new Vector3(1f, 1.1f, 0.6f), green);
            Shape("Recycle bin", PrimitiveType.Cube, new Vector3(3f, 0.35f, 2.9f), new Vector3(1f, 0.7f, 0.6f), red);
            Marker("SOCKET", new Vector3(0f, 3.05f, 2f), newBulb);
            Marker("SUPPLY", new Vector3(-3f, 0.95f, 2f), green);
            Marker("RECYCLE", new Vector3(3f, 0.95f, 2f), red);

            Light roomLight = new GameObject("Room light").AddComponent<Light>();
            roomLight.type = LightType.Point;
            roomLight.range = 12f;
            roomLight.intensity = 0f;
            roomLight.transform.position = new Vector3(0f, 3.5f, 2f);
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

        private static Material MaterialAsset(string folder, string name, Color color, Shader shader)
        {
            Material material = new Material(shader) { color = color };
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
                Object.DestroyImmediate(result.GetComponent<Collider>());
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

            PackageInfo visualStudioPackage = PackageInfo.FindForAssetPath(VisualStudioPackagePath);
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

            string[] demoScenes = AssetDatabase.FindAssets("t:Scene", new[] { GeneratedRoot })
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
