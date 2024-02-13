using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class is here just to identify what is a LassoObject
 * LassoEnemy and SwingableObject are children of this class.
 */
public class LassoObject : MonoBehaviour
{
    public Renderer materialRenderer;
    public Material originalMaterial;
    public Material selectedMaterial;

    public bool isLassoable { get; set; } = true;
    public bool currentlyLassoed { get; set; } = false;

    public void Start()
    {
        if (materialRenderer == null)
        {
            materialRenderer = GetComponent<Renderer>();
        }
    }
    public void Select()
    {
        if (materialRenderer != null && materialRenderer.material != selectedMaterial)
        {
            materialRenderer.material = selectedMaterial;
        }
    }

    public void Deselect()
    {
        if (materialRenderer != null && materialRenderer.material != originalMaterial)
        {
            materialRenderer.material = originalMaterial;
        }
    }
}
