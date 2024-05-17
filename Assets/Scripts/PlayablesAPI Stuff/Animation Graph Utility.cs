using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class AnimationGraphUtility 
{
    public static bool ConnectNodes(PlayableGraph graph, Playable output_node, Playable input_node)
    {
        int out_index = FirstFreeOutput(output_node);
        int in_index = FirstFreeInput(input_node);

        Debug.Log("Out Count [" + out_index + "] In Count [" + in_index + "]");

        if (in_index == -1) { in_index = input_node.GetInputCount(); input_node.SetInputCount(in_index + 1); }
        if (out_index == -1) { out_index = output_node.GetOutputCount(); output_node.SetOutputCount(out_index + 1); }

        Debug.Log("Out Count [" + out_index + "] In Count [" + in_index + "]");

        graph.Connect(output_node, out_index, input_node, in_index);

        input_node.SetInputWeight(in_index, 1.0f);

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
}
