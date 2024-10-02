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

    //WIP
    EgocentricRetargeting egocentricRetargeting;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        graph = PlayableGraph.Create("Traversal Test_" + UnityEngine.Random.Range(0.0f, 1.0f));

        m_handler = new OptitrackGraphHandler(graph, client, skeleton_name, animator, target_root, true);
        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Animation Output 1", animator);

        PlayableGraphUtility.ConnectOutput(m_handler.retargeted, output);

        m_chainHandler = new AvatarChainsHandler();

        m_chainHandler.AddChain(
        new List<HumanBodyBones>         { HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm, HumanBodyBones.RightShoulder },
        new List<IDisplacementOperation> { new EmptyDisplacement(), new EmptyDisplacement(), new EmptyDisplacement(), new EmptyDisplacement() },
        new List<bool>                   { true, false, false, false });

        //m_chainHandler.AddChain(
        //new List<HumanBodyBones> { HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftShoulder }, true);

        egocentricRetargeting = new EgocentricRetargeting(m_handler.avatar, source_BSAD, this.gameObject, dest_BSAD, m_chainHandler, animator);

        graph.Play();
    }

    // Update is called once per frame
    void Update()
    {
        m_handler.Rebind(animator);
    }

    private void LateUpdate()
    {
        egocentricRetargeting.Retarget(animator);
    }

    private void OnDestroy()
    {
        graph.Stop();
        graph.Destroy();
    }
}
