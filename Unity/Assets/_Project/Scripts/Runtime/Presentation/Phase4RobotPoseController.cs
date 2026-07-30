using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Replay;
using Unity.Profiling;
using UnityEngine;

namespace EchoShift.Presentation
{
    public enum Phase4RobotPoseState : byte
    {
        Idle,
        Walk,
        CarryIdle,
        CarryWalk,
        Interact,
        EchoStopped
    }

    [DisallowMultipleComponent]
    public sealed class Phase4RobotPoseController : MonoBehaviour
    {
        public const string ProfilerMarkerName = "EchoShift.Animator.Update";
        public const float DefaultInteractionDuration = 0.32f;
        public const float DefaultBlendDuration = 0.12f;

        private static readonly ProfilerMarker UpdateMarker = new ProfilerMarker(ProfilerMarkerName);
        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int WalkState = Animator.StringToHash("Walk");
        private static readonly int CarryIdleState = Animator.StringToHash("Carry Idle");
        private static readonly int CarryWalkState = Animator.StringToHash("Carry Walk");
        private static readonly int InteractState = Animator.StringToHash("Interact");
        private static readonly int EchoStoppedState = Animator.StringToHash("Echo Stopped");

        [SerializeField] private Transform gameplayRoot;
        [SerializeField] private LoopActor actor;
        [SerializeField] private PlayerSimulation player;
        [SerializeField] private EchoPlayback playback;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform upperArmLeft;
        [SerializeField] private Transform lowerArmLeft;
        [SerializeField] private Transform handLeft;
        [SerializeField] private Transform upperArmRight;
        [SerializeField] private Transform lowerArmRight;
        [SerializeField] private Transform handRight;
        [SerializeField] private Transform upperLegLeft;
        [SerializeField] private Transform lowerLegLeft;
        [SerializeField] private Transform upperLegRight;
        [SerializeField] private Transform lowerLegRight;
        [SerializeField] private Transform torso;
        [SerializeField] private Transform head;
        [SerializeField] private GameObject carryPoseVisualRoot;
        [SerializeField] private Transform carryArmLeft;
        [SerializeField] private Transform carryForearmLeft;
        [SerializeField] private Transform carryArmRight;
        [SerializeField] private Transform carryForearmRight;
        [SerializeField] private Transform carryHandLeft;
        [SerializeField] private Transform carryHandRight;
        [SerializeField] private Renderer[] standardArmRenderers = System.Array.Empty<Renderer>();
        [SerializeField, Min(0.05f)] private float blendDuration = DefaultBlendDuration;
        [SerializeField, Min(0.1f)] private float interactionDuration = DefaultInteractionDuration;

        private Quaternion _upperArmLeftRest;
        private Quaternion _lowerArmLeftRest;
        private Quaternion _upperArmRightRest;
        private Quaternion _lowerArmRightRest;
        private Quaternion _upperLegLeftRest;
        private Quaternion _lowerLegLeftRest;
        private Quaternion _upperLegRightRest;
        private Quaternion _lowerLegRightRest;
        private Quaternion _torsoRest;
        private Quaternion _headRest;
        private Vector3 _previousRootPosition;
        private float _walkPhase;
        private float _interactionRemaining;
        private int _lastInteractionSuccess;
        private bool _capturedRest;
        private bool _armVisibilityInitialized;
        private bool _proceduralArmsVisible;
        private bool _hasAppliedPose;
        private float _poseBlendRemaining;

        public Phase4RobotPoseState CurrentState { get; private set; }
        public bool HasRequiredReferences => gameplayRoot != null && actor != null &&
            animator != null && upperArmLeft != null && lowerArmLeft != null &&
            handLeft != null && upperArmRight != null && lowerArmRight != null &&
            handRight != null && upperLegLeft != null &&
            lowerLegLeft != null && upperLegRight != null && lowerLegRight != null &&
            torso != null && head != null && carryPoseVisualRoot != null &&
            carryArmLeft != null && carryArmRight != null && carryHandLeft != null &&
            carryForearmLeft != null && carryForearmRight != null &&
            carryHandRight != null && standardArmRenderers != null &&
            standardArmRenderers.Length >= 4;
        public bool IsRootMotionDisabled => animator != null && !animator.applyRootMotion;
        public bool IsVisualOnly => HasRequiredReferences && gameplayRoot != transform &&
            transform.IsChildOf(gameplayRoot) && upperArmLeft.IsChildOf(transform) &&
            upperArmRight.IsChildOf(transform) && upperLegLeft.IsChildOf(transform) &&
            upperLegRight.IsChildOf(transform);
        public float BlendDuration => blendDuration;
        public float InteractionDuration => interactionDuration;
        public Animator Animator => animator;
        public bool HasCarryPoseVisuals => carryPoseVisualRoot != null && carryArmLeft != null &&
            carryForearmLeft != null && carryArmRight != null && carryForearmRight != null &&
            carryHandLeft != null && carryHandRight != null;
        public Transform LeftPresentationHand => carryHandLeft;
        public Transform RightPresentationHand => carryHandRight;

        public void Configure(
            Transform root, LoopActor loopActor, PlayerSimulation playerSimulation,
            EchoPlayback echoPlayback, Animator visualAnimator,
            Transform leftUpperArm, Transform leftLowerArm, Transform leftHand,
            Transform rightUpperArm, Transform rightLowerArm, Transform rightHand,
            Transform leftUpperLeg, Transform leftLowerLeg,
            Transform rightUpperLeg, Transform rightLowerLeg,
            Transform torsoBone, Transform headBone)
        {
            gameplayRoot = root;
            actor = loopActor;
            player = playerSimulation;
            playback = echoPlayback;
            animator = visualAnimator;
            upperArmLeft = leftUpperArm;
            lowerArmLeft = leftLowerArm;
            handLeft = leftHand;
            upperArmRight = rightUpperArm;
            lowerArmRight = rightLowerArm;
            handRight = rightHand;
            upperLegLeft = leftUpperLeg;
            lowerLegLeft = leftLowerLeg;
            upperLegRight = rightUpperLeg;
            lowerLegRight = rightLowerLeg;
            torso = torsoBone;
            head = headBone;
            if (animator != null)
            {
                animator.applyRootMotion = false;
                // The imported FBX has no usable clips. Keep the controller as the
                // explicit semantic state map, while the allocation-free procedural
                // pose below owns runtime bone presentation.
                animator.enabled = false;
            }
            CaptureRestPose();
        }

        public void ConfigureCarryVisuals(
            GameObject visualRoot, Transform leftArmVisual, Transform leftForearmVisual,
            Transform rightArmVisual, Transform rightForearmVisual,
            Transform leftHandVisual, Transform rightHandVisual, Renderer[] armRenderers)
        {
            carryPoseVisualRoot = visualRoot;
            carryArmLeft = leftArmVisual;
            carryForearmLeft = leftForearmVisual;
            carryArmRight = rightArmVisual;
            carryForearmRight = rightForearmVisual;
            carryHandLeft = leftHandVisual;
            carryHandRight = rightHandVisual;
            standardArmRenderers = armRenderers ?? System.Array.Empty<Renderer>();
            CaptureRestPose();
            SetCarryVisualActive(true);
        }

        private void Awake()
        {
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.enabled = false;
            }
            CaptureRestPose();
        }

        private void OnEnable()
        {
            CaptureRestPose();
            _previousRootPosition = gameplayRoot != null ? gameplayRoot.position : transform.position;
            _walkPhase = 0f;
            _interactionRemaining = 0f;
            _lastInteractionSuccess = GetInteractionSuccessCount();
            SetState(Phase4RobotPoseState.Idle, true);
            // Echoes can be queried by camera/readability gates in the frame they
            // are instantiated, before their first LateUpdate. Seed every
            // procedural segment immediately so it never exposes builder-time
            // placeholder transforms at the world origin.
            if (HasRequiredReferences)
            {
                ApplyPose(Phase4RobotPoseState.Idle, 1f, 0f);
                _hasAppliedPose = true;
            }
        }

        private void LateUpdate()
        {
            using (UpdateMarker.Auto())
            {
                Evaluate(Time.unscaledDeltaTime);
            }
        }

        public void RefreshNowForTests()
        {
            CaptureRestPose();
            Evaluate(1f / 60f);
        }

        public void ForcePoseForCapture(Phase4RobotPoseState state)
        {
            CaptureRestPose();
            SetState(state, true);
            ApplyPose(state, 1f, state == Phase4RobotPoseState.Walk ||
                state == Phase4RobotPoseState.CarryWalk ? 0.72f : 0f);
            _hasAppliedPose = true;
        }

        private void Evaluate(float deltaTime)
        {
            if (!HasRequiredReferences) return;
            Vector3 position = gameplayRoot.position;
            Vector3 delta = position - _previousRootPosition;
            delta.y = 0f;
            float distance = delta.magnitude;
            _previousRootPosition = position;
            bool moving = distance > 0.0005f && distance < 0.8f;
            if (moving) _walkPhase += distance * 7.5f;

            int interactionSuccess = GetInteractionSuccessCount();
            if (interactionSuccess > _lastInteractionSuccess)
                _interactionRemaining = interactionDuration;
            _lastInteractionSuccess = interactionSuccess;
            _interactionRemaining = Mathf.Max(0f, _interactionRemaining - deltaTime);

            Interactor interactor = player != null ? player.Interactor : playback?.Interactor;
            bool carrying = interactor != null && interactor.CarriedBattery != null;
            bool stopped = playback != null && playback.PlaybackTick >= playback.RecordingLength;
            Phase4RobotPoseState state = _interactionRemaining > 0f
                ? Phase4RobotPoseState.Interact
                : stopped
                    ? Phase4RobotPoseState.EchoStopped
                    : carrying
                        ? moving ? Phase4RobotPoseState.CarryWalk : Phase4RobotPoseState.CarryIdle
                        : moving ? Phase4RobotPoseState.Walk : Phase4RobotPoseState.Idle;
            SetState(state, false);
            float response = Mathf.Clamp01(deltaTime / Mathf.Max(0.01f, blendDuration));
            bool dynamicPose = state == Phase4RobotPoseState.Walk ||
                               state == Phase4RobotPoseState.CarryWalk ||
                               state == Phase4RobotPoseState.Interact;
            if (dynamicPose || !_hasAppliedPose || _poseBlendRemaining > 0f)
            {
                ApplyPose(state, response, Mathf.Sin(_walkPhase));
                _hasAppliedPose = true;
                _poseBlendRemaining = Mathf.Max(0f, _poseBlendRemaining - deltaTime);
            }
        }

        private int GetInteractionSuccessCount() => player != null
            ? player.InteractionSuccessCount
            : playback != null ? playback.InteractionSuccessCount : 0;

        private void SetState(Phase4RobotPoseState state, bool immediate)
        {
            if (!immediate && CurrentState == state) return;
            CurrentState = state;
            _hasAppliedPose = false;
            _poseBlendRemaining = immediate ? 0f : blendDuration;
            if (animator != null && animator.runtimeAnimatorController != null && animator.isActiveAndEnabled)
                animator.CrossFade(GetStateHash(state), immediate ? 0f : blendDuration);
        }

        private void ApplyPose(Phase4RobotPoseState state, float response, float gait)
        {
            float leftArmForward = 0f;
            float rightArmForward = 0f;
            float leftArmDown = 68f;
            float rightArmDown = -68f;
            float leftElbow = 6f;
            float rightElbow = -6f;
            float leftLeg = 0f;
            float rightLeg = 0f;
            float leftKnee = 0f;
            float rightKnee = 0f;
            float torsoLean = 0f;
            float headTilt = 0f;

            if (state == Phase4RobotPoseState.Walk)
            {
                leftArmForward = gait * 23f;
                rightArmForward = -gait * 23f;
                leftLeg = -gait * 27f;
                rightLeg = gait * 27f;
                leftKnee = Mathf.Max(0f, gait) * 18f;
                rightKnee = Mathf.Max(0f, -gait) * 18f;
                torsoLean = 4f;
            }
            else if (state == Phase4RobotPoseState.CarryIdle ||
                     state == Phase4RobotPoseState.CarryWalk)
            {
                leftArmDown = 43f;
                rightArmDown = -43f;
                leftArmForward = -48f;
                rightArmForward = -48f;
                leftElbow = -62f;
                rightElbow = -62f;
                if (state == Phase4RobotPoseState.CarryWalk)
                {
                    leftLeg = -gait * 19f;
                    rightLeg = gait * 19f;
                    leftKnee = Mathf.Max(0f, gait) * 12f;
                    rightKnee = Mathf.Max(0f, -gait) * 12f;
                    torsoLean = 3f;
                }
            }
            else if (state == Phase4RobotPoseState.Interact)
            {
                leftArmDown = 60f;
                rightArmDown = -32f;
                rightArmForward = -68f;
                rightElbow = -38f;
                torsoLean = 5f;
            }
            else if (state == Phase4RobotPoseState.EchoStopped)
            {
                leftArmDown = 52f;
                rightArmDown = -78f;
                leftArmForward = -16f;
                rightArmForward = 12f;
                leftElbow = -24f;
                rightElbow = -12f;
                torsoLean = -6f;
                headTilt = 12f;
            }

            Apply(upperArmLeft, _upperArmLeftRest, Quaternion.identity, response);
            Apply(lowerArmLeft, _lowerArmLeftRest, Quaternion.identity, response);
            Apply(upperArmRight, _upperArmRightRest, Quaternion.identity, response);
            Apply(lowerArmRight, _lowerArmRightRest, Quaternion.identity, response);
            Apply(upperLegLeft, _upperLegLeftRest, Quaternion.Euler(leftLeg, 0f, 0f), response);
            Apply(lowerLegLeft, _lowerLegLeftRest, Quaternion.Euler(leftKnee, 0f, 0f), response);
            Apply(upperLegRight, _upperLegRightRest, Quaternion.Euler(rightLeg, 0f, 0f), response);
            Apply(lowerLegRight, _lowerLegRightRest, Quaternion.Euler(rightKnee, 0f, 0f), response);
            Apply(torso, _torsoRest, Quaternion.Euler(torsoLean, 0f, 0f), response);
            Apply(head, _headRest, Quaternion.Euler(0f, 0f, headTilt), response);
            ApplyArmTargets(state, gait, response,
                leftArmForward, rightArmForward, leftArmDown, rightArmDown,
                leftElbow, rightElbow);
        }

        private void ApplyArmTargets(
            Phase4RobotPoseState state, float gait, float response,
            float leftForward, float rightForward, float leftDown, float rightDown,
            float leftElbow, float rightElbow)
        {
            float leftSide = Mathf.Sign(
                transform.InverseTransformPoint(upperArmLeft.position).x);
            float rightSide = Mathf.Sign(
                transform.InverseTransformPoint(upperArmRight.position).x);
            Vector3 leftTarget = DefaultHandTarget(leftSide, gait, state, true);
            Vector3 rightTarget = DefaultHandTarget(rightSide, gait, state, false);
            if (state == Phase4RobotPoseState.CarryIdle ||
                state == Phase4RobotPoseState.CarryWalk)
            {
                Interactor interactor = player != null ? player.Interactor : playback?.Interactor;
                Transform socket = interactor != null ? interactor.CarrySocket : null;
                if (socket != null)
                {
                    Vector3 center = socket.position;
                    leftTarget = center + gameplayRoot.right * (leftSide * 0.22f) +
                        gameplayRoot.up * 0.015f;
                    rightTarget = center + gameplayRoot.right * (rightSide * 0.22f) +
                        gameplayRoot.up * 0.015f;
                }
            }
            bool carrying = state == Phase4RobotPoseState.CarryIdle ||
                            state == Phase4RobotPoseState.CarryWalk;
            SetCarryVisualActive(true);
            ConfigurePresentationArm(upperArmLeft.position, leftTarget, leftSide,
                carrying, carryArmLeft, carryForearmLeft, carryHandLeft);
            ConfigurePresentationArm(upperArmRight.position, rightTarget, rightSide,
                carrying, carryArmRight, carryForearmRight, carryHandRight);
            SolveArm(upperArmLeft, lowerArmLeft, handLeft, leftTarget, leftSide, response);
            SolveArm(upperArmRight, lowerArmRight, handRight, rightTarget, rightSide, response);
        }

        private void ConfigurePresentationArm(
            Vector3 shoulder, Vector3 handTarget, float side, bool carrying,
            Transform upperVisual, Transform lowerVisual, Transform handVisual)
        {
            Vector3 elbow = Vector3.Lerp(shoulder, handTarget, 0.52f) +
                gameplayRoot.right * (side * (carrying ? 0.16f : 0.11f)) +
                gameplayRoot.up * 0.035f - gameplayRoot.forward * 0.06f;
            ConfigureCarrySegment(upperVisual, shoulder, elbow);
            ConfigureCarrySegment(lowerVisual, elbow, handTarget);
            handVisual.position = handTarget;
        }

        private void SetCarryVisualActive(bool active)
        {
            if (_armVisibilityInitialized && _proceduralArmsVisible == active) return;
            _armVisibilityInitialized = true;
            _proceduralArmsVisible = active;
            if (carryPoseVisualRoot != null && carryPoseVisualRoot.activeSelf != active)
                carryPoseVisualRoot.SetActive(active);
            for (int i = 0; i < standardArmRenderers.Length; i++)
                if (standardArmRenderers[i] != null) standardArmRenderers[i].enabled = !active;
        }

        private void ConfigureCarrySegment(Transform segment, Vector3 start, Vector3 end)
        {
            Vector3 direction = end - start;
            float length = direction.magnitude;
            segment.position = (start + end) * 0.5f;
            segment.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            Vector3 parentScale = segment.parent.lossyScale;
            segment.localScale = new Vector3(
                0.095f / Mathf.Max(0.001f, Mathf.Abs(parentScale.x)),
                length * 0.5f / Mathf.Max(0.001f, Mathf.Abs(parentScale.y)),
                0.095f / Mathf.Max(0.001f, Mathf.Abs(parentScale.z)));
        }

        private Vector3 DefaultHandTarget(
            float side, float gait, Phase4RobotPoseState state, bool left)
        {
            Vector3 local = new Vector3(side * 0.5f, -0.34f, 0.12f);
            if (state == Phase4RobotPoseState.Walk)
            {
                float swing = left ? gait : -gait;
                local.y += Mathf.Abs(swing) * 0.06f;
                local.z += swing * 0.28f;
            }
            else if (state == Phase4RobotPoseState.Interact)
            {
                local = left
                    ? new Vector3(side * 0.48f, -0.26f, 0.16f)
                    : new Vector3(side * 0.28f, 0.12f, 0.78f);
            }
            else if (state == Phase4RobotPoseState.EchoStopped)
            {
                local = left
                    ? new Vector3(side * 0.44f, -0.38f, 0.02f)
                    : new Vector3(side * 0.24f, -0.04f, 0.36f);
            }
            return gameplayRoot.TransformPoint(local);
        }

        private void SolveArm(
            Transform upper, Transform lower, Transform hand,
            Vector3 handTarget, float side, float response)
        {
            Vector3 shoulder = upper.position;
            float upperLength = Vector3.Distance(shoulder, lower.position);
            float lowerLength = Vector3.Distance(lower.position, hand.position);
            Vector3 toTarget = handTarget - shoulder;
            float distance = Mathf.Clamp(toTarget.magnitude, 0.02f,
                Mathf.Max(0.02f, upperLength + lowerLength - 0.01f));
            Vector3 direction = toTarget.normalized;
            float along = (distance * distance + upperLength * upperLength -
                           lowerLength * lowerLength) / (2f * distance);
            float height = Mathf.Sqrt(Mathf.Max(0f,
                upperLength * upperLength - along * along));
            Vector3 bendReference = (gameplayRoot.right * side +
                                     gameplayRoot.forward * 0.34f).normalized;
            Vector3 bend = Vector3.ProjectOnPlane(bendReference, direction).normalized;
            if (bend.sqrMagnitude < 0.01f) bend = gameplayRoot.up;
            Vector3 elbowTarget = shoulder + direction * along + bend * height;
            AimBone(upper, lower.position - shoulder, elbowTarget - shoulder, response);
            AimBone(lower, hand.position - lower.position,
                handTarget - lower.position, response);
        }

        private static void AimBone(
            Transform bone, Vector3 currentDirection, Vector3 targetDirection, float response)
        {
            if (currentDirection.sqrMagnitude < 0.000001f ||
                targetDirection.sqrMagnitude < 0.000001f) return;
            Quaternion target = Quaternion.FromToRotation(
                currentDirection, targetDirection) * bone.rotation;
            bone.rotation = Quaternion.Slerp(bone.rotation, target, response);
        }

        private static void Apply(
            Transform bone, Quaternion rest, Quaternion offset, float response)
        {
            bone.localRotation = Quaternion.Slerp(bone.localRotation, rest * offset, response);
        }

        private void CaptureRestPose()
        {
            if (_capturedRest || !HasRequiredReferences) return;
            _upperArmLeftRest = upperArmLeft.localRotation;
            _lowerArmLeftRest = lowerArmLeft.localRotation;
            _upperArmRightRest = upperArmRight.localRotation;
            _lowerArmRightRest = lowerArmRight.localRotation;
            _upperLegLeftRest = upperLegLeft.localRotation;
            _lowerLegLeftRest = lowerLegLeft.localRotation;
            _upperLegRightRest = upperLegRight.localRotation;
            _lowerLegRightRest = lowerLegRight.localRotation;
            _torsoRest = torso.localRotation;
            _headRest = head.localRotation;
            _capturedRest = true;
        }

        private static int GetStateHash(Phase4RobotPoseState state)
        {
            switch (state)
            {
                case Phase4RobotPoseState.Walk: return WalkState;
                case Phase4RobotPoseState.CarryIdle: return CarryIdleState;
                case Phase4RobotPoseState.CarryWalk: return CarryWalkState;
                case Phase4RobotPoseState.Interact: return InteractState;
                case Phase4RobotPoseState.EchoStopped: return EchoStoppedState;
                default: return IdleState;
            }
        }
    }
}
