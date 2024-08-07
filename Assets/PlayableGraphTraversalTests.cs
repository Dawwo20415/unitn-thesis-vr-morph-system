using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

public class PlayableBehaviourTest : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        string to_print = "ProcessingFrame - normal traversal " + info.output.GetHandle().GetHashCode() + " - ";
        if (playerData != null)
        {
            to_print += playerData.ToString();
        } else
        {
            to_print += "Not Available";
        }
        Debug.Log(to_print);
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        Debug.Log("Preparing frame - inverse traversal - " + info.output.GetHandle().GetHashCode());
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        Debug.Log("Behaviour Play");
    }

    public override void OnGraphStart(Playable playable)
    {
        Debug.Log("On Graph Start");
    }

    public override void OnPlayableCreate(Playable playable)
    {
        Debug.Log("On Playable Create");
    }
}

public class PlayableBehaviourEmpty : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
    }
}


[RequireComponent(typeof(Animator))]
public class PlayableGraphTraversalTests : MonoBehaviour
{
    private Animator animator;
    private PlayableGraph graph;

    // Update is called once per frame
    void Start()
    {
        animator = GetComponent<Animator>();
        graph = PlayableGraph.Create("Traversal Test_" + UnityEngine.Random.Range(0.0f, 1.0f));

        ScriptPlayable<PlayableBehaviourTest> test_script_playable = ScriptPlayable<PlayableBehaviourTest>.Create(graph);
        ScriptPlayable<PlayableBehaviourEmpty> empty_script_playable = ScriptPlayable<PlayableBehaviourEmpty>.Create(graph);

        AnimationPlayableOutput script_output = AnimationPlayableOutput.Create(graph, "animation output 1", animator);
        ScriptPlayableOutput script_output_2 = ScriptPlayableOutput.Create(graph, "script output 2");
        //script_output_2.SetUserData(this);
        //script_output_2.SetReferenceObject(animator);

        test_script_playable.SetOutputCount(2);
        test_script_playable.SetInputCount(1);
        empty_script_playable.SetOutputCount(1);
        empty_script_playable.SetInputCount(1);
        graph.Connect(test_script_playable, 1, empty_script_playable, 0);
        empty_script_playable.SetInputWeight(0, 1.0f);

        script_output.SetSourcePlayable(empty_script_playable);
        script_output_2.SetSourcePlayable(test_script_playable, 0);

        

        graph.Play();
    }

    private void OnDestroy()
    {
        graph.Stop();
        graph.Destroy();
    }
}
