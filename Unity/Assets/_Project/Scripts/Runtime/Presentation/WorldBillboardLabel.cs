using UnityEngine;

namespace EchoShift.Presentation
{
    [DisallowMultipleComponent]
    public sealed class WorldBillboardLabel : MonoBehaviour
    {
        [SerializeField] private Quaternion worldRotation = Quaternion.identity;

        public Quaternion WorldRotation => worldRotation;

        public void Configure(Quaternion rotation)
        {
            worldRotation = rotation;
            transform.rotation = worldRotation;
        }

        private void LateUpdate()
        {
            transform.rotation = worldRotation;
        }
    }
}
