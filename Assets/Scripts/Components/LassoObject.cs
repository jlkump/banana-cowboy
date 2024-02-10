using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class is here just to identify what is a LassoObject
 * LassoEnemy and SwingableObject are children of this class.
 */
public class LassoObject : MonoBehaviour
{
    public Material originalMaterial;
    public Material selectedMaterial;
    // Start is called before the first frame update
    void Start()
    {
        //renderer = gameObject.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select()
    {
        if (GetComponent<Renderer>() != null && GetComponent<Renderer>().material != selectedMaterial)
        {
            GetComponent<Renderer>().material = selectedMaterial;
        }
    }

    public void Deselect()
    {
        if (GetComponent<Renderer>() != null && GetComponent<Renderer>().material != originalMaterial)
            GetComponent<Renderer>().material = originalMaterial;
    }
}
