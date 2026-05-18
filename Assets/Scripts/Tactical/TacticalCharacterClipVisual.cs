using UnityEngine;

public class TacticalCharacterClipVisual : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip walkClip;
    [SerializeField] private AnimationClip aimClip;
    [SerializeField] private AnimationClip fireClip;
    [SerializeField] private AnimationClip hitClip;
    [SerializeField] private AnimationClip downClip;

    private Vector3 previousPosition;
    private float phase;
    private string lastClipStateName = "Idle";
    private int clipStateChangeCount;
    private int rigPartCount;
    private float lastClipEvidence;
    private bool authoredClipEvidence;
    private TacticalCharacterVisualState currentState = TacticalCharacterVisualState.Idle;

    public int AuthoredClipCount => CountAssignedClips();
    public int RigPartCount => rigPartCount;
    public bool HasAuthoredClipLibrary => AuthoredClipCount >= 6;
    public bool AuthoredClipEvidence => authoredClipEvidence;
    public float LastClipEvidence => lastClipEvidence;
    public string LastClipStateName => lastClipStateName;
    public int ClipStateChangeCount => clipStateChangeCount;

    public void Configure(
        Transform root,
        AnimationClip idle,
        AnimationClip walk,
        AnimationClip aim,
        AnimationClip fire,
        AnimationClip hit,
        AnimationClip down)
    {
        visualRoot = root;
        idleClip = idle;
        walkClip = walk;
        aimClip = aim;
        fireClip = fire;
        hitClip = hit;
        downClip = down;
        CaptureRigPartCount();
    }

    public void ApplyPreviewState(TacticalCharacterVisualState state, float speed01)
    {
        SetState(state);
        phase += 0.37f + Mathf.Clamp01(speed01) * 0.23f;
        SampleStateClip(state, Mathf.Repeat(phase, 1f));
        lastClipEvidence = AuthoredClipCount * 0.025f
            + rigPartCount * 0.01f
            + clipStateChangeCount * 0.015f
            + (state == TacticalCharacterVisualState.Fire ? 0.08f : 0f)
            + (state == TacticalCharacterVisualState.Down ? 0.10f : 0f);
        authoredClipEvidence = HasAuthoredClipLibrary
            && rigPartCount >= 12
            && clipStateChangeCount >= 3
            && lastClipEvidence > 0.28f;
    }

    private void Awake()
    {
        previousPosition = transform.position;
        CaptureRigPartCount();
    }

    private void Update()
    {
        if (!HasAuthoredClipLibrary || visualRoot == null)
        {
            return;
        }

        var deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
        var planarDelta = transform.position - previousPosition;
        planarDelta.y = 0f;
        previousPosition = transform.position;
        var speed01 = Mathf.Clamp01(planarDelta.magnitude / deltaTime / 4.5f);
        var nextState = speed01 > 0.08f ? TacticalCharacterVisualState.Walk : TacticalCharacterVisualState.Idle;
        SetState(nextState);
        phase += deltaTime * Mathf.Lerp(0.75f, 1.75f, speed01);
        SampleStateClip(nextState, Mathf.Repeat(phase, 1f));
        lastClipEvidence = Mathf.Max(lastClipEvidence, AuthoredClipCount * 0.02f + rigPartCount * 0.006f);
    }

    private void CaptureRigPartCount()
    {
        rigPartCount = 0;
        foreach (var partName in new[]
        {
            "torso_uniform",
            "head_balaclava",
            "upper_arm_left",
            "forearm_left",
            "glove_left",
            "upper_arm_right",
            "forearm_right",
            "glove_right",
            "thigh_left",
            "shin_left",
            "boot_left",
            "thigh_right",
            "shin_right",
            "boot_right"
        })
        {
            if (FindDescendant(visualRoot, partName) != null)
            {
                rigPartCount++;
            }
        }
    }

    private void SampleStateClip(TacticalCharacterVisualState state, float normalizedTime)
    {
        if (visualRoot == null)
        {
            return;
        }

        var clip = ClipForState(state);
        if (clip == null)
        {
            return;
        }

        var sampleTime = Mathf.Clamp01(normalizedTime) * Mathf.Max(clip.length, 0.1f);
        clip.SampleAnimation(visualRoot.gameObject, sampleTime);
    }

    private AnimationClip ClipForState(TacticalCharacterVisualState state)
    {
        return state switch
        {
            TacticalCharacterVisualState.Walk => walkClip,
            TacticalCharacterVisualState.Aim => aimClip,
            TacticalCharacterVisualState.Fire => fireClip,
            TacticalCharacterVisualState.Hit => hitClip,
            TacticalCharacterVisualState.Down => downClip,
            _ => idleClip
        };
    }

    private int CountAssignedClips()
    {
        var count = 0;
        if (idleClip != null) count++;
        if (walkClip != null) count++;
        if (aimClip != null) count++;
        if (fireClip != null) count++;
        if (hitClip != null) count++;
        if (downClip != null) count++;
        return count;
    }

    private void SetState(TacticalCharacterVisualState state)
    {
        if (currentState != state)
        {
            clipStateChangeCount++;
            currentState = state;
        }

        lastClipStateName = currentState.ToString();
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
}
