using UnityEngine;

namespace IndoorDrone
{
    public sealed class DroneCamera : MonoBehaviour
    {
        public Transform Target;

        private void LateUpdate()
        {
            if (Target == null)
                return;

            Vector3 desired = Target.position + new Vector3(0f, 2f, -3.8f);
            desired.x = Mathf.Clamp(desired.x, -4.7f, 4.7f);
            desired.y = Mathf.Clamp(desired.y, 0.5f, 4f);
            desired.z = Mathf.Clamp(desired.z, -4.7f, 4.7f);
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-6f * Time.deltaTime));
            transform.LookAt(Target.position + Vector3.up * 0.35f);
        }
    }
}
