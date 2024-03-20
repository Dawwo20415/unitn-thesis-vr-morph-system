using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;
using UnityEngine;

public struct MuscleHandleExampleJob : IAnimationJob {
    
    public MuscleHandle muscleHandle;
    public float newValue;
    public Vector3 position;

    public void ProcessRootMotion(AnimationStream stream) 
    {
    }
    public void ProcessAnimation(AnimationStream stream)
    {
        AnimationHumanStream humanStream = stream.AsHuman();
        // Get a muscle value.
        float muscleValue = humanStream.GetMuscle(muscleHandle);

        try
        {
            //Debug.Log("muscleHandle | Name[" + muscleHandle.name + "] DoF[" + muscleHandle.dof + "] DoF[" + muscleHandle.humanPartDof + "]");
            //Debug.Log("muscleHandle | DoF[" + muscleHandle.dof + "] DoF[" + muscleHandle.humanPartDof + "]");
            //Debug.Log("Human Stream | Value[" + muscleValue + "]");
            //Debug.Log("NewValue: " + newValue);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
        }
        // Set a muscle value.
        humanStream.SetMuscle(muscleHandle, newValue);
    }
}


[RequireComponent(typeof(Animator))]
public class AnimationPlayablesTest : MonoBehaviour
{

    [Range(-1.0f, 1.0f)]
    public float value;
    public ushort order;
    [Range(0.0f, 1.0f)]
    public float weight;
    public Vector3 position;

    private PlayableGraph graph;
    private bool start = true;
    private MuscleHandleExampleJob job;
    private AnimationScriptPlayable playable;
    private AnimationPlayableOutput output;

    public HumanPartDof human_part_dof;
    public LegDof leg_dof;

    // Start is called before the first frame update
    void Start()
    {
        graph = PlayableGraph.Create("Muscle Controll");
        output = AnimationPlayableOutput.Create(graph, "output", GetComponent<Animator>());

        job = new MuscleHandleExampleJob();
        job.muscleHandle = new MuscleHandle(human_part_dof, leg_dof);
        job.newValue = value;
        job.position = position;

        playable = AnimationScriptPlayable.Create(graph, job);
        AnimationPlayableOutputExtensions.SetSortingOrder(output, order);
        output.SetWeight<AnimationPlayableOutput>(weight);
        output.SetSourcePlayable(playable);
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            graph.Play();
            start = false;
        }

        job.newValue = value;
        job.muscleHandle = new MuscleHandle(human_part_dof, leg_dof);
        job.position = position;
        playable.SetJobData<MuscleHandleExampleJob>(job);
        output.SetWeight<AnimationPlayableOutput>(weight);
        Debug.Log("Sort Order: " + order + " | Weight: " + output.GetWeight<AnimationPlayableOutput>(), this);
    }

    private void OnDestroy()
    {
        graph.Stop();
        graph.Destroy();
    }
}
