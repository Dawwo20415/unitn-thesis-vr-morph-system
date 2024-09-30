using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class EgocentricGraphHandler
{
    public Playable lastInPath { get => m_endPlayable; }

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

    private Playable m_startPlayable;
    private Playable m_endPlayable;
    private Playable m_outputConnectionPlayable;

    //WIP
    private AnimationPlayableOutput m_testPlayableOutput;
    public Playable firstInSecondStream;

    public EgocentricGraphHandler(PlayableGraph graph, GameObject source, BodySurfaceApproximationDefinition source_BSAD, GameObject avatar, BodySurfaceApproximationDefinition avatar_BSAD, AvatarChainsHandler handler, Animator animator)
    {
        SetupAvatarBSA(source, source_BSAD, avatar, avatar_BSAD);
        InstanceLists();

        //SETUP SECOND ANIMATION OUTPUT
        m_testPlayableOutput = AnimationPlayableOutput.Create(graph, "Optitrack direct output", animator);

        //SETUP REFERENCE DATA OBJECT
        m_referenceObject = avatar.AddComponent<TestEgocentricOutput>();
        m_referenceObject.SetBSAComponents(m_sourceBSA, m_destBSA);
        m_referenceObject.InstanceTargets();

        //SETUP CHAINS
        m_chainHandler = handler;

        foreach(AvatarChainStructure chain in m_chainHandler.chains())
        {
            InstanceChainPlayables(graph, chain, animator, m_referenceObject);
        }

        m_egocentricOutput = new EgocentricPlayableOutput(graph, m_referenceObject);

        PlayableGraphUtility.ConnectOutput(m_outputConnectionPlayable, m_egocentricOutput.output);
    }

    ~EgocentricGraphHandler()
    {
        foreach (EgocentricIKJob job in m_IKJobs)
        {
            job.Dispose();
        }

        foreach (DefineTargets job in m_TargetsJobs)
        {
            job.Dispose();
        }
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
        List<ScriptPlayable<EgocentricProjectionBehaviour>> egoPlayables = new List<ScriptPlayable<EgocentricProjectionBehaviour>>();
        for (int i = 0; i < structure.egocentric.Count; i++)
        {
            if (structure.egocentric[i])
            {
                EgocentricProjectionBehaviour behaviour = new EgocentricProjectionBehaviour();
                behaviour.Setup(structure.chain[i]);
                ScriptPlayable<EgocentricProjectionBehaviour> playable = ScriptPlayable<EgocentricProjectionBehaviour>.Create(graph, behaviour);
                m_projPlayables.Add(playable);
                egoPlayables.Add(playable);
            }
        }

        //INSERT DISPLACEMENTS
        TargetDisplacementBehaviour target_behaviour = new TargetDisplacementBehaviour();
        target_behaviour.Setup(structure.chain, structure.ops);
        ScriptPlayable<TargetDisplacementBehaviour> displacement_playable = ScriptPlayable<TargetDisplacementBehaviour>.Create(graph, target_behaviour);

        m_dispPlayables.Add(displacement_playable);

        //INSERT IK PLAYABLE
        //EgocentricIKJob ik_job = new EgocentricIKJob();
        //ik_job.setup(animator, structure.chain, output.targets, structure.chain_indexes);
        //AnimationScriptPlayable ik_playable = AnimationScriptPlayable.Create(graph, ik_job);

        //m_IKJobs.Add(ik_job);
        //m_IKPlayables.Add(ik_playable);

        //CONNECT PLAYABLES
#if false
        if (m_startPlayable.IsNull())
        {
            m_startPlayable = target_playable;
        } else
        {
            PlayableGraphUtility.ConnectNodes(graph, m_endPlayable, target_playable);
        }

        if (egoPlayables.Count == 0)
        {
            PlayableGraphUtility.ConnectNodes(graph, target_playable, displacement_playable);
        } else
        {
            Playable previous = target_playable;
            foreach (Playable playable in egoPlayables)
            {
                PlayableGraphUtility.ConnectNodes(graph, previous, playable);
                previous = playable;
            }
            PlayableGraphUtility.ConnectNodes(graph, previous, displacement_playable);
        }
        PlayableGraphUtility.ConnectNodes(graph, displacement_playable, ik_playable);
#else

        Playable previous = egoPlayables[0];
        for (int i = 1; i < egoPlayables.Count; i++)
        {
            PlayableGraphUtility.ConnectNodes(graph, previous, egoPlayables[i]);
            previous = egoPlayables[i];
        }
        PlayableGraphUtility.ConnectNodes(graph, previous, displacement_playable);
        //PlayableGraphUtility.ConnectOutput(displacement_playable, m_testPlayableOutput);
#endif

        //target_playable.SetOutputCount(2);
        //ik_playable.SetInputCount(1);
        //PlayableGraphUtility.ConnectNodesI(graph, target_playable, ik_playable, 1, 0);
        //firstInSecondStream = ik_playable;
        //m_endPlayable = ik_playable;
        m_outputConnectionPlayable = displacement_playable;
    }

    public void ConnectGraph(PlayableGraph graph, Playable connection)
    {
        Playable previous = connection;
        //OTHER OUT
        for (int i = 0; i < m_SetTargetPlayables.Count; i++)
        {
            PlayableGraphUtility.ConnectNodes(graph, previous, m_SetTargetPlayables[i]);
            previous = m_SetTargetPlayables[i];
        }

        PlayableGraphUtility.ConnectOutput(previous, m_testPlayableOutput);

        //MAIN OUT
        //PlayableGraphUtility.ConnectNodes(graph, connection, m_startPlayable);
    }
}

public class EgocentricHandler
{
    private BSAComponent m_sourceBSA;
    private BSAComponent m_destBSA;

    private TestEgocentricOutput m_referenceObject;
    private AvatarChainsHandler m_chainHandler;

    private AvatarTargetsComponent m_tComponent;

    public EgocentricHandler(GameObject source, BodySurfaceApproximationDefinition source_BSAD, GameObject avatar, BodySurfaceApproximationDefinition avatar_BSAD, AvatarChainsHandler handler)
    {
        SetupAvatarBSA(source, source_BSAD, avatar, avatar_BSAD);

        //SETUP REFERENCE DATA OBJECT
        m_referenceObject = avatar.AddComponent<TestEgocentricOutput>();
        m_referenceObject.SetBSAComponents(m_sourceBSA, m_destBSA);
        m_referenceObject.InstanceTargets();

        //TARGETS
        m_tComponent = avatar.AddComponent<AvatarTargetsComponent>();
        m_tComponent.InstanceTargets();

        //SETUP CHAINS
        m_chainHandler = handler;

        foreach (AvatarChainStructure structure in m_chainHandler.chains())
        {
            for (int i = 0; i < structure.chain.Count; i++)
            {
                if (structure.egocentric[i])
                {
                    m_tComponent.RegisterEgocentricBone((int)structure.chain[i], structure.ops[i]);
                } else
                {
                    m_tComponent.RegisterBone((int)structure.chain[i], structure.ops[i]);
                }
            }
        }
    }

    private void SetupAvatarBSA(GameObject source, BodySurfaceApproximationDefinition source_BSAD, GameObject dest, BodySurfaceApproximationDefinition dest_BSAD)
    {
        m_sourceBSA = source.AddComponent<BSAComponent>();
        m_sourceBSA.BSAD = source_BSAD;

        m_destBSA = dest.AddComponent<BSAComponent>();
        m_destBSA.BSAD = dest_BSAD;
    }

    public void Project(HumanBodyBones hbb)
    {
        Vector3 previous = m_referenceObject.GetTarget(hbb);
        Vector3 result = m_referenceObject.Calculate(hbb);
    }

    public void Targets(Animator animator)
    {
        m_tComponent.SetTargets(animator);
        m_tComponent.CompoundOperations();
    }
}
