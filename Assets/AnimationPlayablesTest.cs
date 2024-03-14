using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine;

public struct MuscleHandleExampleJob : IAnimationJob {
    
    public MuscleHandle muscleHandle;
    public float newValue;

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
            Debug.Log("muscleHandle | DoF[" + muscleHandle.dof + "] DoF[" + muscleHandle.humanPartDof + "]");
            //Debug.Log("Human Stream | Value[" + muscleValue + "]");
            Debug.Log("NewValue: " + newValue);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
        }
        Debug.Log("Inside IAnimation Job");
        // Set a muscle value.
        humanStream.SetMuscle(muscleHandle, newValue);
    }
}


[RequireComponent(typeof(Animator))]
public class AnimationPlayablesTest : MonoBehaviour
{

    [Range(-1.0f, 1.0f)]
    public float value;

    private PlayableGraph graph;
    private bool start = true;
    private MuscleHandleExampleJob job;
    private AnimationScriptPlayable playable;

    // Start is called before the first frame update
    void Start()
    {
        graph = PlayableGraph.Create();
        var output = AnimationPlayableOutput.Create(graph, "output", GetComponent<Animator>());

        job = new MuscleHandleExampleJob();
        job.muscleHandle = new MuscleHandle(HumanPartDof.LeftLeg, LegDof.UpperLegFrontBack);
        job.newValue = value;

        playable = AnimationScriptPlayable.Create(graph, job);
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
        playable.SetJobData<MuscleHandleExampleJob>(job);

        
    }

    private void OnDestroy()
    {
        graph.Stop();
        graph.Destroy();
    }
}
