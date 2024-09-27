using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DebugLine
{
    public Vector3 start;
    public Vector3 end;
    public float alpha;

    public DebugLine(Vector3 beginning, Vector3 finish, float transparency)
    {
        start = beginning;
        end = finish;
        alpha = transparency;
    }

    public DebugLine(Vector3 beginning, Vector3 finish)
    {
        start = beginning;
        end = finish;
        alpha = 1.0f;
    }
}

public struct BSACLines
{
    public DebugLine projection;
    public DebugLine faceCA;
    public DebugLine faceCB;
    public float weight;

    public BSACLines(DebugLine a, DebugLine b, DebugLine c, float value)
    {
        projection = a;
        faceCA = b;
        faceCB = c;
        weight = value;

        SetWeight(value);
    }

    public BSACLines(BSACLines lines, float new_value)
    {
        projection = lines.projection;
        faceCA = lines.faceCA;
        faceCB = lines.faceCB;
        weight = new_value;

        SetWeight(new_value);
    }

    public void SetWeight(float value)
    {
        weight = value;
        projection.alpha = value;
        faceCA.alpha = value;
        faceCB.alpha = value;
    }
}

public class EgocentricProjectionDebug
{

    private List<BSACLines> m_Projections;

    public EgocentricProjectionDebug(int size)
    {
        m_Projections = new List<BSACLines>(size);
    }

    public void Add(BSACLines proj)
    {
        m_Projections.Add(proj);
    }

    public void Add(DebugLine a, DebugLine b, DebugLine c, float value)
    {
        m_Projections.Add(new BSACLines(a, b, c, value));
    }

    public void Clear()
    {
        m_Projections.Clear();
    }

    public void Reload(int size)
    {
        Clear();
        m_Projections = new List<BSACLines>(size);
    }

    public void Trim()
    {
        m_Projections.TrimExcess();
    }

    public void Reweight()
    {
        float total = 0.0f;
        float max = 0.0f;

        //NORMALIZE
        foreach (BSACLines lines in m_Projections)
        {
            total += lines.weight;
        }

        for (int i = 0; i < m_Projections.Count; i++)
        {
            float new_weight = m_Projections[i].weight / total;
            BSACLines newLines = new BSACLines(m_Projections[i], new_weight);
            m_Projections[i] = newLines;
        }

        //REMAP
        foreach (BSACLines lines in m_Projections)
        {
            if (lines.weight >= max)
            {
                max = lines.weight;
            }
        }

        for (int i = 0; i < m_Projections.Count; i++)
        {
            float new_weight = Remap(m_Projections[i].weight, 0.0f, max, 0.0f, 1.0f);
            BSACLines newLines = new BSACLines(m_Projections[i], new_weight);
            m_Projections[i] = newLines;
        }

        string debug_list = " [";
        float new_total = 0.0f;
        float new_max = 0.0f;
        for(int i = 0; i < m_Projections.Count; i++)
        {
            BSACLines lines = m_Projections[i];

            new_total += lines.weight;
            if (lines.weight > 0.9f)
            {
                debug_list += "(" + i + ")";
                //Debug.Log(i);
            }

            debug_list += lines.weight + ",";
            if (lines.weight >= new_max)
            {
                new_max = lines.weight;
            }
        }
        debug_list += "]";

        string debug = "total: " + total + " max: " + max + " new total: " + new_total + " new max: " + new_max + debug_list;
        //Debug.Log(debug);
    }

    public void OnGizmoDraw(bool draw_projections, bool draw_components)
    {
        foreach (BSACLines lines in m_Projections)
        {
            if (draw_projections)
            {
                OnGizmoDrawLine(lines.projection, Color.magenta);
            }

            if (draw_components)
            {
                OnGizmoDrawLine(lines.faceCA, Color.black);
                OnGizmoDrawLine(lines.faceCB, Color.black);
            }
        }
    }

    private void OnGizmoDrawLine(DebugLine line, Color col)
    {
        Color color = col;
        color.a = line.alpha;
        Gizmos.color = color;
        Gizmos.DrawLine(line.start, line.end);
    }

    private float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
