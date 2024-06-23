using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

public class OptitrackRetargetingAvatar : MonoBehaviour
{
    [Header("Optitrack Stuff")]
    public PlayableOptitrackStreamingClient client;
    public string skeleton_name;
    public Transform retargeting_root;

    [Space]
    [Header("Egocentric Stuff")]
    public Material material;
    public Mesh capsule_mesh;
    [Range(0.0f, 0.3f)]
    public float capsule_thickness;
    public List<CustomAvatarCalibrationMesh> calMeshes;
    public EgocentricRayCasterSource.DebugStruct egoDebug;

    private Animator animator;
    private Avatar avatar;
    private PlayableGraph graph;

    private PlayableOptitrackGraph optitrackGraph;
    private AnimationPlayableOutput avatarOutput;

    private IKTargetPipeline IKPipelineHand;
    private IKTargetPipeline IKPipelineLowerArm;
    private IKTargetPipeline IKPipelineUpperArm;

    private AnimationGraphUtility.PlayableGraphIKChain playableIKGraph;

    private EgocentricSelfContact egocetric;

    NormalMatchingBehaviour m_behaviour;
    NativeArray<Quaternion> m_NativeQuaternion;

    // Start is called before the first frame update
    void Start()
    {
        m_NativeQuaternion = new NativeArray<Quaternion>(1, Allocator.Persistent);
        animator = GetComponent<Animator>();
        avatar = animator.avatar;

        graph = PlayableGraph.Create("Optitrack Test2_" + UnityEngine.Random.Range(0.0f, 1.0f));

        try
        {
            optitrackGraph = new PlayableOptitrackGraph(graph, client, skeleton_name, animator, retargeting_root, true, true);
        } catch (UnityException e)
        {
            Debug.LogError(e.ToString());
            this.enabled = false;
            return;
        }

        egocetric = new EgocentricSelfContact(optitrackGraph.animator, animator, graph, material, capsule_mesh, capsule_thickness, calMeshes, new List<HumanBodyBones>() { HumanBodyBones.LeftHand }, egoDebug);

        IKPipelineHand = new IKTargetPipeline(HumanBodyBones.LeftHand);
        {
            m_behaviour = new NormalMatchingBehaviour();
            m_behaviour.Setup(HumanBodyBones.LeftHand, m_NativeQuaternion);
            IKPipelineHand.AppendBehaviour(graph, m_behaviour);
            /*
            ExtractBone ex = new ExtractBone();
            ex.setup(animator, HumanBodyBones.LeftHand);
            IKPipelineHand.AppendJob(graph, ex);
            */
            IKPipelineHand.AddEgocentric(graph, egocetric);

            /*
            StaticDisplacement dis = new StaticDisplacement();
            dis.Setup(((ScriptPlayable<EgocentricBehaviour>)egocetric[IKPipelineHand.bone]).GetBehaviour(), new Vector3(0.0f, 0.0f, 0.0f));
            IKPipelineHand.AppendBehaviour(graph, dis);
            */
        }

        IKPipelineLowerArm = new IKTargetPipeline(HumanBodyBones.LeftLowerArm);
        {
            ExtractBone ex = new ExtractBone();
            ex.setup(animator, HumanBodyBones.LeftLowerArm);
            IKPipelineLowerArm.AppendJob(graph, ex);

            StaticDisplacement dis = new StaticDisplacement();
            dis.Setup(ex, new Vector3(0.0f, 0.0f, 0.0f));
            IKPipelineLowerArm.AppendBehaviour(graph, dis);
        }

        IKPipelineUpperArm = new IKTargetPipeline(HumanBodyBones.LeftUpperArm);
        {
            ExtractBone ex = new ExtractBone();
            ex.setup(animator, HumanBodyBones.LeftUpperArm);
            IKPipelineUpperArm.AppendJob(graph, ex);

            StaticDisplacement dis = new StaticDisplacement();
            dis.Setup(ex, new Vector3(0.0f, 0.0f, 0.0f));
            IKPipelineUpperArm.AppendBehaviour(graph, dis);
        }

        List<HumanBodyBones> list = new List<HumanBodyBones>(){ HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftShoulder };
        List<IKTarget> targets = new List<IKTarget>() { IKPipelineHand.lastNode, IKPipelineLowerArm.lastNode, IKPipelineUpperArm.lastNode };
        playableIKGraph = new AnimationGraphUtility.PlayableGraphIKChain(graph, animator, list, targets, "Left Arm");

        avatarOutput = AnimationPlayableOutput.Create(graph, "Avatar Output", animator);

        //CONNECT NODES
        AnimationGraphUtility.ConnectNodes(graph, optitrackGraph.retargeted, IKPipelineHand.firstPlayable);
        AnimationGraphUtility.ConnectNodes(graph, optitrackGraph.retargeted, IKPipelineLowerArm.firstPlayable);
        AnimationGraphUtility.ConnectNodes(graph, optitrackGraph.retargeted, IKPipelineUpperArm.firstPlayable);

        List<Playable> pl = new List<Playable>() { IKPipelineHand.lastPlayable, IKPipelineLowerArm.lastPlayable, IKPipelineUpperArm.lastPlayable };
        AnimationGraphUtility.ConnectIKInputs(graph, pl, playableIKGraph);

        {
            //playableIKGraph.dummy.DisconnectInput(0);
            //AnimationGraphUtility.ConnectOutput(playableIKGraph[0], egocetric.output(0));
            //Disconnect
            graph.DestroyOutput(playableIKGraph.targetOutput);
            AnimationGraphUtility.ConnectOutput(playableIKGraph.dummy, egocetric.output(0));
        }

        AnimationGraphUtility.ConnectNodes(graph, optitrackGraph.retargeted, playableIKGraph.output);

#if true
        { //Hand Rotation Stuff
            
            NormalMatchingJob job1 = new NormalMatchingJob();
            job1.Setup(animator, HumanBodyBones.LeftHand, m_NativeQuaternion);
            AnimationScriptPlayable apl = AnimationScriptPlayable.Create(graph, job1);

            AnimationGraphUtility.ConnectNodes(graph, playableIKGraph.output, apl);
            AnimationGraphUtility.ConnectOutput(apl, avatarOutput);
            
        }
#else
        //AnimationGraphUtility.ConnectOutput(playableIKGraph.output, avatarOutput);
#endif
        graph.Play();
    }

    private void OnDisable()
    {
        m_NativeQuaternion.Dispose();
        if (graph.IsValid())
        {
            graph.Stop();
            graph.Destroy();
        }
    }
}
