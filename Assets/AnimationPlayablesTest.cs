using System.Collections;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine;

public struct MuscleHandleExampleJob : IAnimationJob {
    
    public MuscleHandle muscleHandle;

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        AnimationHumanStream humanStream = stream.AsHuman();

        // Get a muscle value.
        float muscleValue = humanStream.GetMuscle(muscleHandle);

        Debug.Log("muscleHandle | Name[" + muscleHandle.name + "] DoF[" + muscleHandle.dof + "] DoF[" + muscleHandle.humanPartDof + "]");
        Debug.Log("Human Stream | Value[" + muscleValue + "]");
        // Set a muscle value.
        humanStream.SetMuscle(muscleHandle, muscleValue);
    }
}


[RequireComponent(typeof(Animator))]
public class AnimationPlayablesTest : MonoBehaviour
{

    private PlayableGraph graph;

    // Start is called before the first frame update
    void Start()
    {
        graph = PlayableGraph.Create();
        var output = AnimationPlayableOutput.Create(graph, "output", GetComponent<Animator>());

        var job = new MuscleHandleExampleJob();
        job.muscleHandle = new MuscleHandle(HumanPartDof.LeftArm, ArmDof.HandDownUp);

        var scriptPlayable = AnimationScriptPlayable.Create(graph, job);
        output.SetSourcePlayable(scriptPlayable);

        graph.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (graph.IsPlaying())
        {
            Debug.Log("Playing");
        }

        if (graph.IsDone())
        {
            Debug.Log("Done");
        }
    }

    private void OnDestroy()
    {
        graph.Stop();
        graph.Destroy();
    }
}
