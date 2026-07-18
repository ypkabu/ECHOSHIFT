using UnityEngine;

namespace EchoShift.Reset
{
    public sealed class TransformResettable : MonoBehaviour, IResettable
    {
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private bool _hasCapturedState;

        public bool HasCapturedState => _hasCapturedState;
        public Vector3 InitialPosition => _initialPosition;
        public Quaternion InitialRotation => _initialRotation;

        public void CaptureInitialState()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _hasCapturedState = true;
        }

        public void RestoreInitialState()
        {
            if (!_hasCapturedState)
            {
                return;
            }

            transform.SetPositionAndRotation(_initialPosition, _initialRotation);
        }
    }
}
