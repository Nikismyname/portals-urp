using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RenderPipeline = UnityEngine.Rendering.RenderPipelineManager;

public class PortalCamera : MonoBehaviour
{
    [SerializeField]
    private Portal[] portals = new Portal[2];

    [SerializeField]
    private Camera portalCamera;

    [SerializeField]
    private int iterations = 7;

    private RenderTexture tempTexture1;
    private RenderTexture tempTexture2;

    private Camera mainCamera;

    private void Awake()
    {
        this.mainCamera = this.GetComponent<Camera>();

        this.tempTexture1 = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        this.tempTexture2 = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
    }

    private void Start()
    {
        this.portals[0].Renderer.material.mainTexture = this.tempTexture1;
        this.portals[1].Renderer.material.mainTexture = this.tempTexture2;
    }

    private void OnEnable()
    {
        RenderPipeline.beginCameraRendering += this.UpdateCamera;
    }

    private void OnDisable()
    {
        RenderPipeline.beginCameraRendering -= this.UpdateCamera;
    }

    void UpdateCamera(ScriptableRenderContext SRC, Camera camera)
    {
        if (!this.portals[0].IsPlaced || !this.portals[1].IsPlaced)
        {
            return;
        }

        if (this.portals[0].Renderer.isVisible)
        {
            this.portalCamera.targetTexture = this.tempTexture1;
            for (int i = this.iterations - 1; i >= 0; --i)
            {
                this.RenderCamera(this.portals[0], this.portals[1], i, SRC);
            }
        }

        if(this.portals[1].Renderer.isVisible)
        {
            this.portalCamera.targetTexture = this.tempTexture2;
            for (int i = this.iterations - 1; i >= 0; --i)
            {
                this.RenderCamera(this.portals[1], this.portals[0], i, SRC);
            }
        }
    }

    private void RenderCamera(Portal inPortal, Portal outPortal, int iterationID, ScriptableRenderContext SRC)
    {
        Transform inTransform = inPortal.transform;
        Transform outTransform = outPortal.transform;

        Transform cameraTransform = this.portalCamera.transform;
        cameraTransform.position = this.transform.position;
        cameraTransform.rotation = this.transform.rotation;

        for(int i = 0; i <= iterationID; ++i)
        {
            // Position the camera behind the other portal.
            Vector3 relativePos = inTransform.InverseTransformPoint(cameraTransform.position);
            relativePos = Quaternion.Euler(0.0f, 180.0f, 0.0f) * relativePos;
            cameraTransform.position = outTransform.TransformPoint(relativePos);

            // Rotate the camera to look through the other portal.
            Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * cameraTransform.rotation;
            relativeRot = Quaternion.Euler(0.0f, 180.0f, 0.0f) * relativeRot;
            cameraTransform.rotation = outTransform.rotation * relativeRot;
        }

        // Set the camera's oblique view frustum.
        Plane p = new Plane(-outTransform.forward, outTransform.position);
        Vector4 clipPlaneWorldSpace = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
        Vector4 clipPlaneCameraSpace =
            Matrix4x4.Transpose(Matrix4x4.Inverse(this.portalCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;

        var newMatrix = this.mainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        this.portalCamera.projectionMatrix = newMatrix;

        // Render the camera to its render target.
        UniversalRenderPipeline.RenderSingleCamera(SRC, this.portalCamera);
        // UniversalRenderPipeline.SubmitRenderRequest(this.portalCamera, new UniversalRenderPipeline.SingleCameraRequest(){});
    }
}
