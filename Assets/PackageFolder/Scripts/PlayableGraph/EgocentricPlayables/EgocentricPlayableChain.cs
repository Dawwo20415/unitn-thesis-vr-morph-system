using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

public class EgocentricIKChainBehaviour : PlayableBehaviour
{
    /*public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        PlayableGraph graph = playable.GetGraph();
        ScriptPlayableOutput output = PlayableGraphUtility.CheckConnectedUserDataByType<TestEgocentricOutput>(graph, playable);
        TestEgocentricOutput userData = (TestEgocentricOutput)output.GetUserData();
    }*/

    public override void PrepareFrame(Playable playable, FrameData info) { }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!playerData.GetType().Equals(typeof(TestEgocentricOutput))) { return; }

        //Debug.Log("Process Frame");
    }
}

public struct DefineTargets : IAnimationJob
{
    NativeArray<Vector3> m_Targets;
    NativeArray<TransformStreamHandle> m_Handles;
    NativeArray<int> m_Indexes;

    public void Setup(NativeArray<Vector3> array, Animator animator, List<HumanBodyBones> bones)
    {
        m_Targets = array;
        m_Handles = new NativeArray<TransformStreamHandle>(bones.Count, Allocator.Persistent);
        m_Indexes = new NativeArray<int>(bones.Count, Allocator.Persistent);

        for (int i = 0; i < bones.Count; i++)
        {
            m_Indexes[i] = ((int)(bones[i]));
            m_Handles[i] = animator.BindStreamTransform(animator.GetBoneTransform(bones[i]));
        }
    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        for (int i = 0; i < m_Indexes.Length; i++)
        {
            m_Targets[m_Indexes[i]] = m_Handles[i].GetPosition(stream);
        }
    }

    public void Dispose()
    {
        m_Handles.Dispose();
        m_Indexes.Dispose();
    }
}

