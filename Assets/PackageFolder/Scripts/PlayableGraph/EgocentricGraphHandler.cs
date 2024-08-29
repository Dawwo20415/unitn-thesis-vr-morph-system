using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class EgocentricGraphHandler
{
    public Playable lastInPath { get => m_IKChain; }

    private TestEgocentricOutput m_referenceObject;
    private EgocentricPlayableOutput m_egocentricOutput;
    private ScriptPlayable<EgocentricIKChainBehaviour> m_IKChain;

    public EgocentricGraphHandler(PlayableGraph graph, GameObject avatar, Playable connection)
    {
        m_referenceObject = avatar.AddComponent<TestEgocentricOutput>();
        m_egocentricOutput = new EgocentricPlayableOutput(graph, m_referenceObject);
        m_IKChain = ScriptPlayable<EgocentricIKChainBehaviour>.Create(graph);

        BuildGraph(graph, connection);
    }

    ~EgocentricGraphHandler()
    {

    }

    private void BuildGraph(PlayableGraph graph, Playable connection)
    {
        PlayableGraphUtility.ConnectNodes(graph, connection, m_IKChain);
        PlayableGraphUtility.ConnectOutput(m_IKChain, m_egocentricOutput.output);
    }
}
