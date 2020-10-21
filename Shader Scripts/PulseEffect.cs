using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    public Transform scannerOrigin;
    public Material pulseMaterial;
    public Material chromaticMaterial;
    public float scanDistance;
    private float scanRate;

    private Camera cam;

    private bool scanning;
    public bool chromatic;
    private bool up;
    private float chromaticAmount;

    public PulseColors goodColors;
    public PulseColors badColors;



    private void Start()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.Depth;

        pulseMaterial.SetVector("_WorldSpaceScannerPos", scannerOrigin.position);
        pulseMaterial.SetFloat("_ScanDistance", 0);

        scanning = false;

        scanRate = 50f;
        chromaticAmount = 0.001f;
        up = true;
    }

    private void OnEnable()
    {
        // Subscribe event
        AudienceInteractableManager.ActionInteractableChosen += CreatePulse;
        MainObjective.ActionCollected += Chromatic;
    }

    private void OnDisable()
    {
        // Unsubscribe event
        AudienceInteractableManager.ActionInteractableChosen -= CreatePulse;
    }

    private void Update()
    {
        if (scanning)
        {
            scanDistance += Time.deltaTime * scanRate;
        }


        // FOR DEBUGGING - UNCOMMENT TO TEST
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    Chromatic("", Vector3.zero);
        //}

        if (chromatic)
        {
            if (up)
            {
                chromaticAmount += 0.02f * Time.deltaTime;

                if (chromaticAmount > 0.01)
                {
                    up = false;
                }
            }
            else
            {
                chromaticAmount -= 0.02f * Time.deltaTime;

                if (chromaticAmount < 0)
                {
                    chromatic = false;
                }
            }

            chromaticMaterial.SetFloat("_Amount", chromaticAmount);
        }


        // FOR DEBUGGING - UNCOMMENT TO TEST
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        //    if (Physics.Raycast(ray, out RaycastHit hit))
        //    {
        //        scanning = true;
        //        scanDistance = 0;
        //        scannerOrigin.position = hit.point;
        //    }
        //}
    }

    private void Chromatic(string tag, Vector3 position)
    {
        chromatic = true;
        up = true;
    }

    private void CreatePulse(Vector3 position, bool good)
    {
        if (good)
        {
            pulseMaterial.SetColor("_LeadColor", goodColors.leadingEdgeColor);
            pulseMaterial.SetColor("_MidColor", goodColors.midColor);
            pulseMaterial.SetColor("_TrailColor", goodColors.trailColor);
            pulseMaterial.SetColor("_HBarColor", goodColors.horizontalBarColor);
        }
        else
        {
            pulseMaterial.SetColor("_LeadColor", badColors.leadingEdgeColor);
            pulseMaterial.SetColor("_MidColor", badColors.midColor);
            pulseMaterial.SetColor("_TrailColor", badColors.trailColor);
            pulseMaterial.SetColor("_HBarColor", badColors.horizontalBarColor);
        }

        scanning = true;
        scanDistance = 0;
        scannerOrigin.position = position;
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (scanning)
        {
            pulseMaterial.SetVector("_WorldSpaceScannerPos", scannerOrigin.position);
            pulseMaterial.SetFloat("_ScanDistance", scanDistance);
        }

        if (chromatic)
        {
            Graphics.Blit(source, destination, chromaticMaterial);
        }
        else
        {
            RaycastCornerBlit(source, destination, pulseMaterial);
        } 
    }

    void RaycastCornerBlit(RenderTexture source, RenderTexture dest, Material mat)
    {
        // Compute Frustum Corners
        float camFar = cam.farClipPlane;
        float camFov = cam.fieldOfView;
        float camAspect = cam.aspect;

        float fovWHalf = camFov * 0.5f;

        Vector3 toRight = cam.transform.right * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camAspect;
        Vector3 toTop = cam.transform.up * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

        Vector3 topLeft = (cam.transform.forward - toRight + toTop);
        float camScale = topLeft.magnitude * camFar;

        topLeft.Normalize();
        topLeft *= camScale;

        Vector3 topRight = (cam.transform.forward + toRight + toTop);
        topRight.Normalize();
        topRight *= camScale;

        Vector3 bottomRight = (cam.transform.forward + toRight - toTop);
        bottomRight.Normalize();
        bottomRight *= camScale;

        Vector3 bottomLeft = (cam.transform.forward - toRight - toTop);
        bottomLeft.Normalize();
        bottomLeft *= camScale;

        // Custom Blit, encoding Frustum Corners as additional Texture Coordinates
        RenderTexture.active = dest;

        mat.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();

        mat.SetPass(0);

        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.MultiTexCoord(1, bottomLeft);
        GL.Vertex3(0.0f, 0.0f, 0.0f);

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.MultiTexCoord(1, bottomRight);
        GL.Vertex3(1.0f, 0.0f, 0.0f);

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.MultiTexCoord(1, topRight);
        GL.Vertex3(1.0f, 1.0f, 0.0f);

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.MultiTexCoord(1, topLeft);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();
    }
}

[System.Serializable]
public class PulseColors
{
    public Color32 leadingEdgeColor;
    public Color32 midColor;
    public Color32 trailColor;
    public Color32 horizontalBarColor;
}
