using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IndoorDrone.Editor
{
    public static class IndoorDroneSceneBuilder
    {
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

            const string root = "Assets/IndoorDrone/Generated";
            Directory.CreateDirectory(root);
            AssetDatabase.Refresh();
            string folder = AssetDatabase.GenerateUniqueAssetPath(root + "/Demo");
            AssetDatabase.CreateFolder(root, Path.GetFileName(folder));
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
            EditorSceneManager.SaveScene(scene, folder + "/IndoorDrone.unity");
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = drone;
            EditorUtility.DisplayDialog("Indoor Drone", "Scene created. Press Play to start. See README for controls.", "OK");
        }

        private static Material MaterialAsset(string folder, string name, Color color, Shader shader)
        {
            Material material = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(material, folder + "/" + name + ".mat");
            return material;
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
}
