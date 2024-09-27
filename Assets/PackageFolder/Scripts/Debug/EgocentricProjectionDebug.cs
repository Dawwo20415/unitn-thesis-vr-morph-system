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

    public void SetWeight(float weight)
    {
        projection.alpha = weight;
        faceCA.alpha = weight;
        faceCB.alpha = weight;
    }
}

public class EgocentricProjectionDebug
{

    private List<DebugLine> m_Projections;
    private List<DebugLine> m_SurfaceComponents;

    public EgocentricProjectionDebug(int size)
    {
        m_Projections = new List<DebugLine>(size);
        m_SurfaceComponents = new List<DebugLine>(size * 2);
    }

    public void AddProjection(Vector3 beginning, Vector3 finish, float transparency)
    {
        m_Projections.Add(new DebugLine(beginning, finish, transparency));
    }

    public void AddProjection(DebugLine line)
    {
        m_Projections.Add(line);
    }

    public void AddComponent(Vector3 beginning, Vector3 finish, float transparency)
    {
        m_SurfaceComponents.Add(new DebugLine(beginning, finish, transparency));
    }

    public void AddComponent(DebugLine line)
    {
        m_SurfaceComponents.Add(line);
    }

    public void Clear()
    {
        m_Projections.Clear();
        m_SurfaceComponents.Clear();
    }

    public void Trim()
    {
        m_SurfaceComponents.TrimExcess();
        m_SurfaceComponents.TrimExcess();
    }

    public void Reweight()
    {
        float total = 0.0f;

        foreach (DebugLine line in m_Projections) { total += line.alpha; }
        foreach (DebugLine line in m_SurfaceComponents) { total += line.alpha; }

        for (int i = 0; i < m_Projections.Count; i++)
        {
            DebugLine newLine = new DebugLine(m_Projections[i].start, m_Projections[i].end, m_Projections[i].alpha / total);
            m_Projections[i] = newLine;
        }

        for (int i = 0; i < m_SurfaceComponents.Count; i++)
        {
            DebugLine newLine = new DebugLine(m_SurfaceComponents[i].start, m_SurfaceComponents[i].end, m_SurfaceComponents[i].alpha / total);
            m_SurfaceComponents[i] = newLine;
        }
    }

    public void OnGizmoDraw(bool draw_projections, bool draw_components)
    {
        if (draw_projections)
        {
            foreach (DebugLine line in m_Projections)
            {
                OnGizmoDrawLine(line, Color.magenta);
            }
        }

        if (draw_components)
        {
            foreach (DebugLine line in m_Projections)
            {
                OnGizmoDrawLine(line, Color.black);
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
}
