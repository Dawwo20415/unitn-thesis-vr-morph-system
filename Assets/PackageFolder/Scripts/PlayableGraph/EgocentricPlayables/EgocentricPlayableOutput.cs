using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class EgocentricPlayableOutput
{
    private ScriptPlayableOutput m_playable;

    public EgocentricPlayableOutput(PlayableGraph graph)
    {
        m_playable = ScriptPlayableOutput.Create(graph, "EgocentricPlayableOutput");
    }

    ~EgocentricPlayableOutput()
    {

    }
}
