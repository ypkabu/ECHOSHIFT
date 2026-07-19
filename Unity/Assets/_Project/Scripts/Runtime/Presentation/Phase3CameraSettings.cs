using UnityEngine;

namespace EchoShift.Presentation
{
    [CreateAssetMenu(fileName = "Phase3CameraSettings", menuName = "ECHO SHIFT/Phase 3 Camera Settings")]
    public sealed class Phase3CameraSettings : ScriptableObject
    {
        [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -11f);
        [SerializeField] private Vector3 lookOffset = new Vector3(0f, 0f, 2.5f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.35f;

        public Vector3 Offset => offset;
        public Vector3 LookOffset => lookOffset;
        public float SmoothTime => smoothTime;
    }
}
