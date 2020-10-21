using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OutlineReplacement : MonoBehaviour
{
    public Shader outlineShader;

    private void OnEnable()
    {
        GetComponent<Camera>().SetReplacementShader(outlineShader, "Occlusion");
    }
}
