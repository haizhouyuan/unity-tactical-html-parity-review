using UnityEngine;

public enum TacticalCharacterVisualState
{
    Idle,
    Walk,
    Aim,
    Fire,
    Hit,
    Down
}

public class TacticalCharacterMotionVisual : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float bobAmplitude = 0.035f;
    [SerializeField] private float swayDegrees = 3.0f;

    private Vector3 previousPosition;
    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private Vector3 baseLocalScale = Vector3.one;
    private float phase;
    private float hitPulse;
    private float lastMotionOffset;
    private float lastSpeed;
    private TacticalCharacterVisualState currentState = TacticalCharacterVisualState.Idle;
    private string lastStateName = "Idle";
    private int stateChangeCount;
    private float lastAnimationEvidence;
    private float lastLimbAnimationEvidence;
    private bool acceptedPlaceholderAnimationEvidence;
    private bool proceduralLimbAnimationEvidence;
    private TacticalPlayerController localPlayer;
    private int localFirstPersonHiddenRendererCount;
    private int proceduralLimbRigPartCount;
    private RigPose torso;
    private RigPose head;
    private RigPose upperArmLeft;
    private RigPose forearmLeft;
    private RigPose gloveLeft;
    private RigPose upperArmRight;
    private RigPose forearmRight;
    private RigPose gloveRight;
    private RigPose thighLeft;
    private RigPose shinLeft;
    private RigPose bootLeft;
    private RigPose thighRight;
    private RigPose shinRight;
    private RigPose bootRight;

    public bool HasVisualRoot => visualRoot != null;
    public float LastMotionOffset => lastMotionOffset;
    public float LastSpeed => lastSpeed;
    public string LastStateName => lastStateName;
    public int StateChangeCount => stateChangeCount;
    public float LastAnimationEvidence => lastAnimationEvidence;
    public float LastLimbAnimationEvidence => lastLimbAnimationEvidence;
    public bool AcceptedPlaceholderAnimationEvidence => acceptedPlaceholderAnimationEvidence;
    public bool ProceduralLimbAnimationEvidence => proceduralLimbAnimationEvidence;
    public int ProceduralLimbRigPartCount => proceduralLimbRigPartCount;
    public int LocalFirstPersonHiddenRendererCount => localFirstPersonHiddenRendererCount;

    public void Configure(Transform root)
    {
        visualRoot = root;
        CaptureBasePose();
    }

    public void PulseHit()
    {
        hitPulse = 1f;
        SetState(TacticalCharacterVisualState.Hit);
    }

    public void ApplyPreviewMotion(float speed01)
    {
        var speed = Mathf.Clamp01(speed01);
        SetState(speed > 0.2f ? TacticalCharacterVisualState.Walk : TacticalCharacterVisualState.Idle);
        ApplyMotion(speed, 0.12f);
    }

    public void ApplyPreviewState(TacticalCharacterVisualState state, float speed01)
    {
        SetState(state);
        ApplyMotion(Mathf.Clamp01(speed01), 0.12f);
    }

    private void Awake()
    {
        localPlayer = GetComponent<TacticalPlayerController>();
        previousPosition = transform.position;
        CaptureBasePose();
    }

    private void Update()
    {
        var deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
        var planarDelta = transform.position - previousPosition;
        planarDelta.y = 0f;
        previousPosition = transform.position;

        var speed01 = Mathf.Clamp01(planarDelta.magnitude / deltaTime / 4.5f);
        if (hitPulse <= 0.001f)
        {
            SetState(speed01 > 0.05f ? TacticalCharacterVisualState.Walk : TacticalCharacterVisualState.Idle);
        }

        ApplyMotion(speed01, deltaTime);
        UpdateLocalFirstPersonVisibility();
    }

    private void CaptureBasePose()
    {
        if (visualRoot == null)
        {
            return;
        }

        baseLocalPosition = visualRoot.localPosition;
        baseLocalRotation = visualRoot.localRotation;
        baseLocalScale = visualRoot.localScale;
        CaptureProceduralRig();
    }

    private void ApplyMotion(float speed01, float deltaTime)
    {
        if (visualRoot == null)
        {
            return;
        }

        lastSpeed = speed01;
        phase += deltaTime * Mathf.Lerp(3.2f, 8.5f, speed01);
        hitPulse = Mathf.MoveTowards(hitPulse, 0f, deltaTime * 5.2f);

        var stride = Mathf.Sin(phase);
        var stateWeight = StateMotionWeight(currentState, speed01);
        var bob = stride * bobAmplitude * stateWeight;
        var sway = stride * swayDegrees * stateWeight;
        var aimLean = currentState == TacticalCharacterVisualState.Aim || currentState == TacticalCharacterVisualState.Fire ? 0.055f : 0f;
        var fireKick = currentState == TacticalCharacterVisualState.Fire ? 0.045f : 0f;
        var downDrop = currentState == TacticalCharacterVisualState.Down ? -0.34f : 0f;
        var hitLean = currentState == TacticalCharacterVisualState.Hit ? -0.08f * hitPulse : 0f;
        var pulseScale = 1f + hitPulse * 0.075f;

        visualRoot.localPosition = baseLocalPosition + new Vector3(0f, bob + downDrop, aimLean - fireKick);
        visualRoot.localRotation = baseLocalRotation * Quaternion.Euler(hitLean, currentState == TacticalCharacterVisualState.Aim ? 5f : 0f, sway);
        visualRoot.localScale = baseLocalScale * pulseScale;
        ApplyProceduralLimbMotion(stride, speed01);
        lastMotionOffset = Mathf.Abs(bob) + Mathf.Abs(sway) * 0.01f + Mathf.Abs(aimLean) + Mathf.Abs(fireKick) + Mathf.Abs(downDrop) + hitPulse * 0.075f;
        lastAnimationEvidence = lastMotionOffset + stateChangeCount * 0.01f;
        acceptedPlaceholderAnimationEvidence = visualRoot != null && stateChangeCount >= 1 && lastAnimationEvidence > 0.02f;
        UpdateLocalFirstPersonVisibility();
    }

    private void CaptureProceduralRig()
    {
        torso = CapturePart("torso_uniform");
        head = CapturePart("head_balaclava");
        upperArmLeft = CapturePart("upper_arm_left");
        forearmLeft = CapturePart("forearm_left");
        gloveLeft = CapturePart("glove_left");
        upperArmRight = CapturePart("upper_arm_right");
        forearmRight = CapturePart("forearm_right");
        gloveRight = CapturePart("glove_right");
        thighLeft = CapturePart("thigh_left");
        shinLeft = CapturePart("shin_left");
        bootLeft = CapturePart("boot_left");
        thighRight = CapturePart("thigh_right");
        shinRight = CapturePart("shin_right");
        bootRight = CapturePart("boot_right");

        proceduralLimbRigPartCount = 0;
        foreach (var pose in new[] { torso, head, upperArmLeft, forearmLeft, gloveLeft, upperArmRight, forearmRight, gloveRight, thighLeft, shinLeft, bootLeft, thighRight, shinRight, bootRight })
        {
            if (pose != null)
            {
                proceduralLimbRigPartCount++;
            }
        }
    }

    private RigPose CapturePart(string partName)
    {
        var part = FindDescendant(visualRoot, partName);
        if (part == null)
        {
            return null;
        }

        return new RigPose(part);
    }

    private static Transform FindDescendant(Transform root, string partName)
    {
        if (root == null)
        {
            return null;
        }

        if (MatchesCharacterPart(root.name, partName))
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var found = FindDescendant(root.GetChild(i), partName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static bool MatchesCharacterPart(string nodeName, string partName)
    {
        if (nodeName == partName || nodeName.Contains(partName))
        {
            return true;
        }

        return partName switch
        {
            "torso_uniform" => nodeName.Contains("anatomical_torso_underlayer") || nodeName.Contains("front_plate_carrier"),
            "head_balaclava" => nodeName.Contains("human_face_visible_under_helmet") || nodeName.Contains("ballistic_helmet_shell"),
            "upper_arm_left" => nodeName.Contains("upper_arm_cloth_-0.39") || nodeName.Contains("shoulder_armor_pad_-0.39"),
            "forearm_left" => nodeName.Contains("forearm_glove_sleeve_-0.39"),
            "glove_left" => nodeName.Contains("black_tactical_glove_-0.39"),
            "upper_arm_right" => nodeName.Contains("upper_arm_cloth_0.39") || nodeName.Contains("shoulder_armor_pad_0.39"),
            "forearm_right" => nodeName.Contains("forearm_glove_sleeve_0.39"),
            "glove_right" => nodeName.Contains("black_tactical_glove_0.39"),
            "thigh_left" => nodeName.Contains("upper_leg_fatigues_-0.18"),
            "shin_left" => nodeName.Contains("lower_leg_boot_gaiter_-0.18"),
            "boot_left" => nodeName.Contains("boot_with_sculpted_sole_-0.18"),
            "thigh_right" => nodeName.Contains("upper_leg_fatigues_0.18"),
            "shin_right" => nodeName.Contains("lower_leg_boot_gaiter_0.18"),
            "boot_right" => nodeName.Contains("boot_with_sculpted_sole_0.18"),
            _ => false
        };
    }

    private void ApplyProceduralLimbMotion(float stride, float speed01)
    {
        if (proceduralLimbRigPartCount < 8)
        {
            lastLimbAnimationEvidence = 0f;
            proceduralLimbAnimationEvidence = false;
            return;
        }

        var walk = Mathf.Clamp01(speed01);
        var armSwing = stride * Mathf.Lerp(3f, 24f, walk);
        var legSwing = stride * Mathf.Lerp(2f, 20f, walk);
        var aim = currentState == TacticalCharacterVisualState.Aim || currentState == TacticalCharacterVisualState.Fire ? 1f : 0f;
        var fire = currentState == TacticalCharacterVisualState.Fire ? 1f : 0f;
        var down = currentState == TacticalCharacterVisualState.Down ? 1f : 0f;
        var hit = currentState == TacticalCharacterVisualState.Hit ? hitPulse : 0f;

        Apply(torso, new Vector3(-12f * down + 5f * hit, 4f * aim, stride * 2.5f * walk + 10f * down));
        Apply(head, new Vector3(8f * aim - 8f * down, 0f, -stride * 1.5f * walk));

        Apply(upperArmLeft, new Vector3(armSwing - 34f * aim - 38f * down, 0f, -18f * aim + 8f * hit));
        Apply(forearmLeft, new Vector3(-armSwing * 0.45f - 24f * aim, 0f, -10f * aim));
        Apply(gloveLeft, new Vector3(-armSwing * 0.18f - 10f * aim, 0f, 0f));

        Apply(upperArmRight, new Vector3(-armSwing - 42f * aim - 18f * fire - 38f * down, 0f, 18f * aim - 8f * hit));
        Apply(forearmRight, new Vector3(armSwing * 0.45f - 30f * aim - 12f * fire, 0f, 10f * aim));
        Apply(gloveRight, new Vector3(armSwing * 0.18f - 12f * aim - 10f * fire, 0f, 0f));

        Apply(thighLeft, new Vector3(-legSwing - 42f * down, 0f, 0f));
        Apply(shinLeft, new Vector3(Mathf.Max(0f, legSwing) * 0.65f + 26f * down, 0f, 0f));
        Apply(bootLeft, new Vector3(-Mathf.Max(0f, legSwing) * 0.28f, 0f, 0f));

        Apply(thighRight, new Vector3(legSwing - 42f * down, 0f, 0f));
        Apply(shinRight, new Vector3(Mathf.Max(0f, -legSwing) * 0.65f + 26f * down, 0f, 0f));
        Apply(bootRight, new Vector3(-Mathf.Max(0f, -legSwing) * 0.28f, 0f, 0f));

        lastLimbAnimationEvidence = Mathf.Abs(armSwing) * 0.01f
            + Mathf.Abs(legSwing) * 0.01f
            + aim * 0.18f
            + fire * 0.12f
            + down * 0.22f
            + hit * 0.08f;
        proceduralLimbAnimationEvidence = proceduralLimbRigPartCount >= 12 && lastLimbAnimationEvidence > 0.08f;
    }

    private static void Apply(RigPose pose, Vector3 localEulerOffset)
    {
        if (pose?.Part == null)
        {
            return;
        }

        pose.Part.localRotation = pose.BaseLocalRotation * Quaternion.Euler(localEulerOffset);
        pose.Part.localPosition = pose.BaseLocalPosition;
        pose.Part.localScale = pose.BaseLocalScale;
    }

    private void UpdateLocalFirstPersonVisibility()
    {
        localFirstPersonHiddenRendererCount = 0;
        if (localPlayer == null || visualRoot == null)
        {
            return;
        }

        var hideLocalBody = localPlayer.CameraMode == TacticalCameraMode.FirstPerson;
        foreach (var renderer in visualRoot.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = !hideLocalBody;
            if (hideLocalBody)
            {
                localFirstPersonHiddenRendererCount++;
            }
        }
    }

    private void SetState(TacticalCharacterVisualState state)
    {
        if (currentState == state)
        {
            lastStateName = currentState.ToString();
            return;
        }

        currentState = state;
        lastStateName = currentState.ToString();
        stateChangeCount++;
    }

    private static float StateMotionWeight(TacticalCharacterVisualState state, float speed01)
    {
        switch (state)
        {
            case TacticalCharacterVisualState.Walk:
                return Mathf.Max(0.35f, speed01);
            case TacticalCharacterVisualState.Aim:
                return Mathf.Max(0.18f, speed01 * 0.35f);
            case TacticalCharacterVisualState.Fire:
                return Mathf.Max(0.28f, speed01 * 0.45f);
            case TacticalCharacterVisualState.Hit:
                return Mathf.Max(0.2f, speed01);
            case TacticalCharacterVisualState.Down:
                return 0f;
            default:
                return speed01 * 0.18f;
        }
    }

    private sealed class RigPose
    {
        public readonly Transform Part;
        public readonly Vector3 BaseLocalPosition;
        public readonly Quaternion BaseLocalRotation;
        public readonly Vector3 BaseLocalScale;

        public RigPose(Transform part)
        {
            Part = part;
            BaseLocalPosition = part.localPosition;
            BaseLocalRotation = part.localRotation;
            BaseLocalScale = part.localScale;
        }
    }
}
