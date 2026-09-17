using UnityEngine;

namespace IndoorDrone
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DroneController : MonoBehaviour
    {
        public Vector3 Home { get; private set; }
        public bool IsFlying { get; private set; }
        public bool IsLanding { get; private set; }
        public float Speed => body.velocity.magnitude;
        public int Collisions { get; private set; }
        public bool IsHome => !IsFlying && Vector3.Distance(transform.position, Home) < 0.4f;

        private Rigidbody body;
        private Vector3 target;
        private Vector3 input;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            Home = transform.position;
            target = Home;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !IsFlying)
            {
                IsFlying = true;
                target = transform.position + Vector3.up * 0.8f;
            }

            if (Input.GetKeyDown(KeyCode.L) && IsFlying)
                IsLanding = true;

            input = new Vector3(
                (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f),
                (Input.GetKey(KeyCode.R) ? 1f : 0f) - (Input.GetKey(KeyCode.F) ? 1f : 0f),
                (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f));
            input = Vector3.ClampMagnitude(input, 1f);
        }

        private void FixedUpdate()
        {
            if (!IsFlying)
                return;

            if (IsLanding)
            {
                Vector3 horizontalOffset = Home - transform.position;
                horizontalOffset.y = 0f;
                target = horizontalOffset.magnitude > 0.15f
                    ? new Vector3(Home.x, Mathf.Max(transform.position.y, 0.9f), Home.z)
                    : Home;

                if (horizontalOffset.magnitude < 0.2f &&
                    transform.position.y <= Home.y + 0.08f && Speed < 0.3f)
                {
                    IsFlying = false;
                    IsLanding = false;
                    return;
                }
            }
            else
            {
                float speed = Input.GetKey(KeyCode.LeftShift) ? 0.3f : 1.2f;
                target += input * (speed * Time.fixedDeltaTime);
                // Limit position error so a blocked drone cannot accumulate a distant target.
                target = transform.position + Vector3.ClampMagnitude(target - transform.position, 0.5f);
                target.x = Mathf.Clamp(target.x, -4.2f, 4.2f);
                target.y = Mathf.Clamp(target.y, 0.22f, 3.6f);
                target.z = Mathf.Clamp(target.z, -4.2f, 4.2f);
            }

            Vector3 desiredVelocity = Vector3.ClampMagnitude((target - transform.position) * 2.5f, 1.2f);
            Vector3 acceleration = Vector3.ClampMagnitude((desiredVelocity - body.velocity) * 5f, 8f);
            body.AddForce(acceleration - Physics.gravity, ForceMode.Acceleration);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsFlying && !IsLanding && collision.relativeVelocity.magnitude > 0.3f)
                Collisions++;
        }
    }
}
