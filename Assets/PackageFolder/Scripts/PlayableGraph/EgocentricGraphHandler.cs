using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class EgocentricGraphHandler
{
    public Playable lastInPath { get => m_projPlayable; }

    private TestEgocentricOutput m_referenceObject;
    private EgocentricPlayableOutput m_egocentricOutput;
    private ScriptPlayable<EgocentricIKChainBehaviour> m_IKChain;
    private ScriptPlayable<EgocentricProjectionBehaviour> m_projPlayable;

    private BSAComponent m_sourceBSA;
    private BSAComponent m_destBSA;

    public EgocentricGraphHandler(PlayableGraph graph, GameObject source, BodySurfaceApproximationDefinition source_BSAD, GameObject avatar, BodySurfaceApproximationDefinition avatar_BSAD, Playable connection)
    {
        SetupAvatarBSA(source, source_BSAD, avatar, avatar_BSAD);

        m_referenceObject = avatar.AddComponent<TestEgocentricOutput>();
        m_referenceObject.SetBSAComponents(m_sourceBSA, m_destBSA);

        m_egocentricOutput = new EgocentricPlayableOutput(graph, m_referenceObject);
        m_IKChain = ScriptPlayable<EgocentricIKChainBehaviour>.Create(graph);
        m_projPlayable = ScriptPlayable<EgocentricProjectionBehaviour>.Create(graph);

        BuildGraph(graph, connection);
    }

    ~EgocentricGraphHandler()
    {

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
        PlayableGraphUtility.ConnectNodes(graph, m_IKChain, m_projPlayable);
        PlayableGraphUtility.ConnectOutput(m_projPlayable, m_egocentricOutput.output, 1);
    }
}
