using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine;

public class EgocentricProjectionBehaviour : PlayableBehaviour
{
    public override void OnPlayableCreate(Playable playable)
    {
        PlayableGraph graph = playable.GetGraph();
        ScriptPlayableOutput output = PlayableGraphUtility.CheckConnectedUserDataByType<TestEgocentricOutput>(graph, playable);
        TestEgocentricOutput userData = (TestEgocentricOutput)output.GetUserData();

        //Here is the need to register the fact that this bone wants to project on the ProjectionEngine component
    }

    public override void PrepareFrame(Playable playable, FrameData info) { }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!playerData.GetType().Equals(typeof(TestEgocentricOutput))) { return; }

        Debug.Log("Process Frame");
    }
}
