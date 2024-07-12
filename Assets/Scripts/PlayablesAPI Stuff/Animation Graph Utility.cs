using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

public class AnimationGraphUtility 
{
    public class IKChainPlayableGraph
    {
        private AnimationScriptPlayable IKChain;
        private List<AnimationScriptPlayable> IKInputs;
        private NativeArray<Vector3> arr;

        public AnimationScriptPlayable output { get => IKChain; }
        public int inputsCount { get => IKInputs.Count; }
        public AnimationScriptPlayable this[int i] { get => IKInputs[i]; }

        public IKChainPlayableGraph (PlayableGraph graph, Animator animator, List<HumanBodyBones> bones, List<IKTarget> targets)
        {
            Generate(graph, animator, bones, targets);
            ConnectInternalGraph(graph);
        }

        public void ChangeVector(int i, Vector3 v)
        {
            arr[i] = v;
        }

        public void Dispose()
        {
            IKChain.GetJobData<PlayableIKChain>().Dispose();
            arr.Dispose();
        }

        private void Generate(PlayableGraph graph, Animator animator, List<HumanBodyBones> bones, List<IKTarget> targets)
        {
            List<IKTargetInput> targetInputs = new List<IKTargetInput>(targets.Count);
            IKInputs = new List<AnimationScriptPlayable>(targets.Count);

            foreach (IKTarget input in targets)
            {
                IKTargetInput t = new IKTargetInput();
                t.setup(input);

                AnimationScriptPlayable playable = AnimationScriptPlayable.Create(graph, t);

                IKInputs.Add(playable);
                targetInputs.Add(t);
            }
            
            PlayableIKChain IKJob = new PlayableIKChain();
            arr = new NativeArray<Vector3>(3, Allocator.Persistent);
            arr[0] = Vector3.zero;
            arr[1] = Vector3.forward;
            arr[2] = Vector3.one;
            IKJob.setup(animator, bones, targetInputs, arr);

            IKChain = AnimationScriptPlayable.Create(graph, IKJob);
        }
        private void ConnectInternalGraph(PlayableGraph graph)
        {
            foreach (AnimationScriptPlayable playable in IKInputs)
            {
                ConnectNodes(graph, playable, IKChain);
            } 
        }
    }

    public class PlayableGraphIKChain
    {
        private AnimationScriptPlayable m_IKPlayable;
        private IKChainJob m_IKJob;
        private List<ScriptPlayable<IKTargetWrap>> m_IKInputs;
        private ScriptPlayableOutput m_TargetOutput;
        private NativeArray<Vector3> m_IKTargets;
        private ScriptPlayable<IKTargetJoin> m_Dummy;

        public AnimationScriptPlayable output { get => m_IKPlayable; }
        public int inputsCount { get => m_IKInputs.Count; }
        public ScriptPlayable<IKTargetWrap> this[int i] { get => m_IKInputs[i]; }
        public string name { get; }
        public ScriptPlayable<IKTargetJoin> dummy { get => m_Dummy; }
        public ScriptPlayableOutput targetOutput { get => m_TargetOutput; }

        public PlayableGraphIKChain(PlayableGraph graph, Animator animator, List<HumanBodyBones> bones, List<IKTarget> targets, string n)
        {
            name = n;
            GEN_NODES(graph, animator, bones, targets);
            CONN_INTERNAL_NODES(graph);
        }

        ~PlayableGraphIKChain()
        {
            m_IKTargets.Dispose();
            m_IKJob.Dispose();
        }
        private void GEN_NODES(PlayableGraph graph, Animator animator, List<HumanBodyBones> bones, List<IKTarget> targets)
        {
            m_IKInputs = new List<ScriptPlayable<IKTargetWrap>>(targets.Count);
            m_IKTargets = new NativeArray<Vector3>(targets.Count, Allocator.Persistent);

            for (int i = 0; i < targets.Count; i++)
            {
                ScriptPlayable<IKTargetWrap> playable = ScriptPlayable<IKTargetWrap>.Create(graph);
                IKTargetWrap behaviour = playable.GetBehaviour();
                behaviour.setup(targets[i], m_IKTargets, i, name + "_" + i.ToString());

                m_IKInputs.Add(playable);
                m_IKTargets[i] = Vector3.zero;
            }

            Debug.Log("Finished Allocating Memory");

            m_IKJob = new IKChainJob();
            m_IKJob.setup(animator, bones, m_IKTargets);
            m_IKPlayable = AnimationScriptPlayable.Create(graph, m_IKJob);

            m_TargetOutput = ScriptPlayableOutput.Create(graph, "IKTargetsOutput " + name);
        }
        private void CONN_INTERNAL_NODES(PlayableGraph graph)
        {
            m_Dummy = ScriptPlayable<IKTargetJoin>.Create(graph);
            foreach (ScriptPlayable<IKTargetWrap> playable in m_IKInputs)
            {
                ConnectNodes(graph, playable, m_IKPlayable);
                ConnectNodes(graph, playable, m_Dummy);
            }
            ConnectOutput(m_Dummy, m_TargetOutput);
        }
    }

    public static bool ConnectIKInputs(PlayableGraph graph, List<Playable> targets, PlayableGraphIKChain chain)
    {
        if (targets.Count != chain.inputsCount) { return false; }

        for(int i = 0; i < targets.Count; i++)
        {
            ConnectNodes(graph, targets[i], chain[i]);
        }

        return true;
    }

    public static bool ConnectNodes(PlayableGraph graph, Playable output_node, Playable input_node)
    {

        int out_index = FirstFreeOutput(output_node);
        int in_index = FirstFreeInput(input_node);

        if (in_index == -1) { in_index = input_node.GetInputCount(); input_node.SetInputCount(in_index + 1); }
        if (out_index == -1) { out_index = output_node.GetOutputCount(); output_node.SetOutputCount(out_index + 1); }

        graph.Connect(output_node, out_index, input_node, in_index);
        input_node.SetInputWeight(in_index, 1.0f);

        return true;
    }

    public static bool ConnectNodesI(PlayableGraph graph, Playable output_node, Playable input_node, int out_index, int in_index)
    {

        if (in_index == -1) { in_index = input_node.GetInputCount(); input_node.SetInputCount(in_index + 1); }
        if (out_index == -1) { out_index = output_node.GetOutputCount(); output_node.SetOutputCount(out_index + 1); }

        graph.Connect(output_node, out_index, input_node, in_index);
        input_node.SetInputWeight(in_index, 1.0f);

        return true;
    }

    public static bool InterposeNodes(PlayableGraph graph, Playable node_in, Playable node_out, Playable source, Playable destination)
    {
        int in_index = GetInputIndex(destination, source);
        if (in_index == -1) { return false; }

        destination.DisconnectInput(in_index);

        ConnectNodes(graph, source, node_in);
        ConnectNodes(graph, node_out, destination);

        return true;
    }

    public static bool InterposeNode(PlayableGraph graph, Playable node, Playable source, Playable destination)
    {
        return InterposeNodes(graph, node, node, source, destination);
    }

    public static bool ConnectOutput(Playable src_node, PlayableOutput dest_output)
    {
        int src_index = FirstFreeOutput(src_node);

        if (src_index == -1) { src_index = src_node.GetOutputCount(); src_node.SetOutputCount(src_index + 1); }

        dest_output.SetSourcePlayable(src_node, src_index);
        dest_output.SetWeight(1.0f);

        return true;
    }

    private static int FirstFreeInput(Playable playable)
    {
        for (int i = 0; i < playable.GetInputCount(); i++)
        {
            if (playable.GetInput(i).IsNull())
            {
                return i;
            }
        }

        return -1;
    }

    private static int FirstFreeOutput(Playable playable)
    {
        for (int i = 0; i < playable.GetOutputCount(); i++)
        {
            if (playable.GetOutput(i).IsNull())
            {
                return i;
            }
        }

        return -1;
    }

    private static int GetInputIndex(Playable playable, Playable comparison)
    {
        for (int i = 0; i < playable.GetInputCount(); i++)
        {
            if (playable.GetInput(i).Equals(comparison))
                return i;
        }

        return -1;
    }

    private static int GetOutputIndex(Playable playable, Playable comparison)
    {
        for (int i = 0; i < playable.GetOutputCount(); i++)
        {
            if (playable.GetOutput(i).Equals(comparison))
                return i;
        }

        return -1;
    }

    private static bool ConnectEgocentric(EgocentricSelfContact egocentric, IKTargetPipeline pipeline)
    {

        return true;
    }
}
