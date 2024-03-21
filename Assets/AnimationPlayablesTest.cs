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
        humanStream.SetMuscle(muscleHandle, newValue);
        humanStream.bodyPosition = position;
    }
}


[RequireComponent(typeof(Animator))]
public class AnimationPlayablesTest : MonoBehaviour
{

    [Range(-1.0f, 1.0f)]
    public float value;
    //public ushort order;
    [Range(0.0f, 1.0f)]
    public float weight;
    public Vector3 position;

    private PlayableGraph graph;
    //private bool start = true;
    private MuscleHandleExampleJob job;
    private AnimationScriptPlayable playable;
    private AnimationPlayableOutput output;

    public HumanPartDof human_part_dof;
    public LegDof leg_dof;
    //public HumanBodyBones rotation_bone;

    // Start is called before the first frame update
    void Start()
    {
        Animator animator = GetComponent<Animator>();
        graph = PlayableGraph.Create("Muscle Controll_" + UnityEngine.Random.Range(0.0f, 1.0f));
        output = AnimationPlayableOutput.Create(graph, "output", animator);

        job = new MuscleHandleExampleJob();
        job.muscleHandle = new MuscleHandle(human_part_dof, leg_dof);
        job.newValue = value;
        job.position = position;

        playable = AnimationScriptPlayable.Create(graph, job);
        //AnimationPlayableOutputExtensions.SetSortingOrder(output, order);
        output.SetWeight<AnimationPlayableOutput>(weight);
        output.SetSourcePlayable(playable);
        graph.Play();
    }

    // Update is called once per frame
    void Update()
    {

        job.newValue = value;
        job.muscleHandle = new MuscleHandle(human_part_dof, leg_dof);
        job.position = position;
        playable.SetJobData<MuscleHandleExampleJob>(job);
        output.SetWeight<AnimationPlayableOutput>(weight);
    }

    private void OnDestroy()
    {
        if (graph.IsValid())
        {
            graph.Stop();
            graph.Destroy();
        }     
    }
}
