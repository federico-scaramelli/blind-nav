using System;
using UnityEngine;
using UnityEngine.UI;

public class BlackMaskSwitcher : MonoBehaviour
{
    [SerializeField] private Image blackMask;
    private MeshRenderer[] meshRenderers;

    private void Awake()
    {
        meshRenderers = FindObjectsOfType<MeshRenderer>();
        foreach (var m in meshRenderers)
            m.enabled = !blackMask.enabled;
    }

    public void SwitchMask()
    {
        blackMask.enabled = !blackMask.enabled;
        foreach (var m in meshRenderers)
            m.enabled = !blackMask.enabled;
    }
}
