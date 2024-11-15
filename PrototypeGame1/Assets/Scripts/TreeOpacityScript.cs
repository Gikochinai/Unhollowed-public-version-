using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeOpacityScript : MonoBehaviour
{
    public Material transparentMaterial;
    private Material originalMaterial;
    private Renderer treeRenderer;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("TreeTransparency script initialized on: " + gameObject.name);
        treeRenderer = GetComponent<Renderer>();
        originalMaterial = treeRenderer.material;

        // Make the tree invisible at the start
        if (treeRenderer != null)
        {
            treeRenderer.enabled = false; // Hide the tree initially
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("The player entered the box collider range");
        if (other.transform.parent != null && other.transform.parent.CompareTag("Player"))
        {
            Debug.Log("Player has entered the trigger.");
            treeRenderer.enabled = true; // Make the tree visible when the player enters
            SetMaterialTransparent();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("The player exited the box collider range");
        if (other.transform.parent != null && other.transform.parent.CompareTag("Player"))
        {
            Debug.Log("Player has exited the trigger.");
            SetMaterialOpaque();
        }
    }

    void SetMaterialTransparent()
    {
        Debug.Log("Setting material to transparent");
        treeRenderer.material = transparentMaterial;
        treeRenderer.material.SetFloat("_Surface", 1);
        treeRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        treeRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        treeRenderer.material.SetInt("_ZWrite", 0);
        treeRenderer.material.EnableKeyword("_ALPHABLEND_ON");
        treeRenderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        Color color = treeRenderer.material.color;
        color.a = 0.5f;
        treeRenderer.material.color = color;
    }

    void SetMaterialOpaque()
    {
        Debug.Log("Setting material to opaque");
        treeRenderer.material.SetFloat("_Surface", 0);
        treeRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        treeRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        treeRenderer.material.SetInt("_ZWrite", 1);
        treeRenderer.material.DisableKeyword("_ALPHABLEND_ON");
        treeRenderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

        Color color = treeRenderer.material.color;
        color.a = 1.0f;
        treeRenderer.material.color = color;
    }
}
