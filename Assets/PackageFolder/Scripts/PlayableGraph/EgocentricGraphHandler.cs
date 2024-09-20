using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class EgocentricGraphHandler
{
    public Playable lastInPath { get => m_IKPlayable; }

    private TestEgocentricOutput m_referenceObject;
    private EgocentricPlayableOutput m_egocentricOutput;
    private ScriptPlayable<EgocentricIKChainBehaviour> m_IKChain;
    private ScriptPlayable<EgocentricProjectionBehaviour> m_projPlayable;

    private AnimationScriptPlayable m_IKPlayable;
    private EgocentricIKJob m_IKJob;

    private AnimationScriptPlayable m_SetTargetsPlayable;
    private DefineTargets m_TargetsJob;

    private BSAComponent m_sourceBSA;
    private BSAComponent m_destBSA;

    public EgocentricGraphHandler(PlayableGraph graph, GameObject source, BodySurfaceApproximationDefinition source_BSAD, GameObject avatar, BodySurfaceApproximationDefinition avatar_BSAD, Playable connection, Animator animator)
    {
        SetupAvatarBSA(source, source_BSAD, avatar, avatar_BSAD);

        m_referenceObject = avatar.AddComponent<TestEgocentricOutput>();
        m_referenceObject.InstanceTargets();
        m_referenceObject.SetBSAComponents(m_sourceBSA, m_destBSA);

        m_egocentricOutput = new EgocentricPlayableOutput(graph, m_referenceObject);
        m_IKChain = ScriptPlayable<EgocentricIKChainBehaviour>.Create(graph);
        m_projPlayable = ScriptPlayable<EgocentricProjectionBehaviour>.Create(graph);

        m_TargetsJob = new DefineTargets();
        m_TargetsJob.Setup(m_referenceObject.targets, animator);
        m_SetTargetsPlayable = AnimationScriptPlayable.Create(graph, m_TargetsJob);

        m_IKJob = new EgocentricIKJob();
        List<HumanBodyBones> chain = new List<HumanBodyBones> { HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftShoulder };
        List<int> indexes = new List<int> { (int)HumanBodyBones.LeftHand, (int)HumanBodyBones.LeftLowerArm, (int)HumanBodyBones.LeftUpperArm, (int)HumanBodyBones.LeftShoulder };
        m_IKJob.setup(animator, chain, m_referenceObject.targets, indexes);
        m_IKPlayable = AnimationScriptPlayable.Create(graph, m_IKJob);

        BuildGraph(graph, connection);
    }

    ~EgocentricGraphHandler()
    {
        m_IKJob.Dispose();
        m_TargetsJob.Dispose();
    }

    private void SetupAvatarBSA(GameObject source, BodySurfaceApproximationDefinition source_BSAD, GameObject dest, BodySurfaceApproximationDefinition dest_BSAD)
    {
        m_sourceBSA = source.AddComponent<BSAComponent>();
        m_sourceBSA.BSAD = source_BSAD;

        m_destBSA = dest.AddComponent<BSAComponent>();
        m_destBSA.BSAD = dest_BSAD;
    }

    private void BuildGraph(PlayableGraph graph, Playable connection)
    {
        PlayableGraphUtility.ConnectNodes(graph, connection, m_IKChain);
        PlayableGraphUtility.ConnectNodes(graph, m_IKChain, m_SetTargetsPlayable);
        PlayableGraphUtility.ConnectNodes(graph, m_SetTargetsPlayable, m_projPlayable);
        PlayableGraphUtility.ConnectNodes(graph, m_projPlayable, m_IKPlayable);
        PlayableGraphUtility.ConnectOutput(m_projPlayable, m_egocentricOutput.output, 1);
    }
}
