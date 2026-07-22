using System;
using UnityEngine;

namespace EchoShift.Presentation
{
    [Serializable]
    public struct SectionCameraBounds
    {
        [SerializeField] private Vector2 minimum;
        [SerializeField] private Vector2 maximum;

        public SectionCameraBounds(Vector2 min, Vector2 max)
        {
            minimum = min;
            maximum = max;
        }

        public Vector2 Minimum => minimum;
        public Vector2 Maximum => maximum;
        public bool IsValid => maximum.x > minimum.x && maximum.y > minimum.y;

        public Vector3 Clamp(Vector3 worldPosition, Vector3 sectionOrigin)
        {
            if (!IsValid) return worldPosition;
            Vector3 local = worldPosition - sectionOrigin;
            local.x = Mathf.Clamp(local.x, minimum.x, maximum.x);
            local.z = Mathf.Clamp(local.z, minimum.y, maximum.y);
            return sectionOrigin + local;
        }
    }

    [CreateAssetMenu(fileName = "Phase3CameraSettings", menuName = "ECHO SHIFT/Phase 3 Camera Settings")]
    public sealed class Phase3CameraSettings : ScriptableObject
    {
        [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -11f);
        [SerializeField] private Vector3 lookOffset = new Vector3(0f, 0f, 2.5f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.35f;
        [Header("Phase 4.2 framing")]
        [SerializeField, Range(0f, 6f)] private float lookAheadDistance = 2.6f;
        [SerializeField, Range(0f, 0.5f)] private float echoGroupCenterWeight = 0.2f;
        [SerializeField, Min(0f)] private float maximumGroupFocusOffset = 1.5f;
        [SerializeField, Min(0.01f)] private float zoomSmoothTime = 0.32f;
        [SerializeField, Range(30f, 70f)] private float baseFieldOfView = 48f;
        [SerializeField, Range(30f, 70f)] private float twoEchoFieldOfView = 52f;
        [SerializeField, Range(30f, 70f)] private float maximumFieldOfView = 54f;
        [SerializeField] private SectionCameraBounds[] sectionBounds = Array.Empty<SectionCameraBounds>();

        public Vector3 Offset => offset;
        public Vector3 LookOffset => lookOffset;
        public float SmoothTime => smoothTime;
        public float LookAheadDistance => lookAheadDistance;
        public float EchoGroupCenterWeight => echoGroupCenterWeight;
        public float MaximumGroupFocusOffset => maximumGroupFocusOffset;
        public float ZoomSmoothTime => zoomSmoothTime;
        public float BaseFieldOfView => baseFieldOfView;
        public float TwoEchoFieldOfView => twoEchoFieldOfView;
        public float MaximumFieldOfView => maximumFieldOfView;
        public int SectionBoundsCount => sectionBounds?.Length ?? 0;

        public SectionCameraBounds GetSectionBounds(int sectionNumber)
        {
            int index = sectionNumber - 1;
            return sectionBounds != null && index >= 0 && index < sectionBounds.Length
                ? sectionBounds[index]
                : default;
        }

        public Vector3 ClampFocus(int sectionNumber, Vector3 sectionOrigin, Vector3 focus) =>
            GetSectionBounds(sectionNumber).Clamp(focus, sectionOrigin);

        public void ConfigurePresentation(
            float forwardLookAhead,
            float groupCenterWeight,
            float groupFocusLimit,
            float fieldOfView,
            float twoEchoFov,
            float maximumFov,
            float zoomSeconds,
            SectionCameraBounds[] bounds)
        {
            lookAheadDistance = Mathf.Max(0f, forwardLookAhead);
            echoGroupCenterWeight = Mathf.Clamp(groupCenterWeight, 0f, 0.5f);
            maximumGroupFocusOffset = Mathf.Max(0f, groupFocusLimit);
            baseFieldOfView = Mathf.Clamp(fieldOfView, 30f, 70f);
            twoEchoFieldOfView = Mathf.Clamp(twoEchoFov, baseFieldOfView, 70f);
            maximumFieldOfView = Mathf.Clamp(maximumFov, twoEchoFieldOfView, 70f);
            zoomSmoothTime = Mathf.Max(0.01f, zoomSeconds);
            sectionBounds = bounds ?? Array.Empty<SectionCameraBounds>();
        }
    }
}
