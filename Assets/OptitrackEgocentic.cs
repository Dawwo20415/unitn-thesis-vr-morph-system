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


    //WIP
    EgocentricHandler egocentricHandler;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        graph = PlayableGraph.Create("Traversal Test_" + UnityEngine.Random.Range(0.0f, 1.0f));

        m_handler = new OptitrackGraphHandler(graph, client, skeleton_name, animator, target_root, true);
        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Animation Output 1", animator);

        PlayableGraphUtility.ConnectOutput(m_handler.retargeted, output);

        m_chainHandler = new AvatarChainsHandler();
        //m_chainHandler.AddChain(
        //new List<HumanBodyBones> { HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm, HumanBodyBones.RightShoulder }, true);

        m_chainHandler.AddChain(
        new List<HumanBodyBones>         { HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm, HumanBodyBones.RightShoulder },
        new List<IDisplacementOperation> { new ScalarDisplacement(new Vector3(0.0f, 0.2f, 0.0f)), new EmptyDisplacement(), new EmptyDisplacement(), new EmptyDisplacement() },
        new List<bool>                   { true, false, false, false });

        egocentricHandler = new EgocentricHandler(m_handler.avatar, source_BSAD, this.gameObject, dest_BSAD, m_chainHandler);

        //m_egocentricGraphHandler = new EgocentricGraphHandler(graph, m_handler.avatar, source_BSAD, this.gameObject, dest_BSAD, m_chainHandler, animator);

        //m_egocentricGraphHandler.ConnectGraph(graph, m_handler.retargeted);
        //PlayableGraphUtility.ConnectOutput(m_egocentricGraphHandler.lastInPath, output);

        graph.Play();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("NEW FRAME ------------------------------------" + UnityEngine.Random.Range(0.0f, 1.0f));
        m_handler.Rebind(animator);
    }

    private void LateUpdate()
    {
        egocentricHandler.Targets(animator);
        egocentricHandler.Project(HumanBodyBones.RightHand);
    }

    private void OnDestroy()
    {
        graph.Stop();
        graph.Destroy();
    }
}
