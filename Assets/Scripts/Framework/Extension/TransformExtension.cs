using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension
{
    public static void AddChild(this Transform transform, Transform child)
    {
        child.SetParent(transform);
    }

    public static Transform CreateChild(this Transform transform, string name)
    {
        var go = new GameObject(name);
        var child = go.transform;
        transform.AddChild(child);
        return child;
    }

    public static void SetSizeDelta(this Transform transform, float width, float height)
    {
        var rt = transform as RectTransform;
        rt.sizeDelta = new Vector2(width, height);
    }

    public static void SetSizeWidth(this Transform transform, float width)
    {
        var rt = transform as RectTransform;
        rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
    }

    public static void SetSizeHeight(this Transform transform, float height)
    {
        var rt = transform as RectTransform;
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
    }

    public static void SetAnchoredMin(this Transform transform, float x, float y)
    {
        var rt = transform as RectTransform;
        rt.anchorMin = new Vector2(x, y);
    }

    public static void SetAnchoredMax(this Transform transform, float x, float y)
    {
        var rt = transform as RectTransform;
        rt.anchorMax = new Vector2(x, y);
    }

    public static void SetOffsetMin(this Transform transform, float x, float y)
    {
        var rt = transform as RectTransform;
        rt.offsetMin = new Vector2(x, y);
    }

    public static void SetOffsetMax(this Transform transform, float x, float y)
    {
        var rt = transform as RectTransform;
        rt.offsetMax = new Vector2(x, y);
    }

    public static void ResetRTS(this Transform transform)
    {
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localPosition = Vector3.zero;
    }
}
