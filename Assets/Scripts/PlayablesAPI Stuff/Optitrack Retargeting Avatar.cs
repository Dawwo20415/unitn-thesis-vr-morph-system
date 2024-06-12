using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class OptitrackRetargetingAvatar : MonoBehaviour
{
    public PlayableOptitrackStreamingClient client;
    public string skeleton_name;
    public Transform retargeting_root;

    private Animator animator;
    private Avatar avatar;
    private PlayableGraph graph;

    private PlayableOptitrackGraph optitrackGraph;
    private AnimationPlayableOutput avatarOutput;

    private IKTargetPipeline IKPipelineHand;
    private IKTargetPipeline IKPipelineLowerArm;
    private IKTargetPipeline IKPipelineUpperArm;

    private AnimationGraphUtility.PlayableGraphIKChain playableIKGraph;

    // Start is called before the first frame update
    void Start()
    {
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

        IKPipelineHand = new IKTargetPipeline();
        {
            ExtractBone ex = new ExtractBone();
            ex.setup(animator, HumanBodyBones.LeftHand);
            IKPipelineHand.AppendJob(graph, ex);

            StaticDisplacement dis = new StaticDisplacement();
            dis.Setup(ex, new Vector3(0.0f, 0.0f, 0.2f));
            IKPipelineHand.AppendBehaviour(graph, dis);
        }

        IKPipelineLowerArm = new IKTargetPipeline();
        {
            ExtractBone ex = new ExtractBone();
            ex.setup(animator, HumanBodyBones.LeftLowerArm);
            IKPipelineLowerArm.AppendJob(graph, ex);

            StaticDisplacement dis = new StaticDisplacement();
            dis.Setup(ex, new Vector3(0.0f, 0.2f, 0.0f));
            IKPipelineLowerArm.AppendBehaviour(graph, dis);
        }

        IKPipelineUpperArm = new IKTargetPipeline();
        {
            ExtractBone ex = new ExtractBone();
            ex.setup(animator, HumanBodyBones.LeftUpperArm);
            IKPipelineUpperArm.AppendJob(graph, ex);

            StaticDisplacement dis = new StaticDisplacement();
            dis.Setup(ex, new Vector3(0.0f, 0.2f, 0.0f));
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

        AnimationGraphUtility.ConnectNodes(graph, optitrackGraph.retargeted, playableIKGraph.output);
        AnimationGraphUtility.ConnectOutput(playableIKGraph.output, avatarOutput);

        graph.Play();
    }

    private void OnDisable()
    {
        if (graph.IsValid())
        {
            graph.Stop();
            graph.Destroy();
        }
    }
}
