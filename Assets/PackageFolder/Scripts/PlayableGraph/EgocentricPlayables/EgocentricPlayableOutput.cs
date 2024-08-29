using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class EgocentricPlayableOutput
{
    public ScriptPlayableOutput output { get => m_playable; }

    private ScriptPlayableOutput m_playable;
    private TestEgocentricOutput m_userData;

    public EgocentricPlayableOutput(PlayableGraph graph, TestEgocentricOutput ref_object)
    {
        m_playable = ScriptPlayableOutput.Create(graph, "EgocentricPlayableOutput");
        m_userData = ref_object;
        m_playable.SetUserData(m_userData);
    }

    ~EgocentricPlayableOutput()
    {

    }
}
