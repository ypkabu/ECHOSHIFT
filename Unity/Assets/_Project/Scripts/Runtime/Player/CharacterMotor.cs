using UnityEngine;

namespace EchoShift.Player
{
    public sealed class CharacterMotor : MonoBehaviour
    {
        private const float SkinWidth = 0.015f;

        [SerializeField, Min(0.01f)] private float moveSpeed = 4f;
        [SerializeField, Min(0.01f)] private float capsuleRadius = 0.45f;
        [SerializeField, Min(0.1f)] private float capsuleHeight = 2f;
        [SerializeField] private LayerMask collisionMask = ~0;

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        public void Configure(
            float speed,
            float radius,
            float height,
            LayerMask blockingLayers)
        {
            moveSpeed = speed;
            capsuleRadius = radius;
            capsuleHeight = height;
            collisionMask = blockingLayers;
        }

        public void Simulate(Vector2 moveInput, float tickDuration)
        {
            Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            if (direction.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            float distance = moveSpeed * tickDuration;
            Vector3 displacement = direction * distance;
            MoveAxis(new Vector3(displacement.x, 0f, 0f));
            MoveAxis(new Vector3(0f, 0f, displacement.z));

            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        public void ResetPose(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        private void MoveAxis(Vector3 displacement)
        {
            float distance = displacement.magnitude;
            if (distance <= 0.000001f)
            {
                return;
            }

            Vector3 center = transform.position;
            float halfSegment = Mathf.Max(0f, (capsuleHeight * 0.5f) - capsuleRadius);
            Vector3 top = center + (Vector3.up * halfSegment);
            Vector3 bottom = center - (Vector3.up * halfSegment);
            Vector3 direction = displacement / distance;

            if (Physics.CapsuleCast(
                    top,
                    bottom,
                    capsuleRadius,
                    direction,
                    out RaycastHit hit,
                    distance + SkinWidth,
                    collisionMask,
                    QueryTriggerInteraction.Ignore))
            {
                distance = Mathf.Max(0f, hit.distance - SkinWidth);
            }

            transform.position += direction * distance;
        }
    }
}
