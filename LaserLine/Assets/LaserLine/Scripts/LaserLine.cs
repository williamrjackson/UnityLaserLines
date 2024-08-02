using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
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

    private Color _innerColor = Color.white;
    [SerializeField]
    [HideInInspector]
    private LineRenderer _colorLine;
    [SerializeField]
    [HideInInspector]
    private LineRenderer _whiteLine;
    private float sourceAlpha = 1f;
    private float goalAlpha;
    private float lastColorChangeTime;
    public LineRenderer ControlLine => _colorLine;

    private LineRenderer WhiteLine
    {
        get
        {
            if (_whiteLine == null)
            {
                _whiteLine = new GameObject("WhiteLine").AddComponent<LineRenderer>();
                _whiteLine.transform.SetParent(transform);
                _whiteLine.transform.localPosition = Vector3.zero;
                _whiteLine.transform.localRotation = Quaternion.identity;
                _whiteLine.useWorldSpace = _colorLine.useWorldSpace;
                _whiteLine.positionCount = _colorLine.positionCount;
                _whiteLine.startWidth = _colorLine.startWidth / (100 / centerGlow);
                _whiteLine.endWidth = _colorLine.endWidth / (100 / centerGlow);
                _whiteLine.numCapVertices = 9;
                _whiteLine.material = innerFadeMaterial;
            }
            return _whiteLine;
        }
    }
    private LineRenderer ColorLine
    {
        get
        {
            if (_colorLine == null)
            {
                _colorLine = GetComponent<LineRenderer>();
                _colorLine.numCapVertices = 9;
                _colorLine.material = outerFadeMaterial;
            }
            return _colorLine;
        }
    }
    void Awake ()
    {
        if (outerFadeMaterial == null)
        {
            Debug.LogError("Outer Fade Material is Missing.");
        }
        if (innerFadeMaterial == null)
        {
            Debug.LogError("Inner Fade Material is Missing.");
        }

        Synchronize();
    }

    void LateUpdate ()
    {
        Synchronize();
        WhiteLine.enabled = ColorLine.enabled;
        if (Application.isPlaying)
        {
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

            Color innerColor = _innerColor;
            innerColor.a = centerIntensity;

            ColorLine.startColor = appliedColor;
            ColorLine.endColor = appliedColor;
            WhiteLine.startColor = innerColor;
            WhiteLine.endColor = innerColor;
        }
    }

    public void Synchronize()
    {
        ColorLine.startColor = color;
        ColorLine.endColor = color;
        WhiteLine.startColor = _innerColor;
        WhiteLine.endColor = _innerColor;
        ColorLine.material = outerFadeMaterial;
        WhiteLine.material = innerFadeMaterial;
        WhiteLine.startWidth = ColorLine.startWidth / (100 / centerGlow);
        WhiteLine.endWidth = ColorLine.endWidth / (100 / centerGlow);

        var positions = new Vector3[ColorLine.positionCount];
        ColorLine.GetPositions(positions);
        WhiteLine.positionCount = positions.Length;
        WhiteLine.SetPositions(positions);
        WhiteLine.useWorldSpace = ColorLine.useWorldSpace;
    }

    public void SetColor(Color newColor)
    {
        color = newColor;
    }
}
