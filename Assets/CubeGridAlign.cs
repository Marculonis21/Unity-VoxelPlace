using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeGridAlign : MonoBehaviour
{
    // snap to this value
    public float snap = 0.01f;
    private Transform _transform;

    // Use this for initialization
    void OnEnable()
    {
        _transform = transform;
        snap = transform.localScale.x;
        _transform.position = GetSharedSnapPosition(_transform.position, snap);
    }

    public static Vector3 GetSharedSnapPosition(Vector3 originalPosition, float snap = 0.01f)
    {
        return new Vector3(GetSnapValue(originalPosition.x, snap), GetSnapValue(originalPosition.y, snap), GetSnapValue(originalPosition.z, snap));
    }

    /// <summary>
    public static float GetSnapValue(float value, float snap = 0.01f)
    {
        return (!Mathf.Approximately(snap, 0f)) ? Mathf.RoundToInt(value / snap) * snap : value;
    }
}
