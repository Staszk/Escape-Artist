using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionScript : MonoBehaviour
{
    public Shader outlineShader;
    public Color edgeOutline;

    private void Start()
    {
        Shader.SetGlobalColor("_EdgeColor", edgeOutline);
        GetComponent<Camera>().SetReplacementShader(outlineShader, "Occlusion");
    }
}
