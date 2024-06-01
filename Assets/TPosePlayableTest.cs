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
    private Quaternion modification;

    public void SetupJob(Animator animator, HumanBodyBones b, Quaternion q)
    {
        modification = q;
        bone = b;
        handle = animator.BindStreamTransform(animator.GetBoneTransform(b));
    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        handle.SetLocalRotation(stream, handle.GetLocalRotation(stream) * modification);
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

    private ModifySingleBonePosition singlePosJob;
    private AnimationScriptPlayable singlePosPlayable;

    //DEBUG PRINT
    private JointPrintPosition printJob;
    private AnimationScriptPlayable printPlayable;

    private JointPrintPosition printJob2;
    private AnimationScriptPlayable printPlayable2;

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

        singlePosJob = new ModifySingleBonePosition();
        singlePosJob.SetupJob(animator, HumanBodyBones.LeftLowerArm, vec);
        singlePosPlayable = AnimationScriptPlayable.Create(graph, singlePosJob);

        printJob2 = new JointPrintPosition();
        printJob2.SetupJob(animator, HumanBodyBones.LeftLowerArm, 1);
        printPlayable2 = AnimationScriptPlayable.Create(graph, printJob2);

        //Connections
        AnimationGraphUtility.ConnectNodes(graph, tposePlayable, animationPlayable);
        AnimationGraphUtility.ConnectNodes(graph, animationPlayable, printPlayable);
        
        //Change Rotation
        AnimationGraphUtility.ConnectNodes(graph, printPlayable, singlePlayable);
        AnimationGraphUtility.ConnectNodes(graph, singlePlayable, printPlayable2);

        //Change Position as I suspected setting the position does basically nothing maybe if you do a run IK? NOPE
        //AnimationGraphUtility.ConnectNodes(graph, printPlayable, singlePosPlayable);
        //AnimationGraphUtility.ConnectNodes(graph, singlePosPlayable, printPlayable2);

        AnimationGraphUtility.ConnectOutput(printPlayable2, avatarPlayableOutput);

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

        if (graph.IsValid())
        {
            graph.Stop();
            graph.Destroy();
        }
    }
}
