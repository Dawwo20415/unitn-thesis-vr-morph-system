using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public struct JointPrintPosition : IAnimationJob
{
    private HumanBodyBones bone;
    private TransformStreamHandle handle;
    private int n;

    public void SetupJob(Animator animator, HumanBodyBones b, int number)
    {
        n = number;
        bone = b;
        handle = animator.BindStreamTransform(animator.GetBoneTransform(b));
    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        Vector3 pos = handle.GetLocalPosition(stream);
        Quaternion rot = handle.GetLocalRotation(stream);

        Vector3 gpos = handle.GetPosition(stream);
        Quaternion grot = handle.GetRotation(stream);

        Debug.Log("Node [" + n + "] Bone [" + bone.ToString() + "] local{Pos " + VExtension.Print(pos) + " Rot " + QExtension.PrintEuler(rot) + "} global{Pos " + VExtension.Print(gpos) + " Rot " + QExtension.PrintEuler(grot) + "}");
    }
}

public struct ModifySingleBoneRotation : IAnimationJob
{
    private HumanBodyBones bone;
    private TransformStreamHandle handle;
    private TransformStreamHandle debughandle;
    private Quaternion modification;

    public void SetupJob(Animator animator, HumanBodyBones b, Quaternion q)
    {
        modification = q;
        bone = b;
        handle = animator.BindStreamTransform(animator.GetBoneTransform(b));
        debughandle = animator.BindStreamTransform(animator.GetBoneTransform(b+2));
    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        //Debug.Log("Pre-Rotation position " + VExtension.Print(debughandle.GetPosition(stream)));
        handle.SetLocalRotation(stream, handle.GetLocalRotation(stream) * modification);
        //Debug.Log("Post-Rotation position " + VExtension.Print(debughandle.GetPosition(stream)));
    }
}

public struct ModifySingleBonePosition : IAnimationJob
{
    private HumanBodyBones bone;
    private TransformStreamHandle handle;
    private Vector3 modification;

    public void SetupJob(Animator animator, HumanBodyBones b, Vector3 v)
    {
        modification = v;
        bone = b;
        handle = animator.BindStreamTransform(animator.GetBoneTransform(b));
    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        handle.SetPosition(stream, handle.GetPosition(stream) + modification);
        stream.AsHuman().SolveIK();
    }
}

[RequireComponent(typeof(Animator))]
public class TPosePlayableTest : MonoBehaviour
{
    public Quaternion mod;
    public Vector3 vec;

    //public List<Transform> targets_trn;
    public Transform LeftHandTrn;
    public Transform LeftLowerArmTrn;
    public Transform LeftUpperArmTrn;

    //GRAPH
    private Animator animator;
    private PlayableGraph graph;

    //TPOSE
    private ScriptPlayable<AvatarTPoseBehaviour> tposePlayable;
    private AvatarTPoseBehaviour tposeBehaviour;

    //POSE APPLY
    private PoseApplyJob poseApplyJob;
    private AnimationScriptPlayable animationPlayable;

    private ModifySingleBoneRotation singleJob;
    private AnimationScriptPlayable singlePlayable;

    //DEBUG PRINT
    private JointPrintPosition printJob;
    private AnimationScriptPlayable printPlayable;

    private JointPrintPosition printJob2;
    private AnimationScriptPlayable printPlayable2;

    //IK TARGET MODIFICATION
    //private ExtractJoint extractJob;
    //private AnimationScriptPlayable extractPlayable;

    //private StaticDisplacement displacementBehaviour;
    //private ScriptPlayable<StaticDisplacement> displacementPlayable;

    private IKTargetLink targetLeftHandJob;
    private IKTargetLink targetLeftLowerArmJob;
    private IKTargetLink targetLeftUpperArmJob;

    private AnimationScriptPlayable targetLeftHandPlayable;
    private AnimationScriptPlayable targetLeftLowerArmPlayable;
    private AnimationScriptPlayable targetLeftUpperArmPlayable;

    //IK NODE
    private AnimationGraphUtility.PlayableGraphIKChain playableIKGraph;

    //ANIMATION OUTPUT
    private AnimationPlayableOutput avatarPlayableOutput;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        graph = PlayableGraph.Create("Tpose-Test_" + UnityEngine.Random.Range(0.0f, 1.0f));

        //Nodes declaration
        tposePlayable = ScriptPlayable<AvatarTPoseBehaviour>.Create(graph);
        tposeBehaviour = tposePlayable.GetBehaviour();

        //displacementPlayable = ScriptPlayable<StaticDisplacement>.Create(graph);
        //displacementBehaviour = displacementPlayable.GetBehaviour();

        //Output Declaration
        avatarPlayableOutput = AnimationPlayableOutput.Create(graph, "Avatar Animation Output", animator);

        //Nodes setup
        tposeBehaviour.TPoseSetup(animator);

        //Animation
        poseApplyJob = new PoseApplyJob();
        poseApplyJob.Init(tposePlayable.GetBehaviour(), animator, true);
        animationPlayable = AnimationScriptPlayable.Create(graph, poseApplyJob);

        printJob = new JointPrintPosition();
        printJob.SetupJob(animator, HumanBodyBones.LeftLowerArm, 0);
        printPlayable = AnimationScriptPlayable.Create(graph, printJob);

        singleJob = new ModifySingleBoneRotation();
        singleJob.SetupJob(animator, HumanBodyBones.LeftUpperArm, QExtension.Fix(mod));
        singlePlayable = AnimationScriptPlayable.Create(graph, singleJob);

        printJob2 = new JointPrintPosition();
        printJob2.SetupJob(animator, HumanBodyBones.LeftLowerArm, 1);
        printPlayable2 = AnimationScriptPlayable.Create(graph, printJob2);

        //extractJob = new ExtractJoint();
        //extractJob.Setup(animator, HumanBodyBones.LeftHand);
        {
            targetLeftHandJob = new IKTargetLink();
            targetLeftHandJob.Setup(animator, LeftHandTrn);
            targetLeftLowerArmJob = new IKTargetLink();
            targetLeftLowerArmJob.Setup(animator, LeftLowerArmTrn);
            targetLeftUpperArmJob = new IKTargetLink();
            targetLeftUpperArmJob.Setup(animator, LeftUpperArmTrn);

            targetLeftHandPlayable = AnimationScriptPlayable.Create(graph, targetLeftHandJob);
            targetLeftLowerArmPlayable = AnimationScriptPlayable.Create(graph, targetLeftLowerArmJob);
            targetLeftUpperArmPlayable = AnimationScriptPlayable.Create(graph, targetLeftUpperArmJob);
        }
        List<HumanBodyBones> bones = new List<HumanBodyBones>();
        {
            bones.Add(HumanBodyBones.LeftHand);
            bones.Add(HumanBodyBones.LeftLowerArm);
            bones.Add(HumanBodyBones.LeftUpperArm);
            bones.Add(HumanBodyBones.LeftShoulder);
        }
        List<IKTarget> targets = new List<IKTarget>();
        {
            targets.Add(targetLeftHandJob);
            targets.Add(targetLeftLowerArmJob);
            targets.Add(targetLeftUpperArmJob);
        }

        playableIKGraph = new AnimationGraphUtility.PlayableGraphIKChain(graph, animator, bones, targets, "LeftArm");

        //Connections
        AnimationGraphUtility.ConnectNodes(graph, tposePlayable, animationPlayable);
        AnimationGraphUtility.ConnectNodes(graph, animationPlayable, playableIKGraph.output);
        {
            List<Playable> list = new List<Playable>(3);
            list.Add(targetLeftHandPlayable);
            list.Add(targetLeftLowerArmPlayable);
            list.Add(targetLeftUpperArmPlayable);
            AnimationGraphUtility.ConnectIKInputs(graph, list, playableIKGraph);
        }
        //AnimationGraphUtility.ConnectNodes(graph, displacementPlayable, IKPlayable);
        AnimationGraphUtility.ConnectOutput(playableIKGraph.output, avatarPlayableOutput);

        graph.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        poseApplyJob.Dispose();
        tposeBehaviour.Dispose();
        targetLeftHandJob.Dispose();
        targetLeftLowerArmJob.Dispose();
        targetLeftUpperArmJob.Dispose();
        //extractJob.Dispose();

        if (graph.IsValid())
        {
            graph.Stop();
            graph.Destroy();
        }
    }
}
