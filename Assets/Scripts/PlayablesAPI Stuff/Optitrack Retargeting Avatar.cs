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

        avatarOutput = AnimationPlayableOutput.Create(graph, "Avatar Output", animator);
        AnimationGraphUtility.ConnectOutput(optitrackGraph.retargeted, avatarOutput);

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
