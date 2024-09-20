using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

[RequireComponent(typeof(Animator))]
public class PlayableGraphTraversalTests : MonoBehaviour
{
    private Animator animator;
    private PlayableGraph graph;
    private OptitrackGraphHandler m_handler;
    private EgocentricGraphHandler m_egoHandler;

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
        m_egoHandler = new EgocentricGraphHandler(graph, m_handler.avatar, source_BSAD, this.gameObject, dest_BSAD, m_handler.retargeted, animator);

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
