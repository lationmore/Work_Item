using UnityEngine;

namespace IndoorDrone
{
    public sealed class BulbReplacementMission : MonoBehaviour
    {
        public DroneController Drone;
        public GameObject SocketBulb;
        public GameObject SupplyBulb;
        public GameObject Payload;
        public Light RoomLight;
        public Material OldBulbMaterial;
        public Material NewBulbMaterial;

        private enum Stage { RemoveOld, RecycleOld, CollectNew, InstallNew, RestorePower, Land, Complete }
        private Stage stage;
        private bool powerOn = true;
        private string message = "Press P to isolate simulated power before servicing.";
        private float elapsed;

        private static readonly Vector3 SocketTarget = new Vector3(0f, 3.05f, 2f);
        private static readonly Vector3 RecycleTarget = new Vector3(3f, 0.95f, 2f);
        private static readonly Vector3 SupplyTarget = new Vector3(-3f, 0.95f, 2f);

        private Vector3 Target
        {
            get
            {
                switch (stage)
                {
                    case Stage.RecycleOld: return RecycleTarget;
                    case Stage.CollectNew: return SupplyTarget;
                    case Stage.Land:
                    case Stage.Complete: return Drone.Home;
                    default: return SocketTarget;
                }
            }
        }

        private void Update()
        {
            if (stage == Stage.Complete)
                return;

            elapsed += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.P))
                TogglePower();
            if (Input.GetKeyDown(KeyCode.E))
                Interact();
            if (stage == Stage.Land && Drone.IsHome)
            {
                stage = Stage.Complete;
                message = "Mission complete: new bulb lit and drone safely returned.";
            }
        }

        private void TogglePower()
        {
            if (powerOn)
            {
                powerOn = false;
                RoomLight.intensity = 0f;
                if (stage == Stage.Land)
                    stage = Stage.RestorePower;
                message = "Simulated power isolated.";
                return;
            }

            if (stage != Stage.RestorePower || Payload.activeSelf)
            {
                message = "Interlock: install the new bulb before restoring power.";
                return;
            }

            if (Vector3.Distance(Drone.transform.position, SocketTarget) < 0.8f)
            {
                message = "Move at least 0.8 m away from the socket before restoring power.";
                return;
            }

            powerOn = true;
            RoomLight.intensity = 2f;
            stage = Stage.Land;
            message = "New bulb is lit. Press L to return to the landing pad.";
        }

        private void Interact()
        {
            if (stage >= Stage.RestorePower)
            {
                message = stage == Stage.RestorePower
                    ? "Move clear of the socket, then press P to restore power."
                    : "Press L to return and land.";
                return;
            }

            if (powerOn)
            {
                message = "Blocked: press P to isolate simulated power first.";
                return;
            }

            if (!Drone.IsFlying || Drone.IsLanding ||
                Vector3.Distance(Drone.transform.position, Target) > 0.25f || Drone.Speed > 0.25f)
            {
                message = "Hover within 0.25 m of the target at less than 0.25 m/s; use Shift for precision.";
                return;
            }

            switch (stage)
            {
                case Stage.RemoveOld:
                    SocketBulb.SetActive(false);
                    Payload.GetComponent<Renderer>().sharedMaterial = OldBulbMaterial;
                    Payload.SetActive(true);
                    stage = Stage.RecycleOld;
                    message = "Old bulb secured. Fly to the red RECYCLE target.";
                    break;
                case Stage.RecycleOld:
                    Payload.SetActive(false);
                    stage = Stage.CollectNew;
                    message = "Old bulb recycled. Fly to the green SUPPLY target.";
                    break;
                case Stage.CollectNew:
                    SupplyBulb.SetActive(false);
                    Payload.GetComponent<Renderer>().sharedMaterial = NewBulbMaterial;
                    Payload.SetActive(true);
                    stage = Stage.InstallNew;
                    message = "New bulb secured. Return to the yellow SOCKET target.";
                    break;
                case Stage.InstallNew:
                    Payload.SetActive(false);
                    SocketBulb.GetComponent<Renderer>().sharedMaterial = NewBulbMaterial;
                    SocketBulb.SetActive(true);
                    stage = Stage.RestorePower;
                    message = "New bulb installed. Move clear, then press P to restore power.";
                    break;
            }
        }

        private void OnGUI()
        {
            float scale = Mathf.Min(Screen.width / 960f, Screen.height / 600f);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            GUILayout.BeginArea(new Rect(12f, 12f, 640f, 265f), GUI.skin.box);
            GUILayout.Label("INDOOR DRONE - BULB REPLACEMENT (SIMULATION ONLY)");
            GUILayout.Label("Space: take off | WASD: move | R/F: up/down | Shift: precision");
            GUILayout.Label("Release keys: hover | E: interact | P: simulated power | L: return / land");
            GUILayout.Label("Objective: " + Objective());
            GUILayout.Label("Power: " + (powerOn ? "ON" : "ISOLATED") +
                " | Flight: " + (Drone.IsLanding ? "RETURNING" : Drone.IsFlying ? "HOVER / MANUAL" : "GROUNDED"));
            GUILayout.Label($"Altitude: {Drone.transform.position.y:F2} m | Speed: {Drone.Speed:F2} m/s | Contacts: {Drone.Collisions}");
            GUILayout.Label($"Target XYZ: {Target.x:F2}, {Target.y:F2}, {Target.z:F2} | Distance: {Vector3.Distance(Drone.transform.position, Target):F2} m");
            GUILayout.Label($"Time: {elapsed:F1} s");
            GUILayout.Label(message);
            GUILayout.Label("Restart: stop Play mode and press Play again.");
            GUILayout.EndArea();
            GUI.matrix = previous;
        }

        private string Objective()
        {
            switch (stage)
            {
                case Stage.RemoveOld: return "1/7 Isolate power (P), hover at SOCKET, remove old bulb (E)";
                case Stage.RecycleOld: return "2/7 Hover at RECYCLE and deposit old bulb (E)";
                case Stage.CollectNew: return "3/7 Hover at SUPPLY and collect new bulb (E)";
                case Stage.InstallNew: return "4/7 Hover at SOCKET and install new bulb (E)";
                case Stage.RestorePower: return "5/7 Move clear and restore power (P)";
                case Stage.Land: return "6/7 Return and land (L)";
                default: return "7/7 COMPLETE";
            }
        }
    }
}
