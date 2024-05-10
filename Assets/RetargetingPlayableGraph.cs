using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

[RequireComponent(typeof(Animator))]
public class RetargetingPlayableGraph : MonoBehaviour
{
    public Animator source_animator;

    //PlayableGraph Stuff
    private Animator animator;
    private Avatar avatar;
    private PlayableGraph graph;

    //SCRIPT PLAYABLE TPOSE
    private ScriptPlayable<AvatarTPoseBehaviour> playable;
    private AvatarTPoseBehaviour tpose_behaviour;

    //RETARGETING NODE
    private ScriptPlayable<AvatarRetargetingBehaviour> retargetingPlayable;
    private AvatarRetargetingBehaviour retargeting_behaviour;

    //ANIMATION OUTPUT - AVATAR
    private AnimationPlayableOutput animationOutput;
    private PoseApplyJob poseApplyJob;
    private AnimationScriptPlayable animationPlayable;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        avatar = animator.avatar;

        graph = PlayableGraph.Create("Retargeting Test_" + UnityEngine.Random.Range(0.0f, 1.0f));

        //TPose Playable
        playable = ScriptPlayable<AvatarTPoseBehaviour>.Create(graph);
        tpose_behaviour = playable.GetBehaviour();
        //tpose_behaviour.TPoseSetup(source_animator);
        tpose_behaviour.TPoseSetup(animator);

        //Retargeting Behaviour
        retargetingPlayable = ScriptPlayable<AvatarRetargetingBehaviour>.Create(graph);
        retargeting_behaviour = retargetingPlayable.GetBehaviour();
        //retargeting_behaviour.RetargetingSetup(source_animator, animator, playable.GetBehaviour());
        //retargeting_behaviour.RetargetingSetup(animator, animator, playable.GetBehaviour());

        //Pose Applier
        poseApplyJob = new PoseApplyJob();
        poseApplyJob.Init(retargetingPlayable.GetBehaviour(), animator, true);
        animationPlayable = AnimationScriptPlayable.Create(graph, poseApplyJob);

        //Animation Output
        animationOutput = AnimationPlayableOutput.Create(graph, "Avatar Animation Output", animator);

        //Connections
        animationOutput.SetSourcePlayable(animationPlayable, 1);

        animationPlayable.SetInputCount(1);
        retargetingPlayable.SetOutputCount(1);
        graph.Connect(retargetingPlayable, 0, animationPlayable, 0);
        animationPlayable.SetInputWeight(0, 1.0f);

        retargetingPlayable.SetInputCount(1);
        playable.SetOutputCount(1);
        graph.Connect(playable, 0, retargetingPlayable, 0);
        retargetingPlayable.SetInputWeight(0, 1.0f);

        graph.Play();
    }

    private void OnDisable()
    {
        poseApplyJob.Dispose();
        tpose_behaviour.Dispose();
        retargeting_behaviour.Dispose();
        if (graph.IsValid())
        {
            graph.Stop();
            graph.Destroy();
        }
    }
}
