using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Collections;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;
using UnityEngine;
using UnityEngine.XR.OpenXR.Input;

/// <progress>
/// TODO create the graph, cre ate the single quaternion modifiers and create the merger
/// </progress>

public class PoseBehaviour : PlayableBehaviour
{
    protected NativeArray<Quaternion> bones;
    protected NativeArray<Vector3> joints;
    protected int bone_count;

    public virtual void Initialize(Animator animator)
    {
        bone_count = animator.avatar.humanDescription.human.Length;
        bones = new NativeArray<Quaternion>(bone_count, Allocator.Persistent);
        joints = new NativeArray<Vector3>(bone_count, Allocator.Persistent);
    }

    public Quaternion ExposeBone(int index)
    {
        return bones[index];
    }

    public Vector3 ExposeJoint(int index)
    {
        return joints[index];
    }
    
    public void Dispose()
    {
        bones.Dispose();
        joints.Dispose();
    }
}

public class TPoseBehaviour : PoseBehaviour
{
    public override void Initialize(Animator animator)
    {
        base.Initialize(animator);
        
        for (int i = 0; i < bone_count; i++)
        {
            int skeleton_index = LookUpSkeleton(animator.avatar.humanDescription.human[i].boneName, animator.avatar.humanDescription);
            bones[i] = animator.avatar.humanDescription.skeleton[skeleton_index].rotation;
            joints[i] = animator.avatar.humanDescription.skeleton[skeleton_index].position;
        }

    }

    int LookUpBone(string name)
    {
        for (int i = 0; i < HumanTrait.BoneName.Length; i++)
        {
            if (HumanTrait.BoneName[i] == name)
                return i;
        }

        return -1;
    }

    int LookUpSkeleton(string name, HumanDescription hd)
    {
        for (int i = 0; i < hd.skeleton.Length; i++)
        {
            if (name == hd.skeleton[i].name)
                return i;
        }

        return -1;
    }
}

public struct PoseToAnimation : IAnimationJob
{

    private ScriptPlayable<PoseBehaviour> pose_playable;
    private NativeArray<TransformStreamHandle> bones;
    private int count;

    public void Setup(Animator animator, List<Transform> bones_trn)
    {
        bones = new NativeArray<TransformStreamHandle>(bones_trn.Count, Allocator.Persistent);
        for (int i = 0; i < bones_trn.Count; i++) { bones[i] = animator.BindStreamTransform(bones_trn[i]); }

        count = bones_trn.Count;
    }

    public void SetInputPlayable(Playable owner, Playable input, PlayableGraph graph)
    {
        owner.SetInputCount(1);
        pose_playable = (ScriptPlayable<PoseBehaviour>)input;
        graph.Connect(input, 0, owner, 0);
        owner.SetInputWeight(0, 1);
    }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        Debug.Log("In ProcessAnimation");
        //Is it faster to just Array.Copy() the whole thing every frame? It is a relatively large array (~55 elements)
        //Then what is the C# equivalent on iterating on pointers to avoid the index lookup overhead?
        Debug.Log("In ProcessAnimation - past null check");
        PoseBehaviour pose = pose_playable.GetBehaviour();
        for (int i = 0; i < count; i++)
        {
            Debug.Log("Index[" + i + "] setting rotation[" + pose.ExposeBone(i) + "]");
            bones[i].SetLocalRotation(stream, pose.ExposeBone(i));
        }
    }

    public void Dispose()
    {
        bones.Dispose();
    }
}

public struct RawRotationJob : IAnimationJob
{
    private NativeArray<TransformStreamHandle> bones;
    private NativeArray<Quaternion> quaternions;
    private int count;

    public void Setup(Animator animator, List<Transform> bones_trn, List<Quaternion> t_pose_rotations)
    {
        bones = new NativeArray<TransformStreamHandle>(bones_trn.Count, Allocator.Persistent);
        for (int i = 0; i < bones_trn.Count; i++)   { bones[i] = animator.BindStreamTransform(bones_trn[i]); }

        quaternions = new NativeArray<Quaternion>(t_pose_rotations.Count, Allocator.Persistent);
        for (int i = 0; i < t_pose_rotations.Count; i++) { quaternions[i] = t_pose_rotations[i]; }

        count = bones_trn.Count;
    }

    public void Dispose()
    {
        bones.Dispose();
        quaternions.Dispose();
    }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        for (int i = 0; i < count; i++)
        {
            bones[i].SetLocalRotation(stream, quaternions[i]);
        }
    }
}

public class RawRotationTesting : MonoBehaviour
{
    private PlayableGraph graph;
    private AnimationPlayableOutput output;
    private ScriptPlayableOutput output2;
    private AnimationScriptPlayable playable;
    private PoseToAnimation job;
    private TPoseBehaviour tpose_behaviour;

    // Start is called before the first frame update
    void OnEnable()
    {
        Animator animator = GetComponent<Animator>();
        graph = PlayableGraph.Create("T.Pose Test_" + UnityEngine.Random.Range(0.0f, 1.0f));
        output = AnimationPlayableOutput.Create(graph, "output", animator);
        //output2 = ScriptPlayableOutput.Create(graph, "fittizio");

        tpose_behaviour = new TPoseBehaviour();
        tpose_behaviour.Initialize(animator);
        var tpose_playable = ScriptPlayable<TPoseBehaviour>.Create(graph, tpose_behaviour);

        List<Transform> trn = new List<Transform>(animator.avatar.humanDescription.human.Length);

        for (int i = 0; i < animator.avatar.humanDescription.human.Length; i++)
        {
            int index = LookUpBone(animator.avatar.humanDescription.human[i].humanName);
            trn.Add(animator.GetBoneTransform((HumanBodyBones)index));
        }

        job = new PoseToAnimation();
        job.Setup(animator, trn);

        playable = AnimationScriptPlayable.Create(graph, job);
        job.SetInputPlayable(playable, tpose_playable, graph);
        output.SetSourcePlayable(playable);
        //output2.SetSourcePlayable(tpose_playable);
        graph.Play();
    }

    private void OnDisable()
    {
        job.Dispose();
        tpose_behaviour.Dispose();

        if (graph.IsValid())
        {
            graph.Stop();
            graph.Destroy();
        }
    }


    private void SetupJob( Animator animator, RawRotationJob job)
    {
        List<Transform> trn = new List<Transform>(animator.avatar.humanDescription.human.Length);
        List<Quaternion> q = new List<Quaternion>(animator.avatar.humanDescription.human.Length);

        for (int i = 0; i < animator.avatar.humanDescription.human.Length; i++)
        {
            int index = LookUpBone(animator.avatar.humanDescription.human[i].humanName);
            trn.Add(animator.GetBoneTransform((HumanBodyBones)index));
            q.Add(animator.avatar.humanDescription.skeleton[i].rotation);
        }

        Debug.Log(trn.Count,this);

        job.Setup(animator, trn, q);
    }

    private string EquivalentHumanName(string name, Avatar avatar)
    {
        foreach (HumanBone bone in avatar.humanDescription.human)
        {
            if (name == bone.boneName)
            {
                return bone.humanName;
            }
        }
        Debug.Log(name);
        return "NOT FOUND";
    }

    int LookUpBone(string name)
    {
        for (int i = 0; i < HumanTrait.BoneName.Length; i++)
        {
            if (HumanTrait.BoneName[i] == name)
                return i;
        }

        return -1;
    }

    int LookUpSkeleton(string name, HumanDescription hd)
    {
        for (int i = 0; i < hd.skeleton.Length; i++)
        {
            if (name == hd.skeleton[i].name)
                return i;
        }

        return -1;
    }

}
