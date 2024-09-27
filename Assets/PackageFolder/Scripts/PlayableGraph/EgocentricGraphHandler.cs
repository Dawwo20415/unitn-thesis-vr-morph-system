using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class EgocentricGraphHandler
{
    //public Playable lastInPath { get => m_IKPlayable; }

    private BSAComponent m_sourceBSA;
    private BSAComponent m_destBSA;

    private TestEgocentricOutput m_referenceObject;
    private AvatarChainsHandler m_chainHandler;
    private EgocentricPlayableOutput m_egocentricOutput;

    private List<DefineTargets> m_TargetsJobs;
    private List<AnimationScriptPlayable> m_SetTargetPlayables;

    private List<ScriptPlayable<EgocentricProjectionBehaviour>> m_projPlayables;

    private List<ScriptPlayable<TargetDisplacementBehaviour>> m_dispPlayables;

    private List<EgocentricIKJob> m_IKJobs;
    private List<AnimationScriptPlayable> m_IKPlayables;
    

    /*
    public EgocentricGraphHandler(PlayableGraph graph, GameObject source, BodySurfaceApproximationDefinition source_BSAD, GameObject avatar, BodySurfaceApproximationDefinition avatar_BSAD, Playable connection, Animator animator)
    {
        SetupAvatarBSA(source, source_BSAD, avatar, avatar_BSAD);

        m_referenceObject = avatar.AddComponent<TestEgocentricOutput>();
        m_referenceObject.InstanceTargets();
        m_referenceObject.SetBSAComponents(m_sourceBSA, m_destBSA);

        m_egocentricOutput = new EgocentricPlayableOutput(graph, m_referenceObject);
        m_projPlayable = ScriptPlayable<EgocentricProjectionBehaviour>.Create(graph);

        m_TargetsJob = new DefineTargets();
        List<HumanBodyBones> chain = new List<HumanBodyBones> { HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand };
        m_TargetsJob.Setup(m_referenceObject.targets, animator, chain);
        m_SetTargetsPlayable = AnimationScriptPlayable.Create(graph, m_TargetsJob);

        m_IKJob = new EgocentricIKJob();
        List<int> indexes = new List<int> { (int)HumanBodyBones.LeftHand, (int)HumanBodyBones.LeftLowerArm, (int)HumanBodyBones.LeftUpperArm, (int)HumanBodyBones.LeftShoulder };
        m_IKJob.setup(animator, chain, m_referenceObject.targets, indexes);
        m_IKPlayable = AnimationScriptPlayable.Create(graph, m_IKJob);

        BuildGraph(graph, connection);
    }
    */

    public EgocentricGraphHandler(PlayableGraph graph, GameObject source, BodySurfaceApproximationDefinition source_BSAD, GameObject avatar, BodySurfaceApproximationDefinition avatar_BSAD, AvatarChainsHandler handler, Animator animator)
    {
        SetupAvatarBSA(source, source_BSAD, avatar, avatar_BSAD);

        //SETUP REFERENCE DATA OBJECT
        m_referenceObject = avatar.AddComponent<TestEgocentricOutput>();
        m_referenceObject.InstanceTargets();
        m_referenceObject.SetBSAComponents(m_sourceBSA, m_destBSA);

        //SETUP CHAINS
        m_chainHandler = handler;

        foreach(AvatarChainStructure chain in m_chainHandler.chains())
        {
            InstanceChainPlayables(graph, chain, animator, m_referenceObject);
        }

        m_egocentricOutput = new EgocentricPlayableOutput(graph, m_referenceObject);
    }

    ~EgocentricGraphHandler()
    {
        //m_IKJob.Dispose();
        //m_TargetsJob.Dispose();
    }

    private void InstanceLists()
    {
        m_TargetsJobs = new List<DefineTargets>();
        m_SetTargetPlayables = new List<AnimationScriptPlayable>();
        m_projPlayables = new List<ScriptPlayable<EgocentricProjectionBehaviour>>();
        m_dispPlayables = new List<ScriptPlayable<TargetDisplacementBehaviour>>();
        m_IKJobs = new List<EgocentricIKJob>();
        m_IKPlayables = new List<AnimationScriptPlayable>();
    }

    private void SetupAvatarBSA(GameObject source, BodySurfaceApproximationDefinition source_BSAD, GameObject dest, BodySurfaceApproximationDefinition dest_BSAD)
    {
        m_sourceBSA = source.AddComponent<BSAComponent>();
        m_sourceBSA.BSAD = source_BSAD;

        m_destBSA = dest.AddComponent<BSAComponent>();
        m_destBSA.BSAD = dest_BSAD;
    }

    private void InstanceChainPlayables(PlayableGraph graph, AvatarChainStructure structure, Animator animator, TestEgocentricOutput output)
    {
        //DEFINE TARGETS
        DefineTargets target_job = new DefineTargets();
        target_job.Setup(output.targets, animator, structure.chain);
        AnimationScriptPlayable target_playable = AnimationScriptPlayable.Create(graph, target_job);

        m_TargetsJobs.Add(target_job);
        m_SetTargetPlayables.Add(target_playable);

        //INSERT EGOCENTRIC
        foreach (bool is_ego in structure.egocentric)
        {
            if (is_ego)
            {
                m_projPlayables.Add(ScriptPlayable<EgocentricProjectionBehaviour>.Create(graph));
            }
        }

        //INSERT DISPLACEMENTS
        TargetDisplacementBehaviour target_behaviour = new TargetDisplacementBehaviour();
        target_behaviour.Setup(structure.chain, structure.ops);
        ScriptPlayable<TargetDisplacementBehaviour> displacement_playable = ScriptPlayable<TargetDisplacementBehaviour>.Create(graph, target_behaviour);

        m_dispPlayables.Add(displacement_playable);

        //INSERT IK PLAYABLE
        EgocentricIKJob ik_job = new EgocentricIKJob();
        ik_job.setup(animator, structure.chain, output.targets, structure.chain_indexes);
        AnimationScriptPlayable ik_playable = AnimationScriptPlayable.Create(graph, ik_job);

        m_IKJobs.Add(ik_job);
        m_IKPlayables.Add(ik_playable);
    }

    public void BuildGraph(PlayableGraph graph, Playable connection)
    {
        //REBUILD
    }
}
