using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserLine : MonoBehaviour {
    [SerializeField]
    private Color color = Color.red;
    [Range(0,100)]
    public float centerGlow = 25;
    [Range(0, 1)]
    public float centerIntensity = 1f;
    [Range(0, 1)]
    public float pulseWidth = 0f;
    [Range(0, 5)]
    public float pulseLength = 0f;
    public Material innerFadeMaterial;
    public Material outerFadeMaterial;
    public bool useWorldSpace = true;

    private Color m_InnerColor = Color.white;
    private LineRenderer m_ColorLine;
    private LineRenderer m_WhiteLine;
    private float sourceAlpha = 1f;
    private float goalAlpha;
    private float lastColorChangeTime;
    public LineRenderer ControlLine => m_ColorLine;
    void Awake ()
    {
        m_ColorLine = GetComponent<LineRenderer>();
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

        SetPositions();
    }

    void LateUpdate ()
    {
        SetPositions();
        m_WhiteLine.enabled = m_ColorLine.enabled;

        m_WhiteLine.startWidth = m_ColorLine.startWidth / (100 / centerGlow);
        m_WhiteLine.endWidth = m_ColorLine.endWidth / (100 / centerGlow);
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

    public void SetPositions()
    {
        var positions = new Vector3[m_ColorLine.positionCount];
        m_ColorLine.GetPositions(positions);
        m_WhiteLine.positionCount = positions.Length;
        m_WhiteLine.SetPositions(positions);
        m_WhiteLine.useWorldSpace = m_ColorLine.useWorldSpace;
    }

    public void SetColor(Color newColor)
    {
        color = newColor;
    }
}
