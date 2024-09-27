using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class OptitrackEgocentic : MonoBehaviour
{
    // PLAYABLE GRAPH
    private Animator animator;
    private PlayableGraph graph;

    //OPTITRACK
    public PlayableOptitrackStreamingClient client;
    public string skeleton_name;
    public Transform target_root;
    private OptitrackGraphHandler m_handler;

    //BODY SURFACE APPROXIMATION
    public BodySurfaceApproximationDefinition source_BSAD;
    public BodySurfaceApproximationDefinition dest_BSAD;

    private AvatarChainsHandler m_chainHandler;
    private EgocentricGraphHandler m_egocentricGraphHandler;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        graph = PlayableGraph.Create("Traversal Test_" + UnityEngine.Random.Range(0.0f, 1.0f));

        m_handler = new OptitrackGraphHandler(graph, client, skeleton_name, animator, target_root, true);

        m_chainHandler = new AvatarChainsHandler();
        //m_chainHandler.AddChain(
        //new List<HumanBodyBones> { HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm, HumanBodyBones.RightShoulder }, true);

        m_chainHandler.AddChain(
        new List<HumanBodyBones>         { HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm, HumanBodyBones.RightShoulder },
        new List<IDisplacementOperation> { new ScalarDisplacement(new Vector3(0.0f, 1.5f, 0.0f)), new ScalarDisplacement(new Vector3(0.0f, 1.5f, 0.0f)), new ScalarDisplacement(new Vector3(0.0f, 1.5f, 0.0f)), new ScalarDisplacement(new Vector3(0.0f, 1.5f, 0.0f)) },
        new List<bool>                   { true, false, false, false });


        m_egocentricGraphHandler = new EgocentricGraphHandler(graph, m_handler.avatar, source_BSAD, this.gameObject, dest_BSAD, m_chainHandler, animator);

        m_egocentricGraphHandler.ConnectGraph(graph, m_handler.retargeted);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Animation Output 1", animator);
        PlayableGraphUtility.ConnectOutput(m_egocentricGraphHandler.lastInPath, output);

        graph.Play();
    }

    // Update is called once per frame
    void Update()
    {
        m_handler.Rebind(animator);
    }

    private void OnDestroy()
    {
        graph.Stop();
        graph.Destroy();
    }
}
