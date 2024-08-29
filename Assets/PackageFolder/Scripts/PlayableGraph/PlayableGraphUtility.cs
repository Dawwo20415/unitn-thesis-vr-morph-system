using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public static class PlayableGraphUtility
{
    public static bool ConnectNodes(PlayableGraph graph, Playable output_node, Playable input_node)
    {

        int out_index = FirstFreeOutput(output_node);
        int in_index = FirstFreeInput(input_node);

        ConnectNodesI(graph, output_node, input_node, out_index, in_index);

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

    public static bool ConnectOutput(Playable src_node, PlayableOutput dest_output, int index = -1)
    {
        int src_index = 0;
        if (index == -1)
        {
            src_index = FirstFreeOutput(src_node);
            if (src_index == -1) { src_index = src_node.GetOutputCount(); src_node.SetOutputCount(src_index + 1); }
        } else
        {
            src_node.SetOutputCount(index + 1);
            src_index = index;
        }

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
}
