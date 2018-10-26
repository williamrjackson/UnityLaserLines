using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserLine : MonoBehaviour {
    [SerializeField]
    private Color color = Color.red;
    public float width = .5f;
    [Range(0,100)]
    public float centerGlow = 25;
    [Range(0, 1)]
    public float centerIntensity = 1f;
    [Range(0, 1)]
    public float pulseWidth = 0f;
    [Range(0, 5)]
    public float pulseLength = 0f;
    public bool updateInspectorPositionsInPlaymode = false;
    public Vector3[] positions = { Vector3.zero, new Vector3( 0, 0, 10 ) };
    public Material innerFadeMaterial;
    public Material outerFadeMaterial;
    public bool useWorldSpace = true;

    private Color m_InnerColor = Color.white;
    private LineRenderer m_ColorLine;
    private LineRenderer m_WhiteLine;
    private bool m_IsVisible = true;
    private float sourceAlpha = 1f;
    private float goalAlpha;
    private float lastColorChangeTime;

    void Awake ()
    {
        GameObject colorGO = new GameObject("ColorLine");
        colorGO.transform.parent = transform;            
        colorGO.transform.localPosition = Vector3.zero;
        m_ColorLine = colorGO.AddComponent<LineRenderer>();
        m_ColorLine.numCapVertices = 9;

        GameObject whiteGO = new GameObject("WhiteLine");
        whiteGO.transform.parent = transform;
        whiteGO.transform.localPosition = Vector3.zero;
        m_WhiteLine = whiteGO.AddComponent<LineRenderer>();
        m_WhiteLine.numCapVertices = 9;

        if (outerFadeMaterial != null)
        {
            m_ColorLine.material = outerFadeMaterial;
        }
        else
        {
            Debug.LogError("Outer Fade Material is Missing.");
        }
        if (innerFadeMaterial != null)
        {
            m_WhiteLine.material = innerFadeMaterial;
        }
        else
        {
            Debug.LogError("Inner Fade Material is Missing.");
        }

        SetPositions( positions );
    }

    void Update ()
    {
        if (useWorldSpace != m_ColorLine.useWorldSpace) m_ColorLine.useWorldSpace = useWorldSpace;
        if (useWorldSpace != m_WhiteLine.useWorldSpace) m_WhiteLine.useWorldSpace = useWorldSpace;
        if (updateInspectorPositionsInPlaymode) SetPositions(positions);
        m_ColorLine.enabled = m_IsVisible;
        m_WhiteLine.enabled = m_IsVisible;

        m_ColorLine.startWidth = width;
        m_ColorLine.endWidth = width;

        m_WhiteLine.startWidth = width / (100 / centerGlow);
        m_WhiteLine.endWidth = width / (100 / centerGlow);
        Color appliedColor = color;
        if (pulseLength > 0 && pulseWidth > 0)
        {
            if (goalAlpha > sourceAlpha)
            {
                sourceAlpha = 1f - pulseWidth;
            }
            else
            {
                goalAlpha = 1f - pulseWidth;
            }
            float percentage = (Time.time - lastColorChangeTime) / pulseLength;
            percentage = Mathf.Clamp01(percentage);
            appliedColor.a = Mathf.Lerp(sourceAlpha, goalAlpha, percentage);
            if (percentage == 1f)
            {
                lastColorChangeTime = Time.time;

                // Switch alpha fade direction
                float temp = sourceAlpha;
                sourceAlpha = goalAlpha;
                goalAlpha = temp;
            }
        }

        Color innerColor = m_InnerColor;
        innerColor.a = centerIntensity;

        m_ColorLine.startColor = appliedColor;
        m_ColorLine.endColor = appliedColor;
        m_WhiteLine.startColor = innerColor;
        m_WhiteLine.endColor = innerColor;

    }

    public void SetPositions(Vector3[] newPositions)
    {
        positions = newPositions;
        m_ColorLine.positionCount = positions.Length;
        m_ColorLine.SetPositions( positions );
        m_WhiteLine.positionCount = positions.Length;
        m_WhiteLine.SetPositions( positions );
    }

    public bool SetPosition(int index, Vector3 newPosition)
    {
        if (index > positions.Length)
        {
            Debug.Log("Line Index Out of Range.");
            return false;
        }
        positions[index] = newPosition;
        SetPositions(positions);
        return true;
    }

    public Vector3 GetPosition(int index)
    {
        return positions[index];
    }

    public void SetColor(Color newColor)
    {
        color = newColor;
    }

    public int numPositions
    {
        get
        {
            return positions.Length;
        }
        set
        {
            positions = new Vector3[value];
        }
    }

    public bool Visible
    {
        get
        {   
            return m_IsVisible;
        }
        set
        {
            m_IsVisible = value;
        }
    }
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = color;
            for (int i = 0; i < positions.Length - 1; i++)
            {
                if (useWorldSpace)
                {
                    Gizmos.DrawRay(positions[i], (positions[i + 1] - positions[i]).normalized * Vector3.Distance(positions[i], positions[i + 1]));
                }
                else
                {
                    Vector3 a = transform.TransformPoint(positions[i]);
                    Vector3 b = transform.TransformPoint(positions[i + 1]);
                    Gizmos.DrawRay(a, transform.TransformDirection((b - a).normalized * Vector3.Distance(a, b)));
                }
            }
        }
    }
}
