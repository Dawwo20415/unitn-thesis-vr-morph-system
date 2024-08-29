using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class EgocentricIKChainBehaviour : PlayableBehaviour
{
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        PlayableGraph graph = playable.GetGraph();
        ScriptPlayableOutput output = PlayableGraphUtility.CheckConnectedUserDataByType<TestEgocentricOutput>(graph, playable);
        TestEgocentricOutput userData = (TestEgocentricOutput)output.GetUserData();
    }

    public override void PrepareFrame(Playable playable, FrameData info) { }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!playerData.GetType().Equals(typeof(TestEgocentricOutput))) { return; }

        Debug.Log("Process Frame");
    }
}
