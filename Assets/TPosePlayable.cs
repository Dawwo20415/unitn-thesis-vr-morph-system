using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class TPosePlayable : MonoBehaviour
{
    //PlayableGraph Stuff
    private Animator animator;
    private Avatar avatar;
    private PlayableGraph graph;

    //SCRIPT PLAYABLE TPOSE
    private ScriptPlayable<AvatarTPoseBehaviour> playable;
    private AvatarTPoseBehaviour behaviour;

    //ANIMATION OUTPUT - AVATAR
    private AnimationPlayableOutput animationOutput;
    private PoseApplyJob poseApplyJob;
    private AnimationScriptPlayable animationPlayable;

    //SCRIPT OUTPUT - SCRIPT
    private ScriptPlayableOutput scriptOutput;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        avatar = animator.avatar;

        graph = PlayableGraph.Create("Optitrack Test_" + UnityEngine.Random.Range(0.0f, 1.0f));
        scriptOutput = ScriptPlayableOutput.Create(graph, "output");
        animationOutput = AnimationPlayableOutput.Create(graph, "Avatar Animation Output", animator);
        PlayableOutputExtensions.SetUserData(scriptOutput, this);

        poseApplyJob = new PoseApplyJob();

        playable = ScriptPlayable<AvatarTPoseBehaviour>.Create(graph);
        behaviour = playable.GetBehaviour();

        poseApplyJob.Init(playable.GetBehaviour(), animator, true);
        behaviour.TPoseSetup(animator);

        animationPlayable = AnimationScriptPlayable.Create(graph, poseApplyJob);
        animationPlayable.SetInputCount(1);
        
        playable.SetOutputCount(2);

        scriptOutput.SetSourcePlayable(playable, 0);
        animationOutput.SetSourcePlayable(animationPlayable, 1);
        graph.Connect(playable, 1, animationPlayable, 0);
        animationPlayable.SetInputWeight(0, 1.0f);

        graph.Play();
    }

    private void OnDisable()
    {
        poseApplyJob.Dispose();
        behaviour.Dispose();
        if (graph.IsValid())
        {
            graph.Stop();
            graph.Destroy();
        }
    }
}
