using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

/// <summary>
/// Go from the ones with 4 / (////)
/// </summary>

[RequireComponent(typeof(Animator))]
public class PlayableGraphTraversalTests : MonoBehaviour
{
    private Animator animator;
    private PlayableGraph graph;
    private OptitrackGraphHandler m_handler;
    private EgocentricGraphHandler m_egoHandler;
    private AvatarChainsHandler m_chainHandler;

    public PlayableOptitrackStreamingClient client;
    public string skeleton_name;
    public Transform target_root;

    [Header("BSADs")]
    public BodySurfaceApproximationDefinition source_BSAD;
    public BodySurfaceApproximationDefinition dest_BSAD;

    // Update is called once per frame
    void Start()
    {
        animator = GetComponent<Animator>();
        graph = PlayableGraph.Create("Traversal Test_" + UnityEngine.Random.Range(0.0f, 1.0f));

        m_handler = new OptitrackGraphHandler(graph, client, skeleton_name, animator, target_root, true);

        m_chainHandler = new AvatarChainsHandler();
        m_chainHandler.AddChain(
            new List<HumanBodyBones>         { HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftShoulder },
            new List<IDisplacementOperation> { new EmptyDisplacement(), new EmptyDisplacement(),     new EmptyDisplacement(),     new EmptyDisplacement() },
            new List<bool>                   { true,                    false,                       false,                       false});

        //OR
        //m_chainHandler.AddChain(new List<HumanBodyBones> { HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand }, true);

        m_egoHandler = new EgocentricGraphHandler(graph, m_handler.avatar, source_BSAD, this.gameObject, dest_BSAD, m_chainHandler, animator);

        m_egoHandler.ConnectGraph(graph, m_handler.retargeted);
        AnimationPlayableOutput out1 = AnimationPlayableOutput.Create(graph, skeleton_name + " Output", animator);

        PlayableGraphUtility.ConnectOutput(m_egoHandler.lastInPath, out1);

#if false

        N Chains of bones
        M Bones of that chain that need to be retargeted
        1 Node that applies all the (N*M) retargetings
        
        X Nodes for displacing targets
        
        1 Node for IK



#endif

        graph.Play();
    }

    private void Update()
    {
        m_handler.Rebind(animator);
    }

    private void OnDestroy()
    {
        graph.Stop();
        graph.Destroy();
    }
}
