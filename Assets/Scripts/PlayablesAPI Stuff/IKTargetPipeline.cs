using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

public struct ExtractBone : IAnimationJob, IKTarget, IDisposable
{
    private TransformStreamHandle handle;
    private NativeArray<Vector3> position;

    public void setup(Animator animator, HumanBodyBones hbb)
    {
        handle = animator.BindStreamTransform(animator.GetBoneTransform(hbb));
        position = new NativeArray<Vector3>(1, Allocator.Persistent);
    }

    public Vector3 GetTarget()
    {
        //Debug.Log("ExtractBone process animation " + VExtension.Print(position[0]));
        return position[0];
    }
    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream) 
    {
        //Debug.Log("ExtractBone process animation " + VExtension.Print(handle.GetPosition(stream)));
        position[0] = handle.GetPosition(stream);
    }

    public void Dispose()
    {
        position.Dispose();
    }
}

public class StaticDisplacement : PlayableBehaviour, IKTarget
{
    IKTarget input;
    Vector3 displacement;

    public void Setup(IKTarget target, Vector3 dis)
    {
        input = target;
        displacement = dis;
    }

    public Vector3 GetTarget()
    {
        return input.GetTarget() + displacement;
    }

}

public interface IDisposable
{
    public void Dispose();
}

public class IKTargetPipeline
{
    public Playable firstPlayable { get => m_playables[0]; }
    public Playable lastPlayable { get => m_playables[m_playables.Count - 1]; }

    public IKTarget firstNode { get => m_nodes[0]; }
    public IKTarget lastNode { get => m_nodes[m_nodes.Count - 1]; }

    private List<IKTarget> m_nodes;
    private List<Playable> m_playables;

    public IKTargetPipeline()
    {
        m_nodes = new List<IKTarget>();
        m_playables = new List<Playable>();
    }

    public void AppendJob<Job>(PlayableGraph graph, Job job) 
        where Job : struct, IAnimationJob, IKTarget
    {
        AnimationScriptPlayable playable = AnimationScriptPlayable.Create(graph, job);
        
        if (m_playables.Count > 0)
            AnimationGraphUtility.ConnectNodes(graph, m_playables[m_playables.Count - 1], playable);

        m_nodes.Add(job);
        m_playables.Add(playable);
    }

    public void AppendBehaviour<Behaviour>(PlayableGraph graph, Behaviour behaviour) 
        where Behaviour : notnull, PlayableBehaviour, IKTarget, new()
    {
        ScriptPlayable<Behaviour> playable = ScriptPlayable<Behaviour>.Create(graph, behaviour);

        if (m_playables.Count > 0)
            AnimationGraphUtility.ConnectNodes(graph, m_playables[m_playables.Count - 1], playable);

        m_nodes.Add(behaviour);
        m_playables.Add(playable);
    }

    ~IKTargetPipeline()
    { 
        foreach (IKTarget node in m_nodes)
        {
            if (node is IDisposable)
            {
                ((IDisposable)node).Dispose();
            }
        }
    }
}
