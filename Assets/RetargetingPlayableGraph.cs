using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class RetargetingPlayableGraph : MonoBehaviour
{
    public Animator source_animator;

    public List<Vector4> mirrors;
    private List<bool> mirrorList;
    private List<Vector3> mirrorAxis;

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

        FillMirrors();

        graph = PlayableGraph.Create("Retargeting Test_" + UnityEngine.Random.Range(0.0f, 1.0f));

        //TPose Playable
        playable = ScriptPlayable<AvatarTPoseBehaviour>.Create(graph);
        tpose_behaviour = playable.GetBehaviour();
        //tpose_behaviour.TPoseSetup(source_animator);
        tpose_behaviour.TPoseSetup(animator);

        //Retargeting Behaviour
        retargetingPlayable = ScriptPlayable<AvatarRetargetingBehaviour>.Create(graph);
        retargeting_behaviour = retargetingPlayable.GetBehaviour();
        retargeting_behaviour.RetargetingSetup(source_animator, animator, playable.GetBehaviour(), mirrorList, mirrorAxis);
        //retargeting_behaviour.RetargetingSetup(animator, animator, playable.GetBehaviour());

        //Pose Applier
        poseApplyJob = new PoseApplyJob();
        poseApplyJob.Init(retargetingPlayable.GetBehaviour(), animator, true);
        animationPlayable = AnimationScriptPlayable.Create(graph, poseApplyJob);

        //Animation Output
        animationOutput = AnimationPlayableOutput.Create(graph, "Avatar Animation Output", animator);

        //Connections
        animationOutput.SetSourcePlayable(animationPlayable, 1);

        AnimationGraphUtility.ConnectNodes(graph, retargetingPlayable, animationPlayable);
        AnimationGraphUtility.ConnectNodes(graph, playable, retargetingPlayable);

        graph.Play();
    }

    private void FillMirrors()
    {
        mirrorList = new List<bool>((int)HumanBodyBones.LastBone);
        mirrorAxis = new List<Vector3>((int)HumanBodyBones.LastBone);

        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            mirrorList.Add(false);
            mirrorAxis.Add(Vector3.zero);
        }

        foreach (Vector4 info in mirrors)
        {
            mirrorList[(int)info.x] = true;
            mirrorAxis[(int)info.x] = new Vector3(info.y, info.z, info.w);
        }
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
