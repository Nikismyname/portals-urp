using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Portal : MonoBehaviour
{
    [field: SerializeField]
    public Portal OtherPortal { get; private set; }

    [SerializeField]
    private Renderer outlineRenderer;

    [field: SerializeField]
    public Color PortalColour { get; private set; }

    [SerializeField]
    private LayerMask placementMask;

    [SerializeField]
    private Transform testTransform;

    private List<PortalableObject> portalObjects = new List<PortalableObject>();
    public bool IsPlaced { get; private set; } = false;
    private Collider wallCollider;

    // Components.
    public Renderer Renderer { get; private set; }
    private new BoxCollider collider;

    private void Awake()
    {
        this.collider = this.GetComponent<BoxCollider>();
        this.Renderer = this.GetComponent<Renderer>();
    }

    private void Start()
    {
        this.outlineRenderer.material.SetColor("_OutlineColour", this.PortalColour);

        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        this.Renderer.enabled = this.OtherPortal.IsPlaced;

        for (int i = 0; i < this.portalObjects.Count; ++i)
        {
            Vector3 objPos = this.transform.InverseTransformPoint(this.portalObjects[i].transform.position);

            if (objPos.z > 0.0f)
            {
                this.portalObjects[i].Warp();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var obj = other.GetComponent<PortalableObject>();
        if (obj != null)
        {
            this.portalObjects.Add(obj);
            obj.SetIsInPortal(this, this.OtherPortal, this.wallCollider);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var obj = other.GetComponent<PortalableObject>();

        if(this.portalObjects.Contains(obj))
        {
            this.portalObjects.Remove(obj);
            obj.ExitPortal(this.wallCollider);
        }
    }

    public bool PlacePortal(Collider wallCollider, Vector3 pos, Quaternion rot)
    {
        this.testTransform.position = pos;
        this.testTransform.rotation = rot;
        this.testTransform.position -= this.testTransform.forward * 0.001f;

        this.FixOverhangs();
        this.FixIntersects();

        if (this.CheckOverlap())
        {
            this.wallCollider = wallCollider;
            this.transform.position = this.testTransform.position;
            this.transform.rotation = this.testTransform.rotation;

            this.gameObject.SetActive(true);
            this.IsPlaced = true;
            return true;
        }

        return false;
    }

    // Ensure the portal cannot extend past the edge of a surface.
    private void FixOverhangs()
    {
        var testPoints = new List<Vector3>
        {
            new Vector3(-1.1f,  0.0f, 0.1f),
            new Vector3( 1.1f,  0.0f, 0.1f),
            new Vector3( 0.0f, -2.1f, 0.1f),
            new Vector3( 0.0f,  2.1f, 0.1f)
        };

        var testDirs = new List<Vector3>
        {
             Vector3.right,
            -Vector3.right,
             Vector3.up,
            -Vector3.up
        };

        for(int i = 0; i < 4; ++i)
        {
            RaycastHit hit;
            Vector3 raycastPos = this.testTransform.TransformPoint(testPoints[i]);
            Vector3 raycastDir = this.testTransform.TransformDirection(testDirs[i]);

            if(Physics.CheckSphere(raycastPos, 0.05f, this.placementMask))
            {
                break;
            }
            else if(Physics.Raycast(raycastPos, raycastDir, out hit, 2.1f, this.placementMask))
            {
                var offset = hit.point - raycastPos;
                this.testTransform.Translate(offset, Space.World);
            }
        }
    }

    // Ensure the portal cannot intersect a section of wall.
    private void FixIntersects()
    {
        var testDirs = new List<Vector3>
        {
             Vector3.right,
            -Vector3.right,
             Vector3.up,
            -Vector3.up
        };

        var testDists = new List<float> { 1.1f, 1.1f, 2.1f, 2.1f };

        for (int i = 0; i < 4; ++i)
        {
            RaycastHit hit;
            Vector3 raycastPos = this.testTransform.TransformPoint(0.0f, 0.0f, -0.1f);
            Vector3 raycastDir = this.testTransform.TransformDirection(testDirs[i]);

            if (Physics.Raycast(raycastPos, raycastDir, out hit, testDists[i], this.placementMask))
            {
                var offset = (hit.point - raycastPos);
                var newOffset = -raycastDir * (testDists[i] - offset.magnitude);
                this.testTransform.Translate(newOffset, Space.World);
            }
        }
    }

    // Once positioning has taken place, ensure the portal isn't intersecting anything.
    private bool CheckOverlap()
    {
        var checkExtents = new Vector3(0.9f, 1.9f, 0.05f);

        var checkPositions = new Vector3[]
        {
            this.testTransform.position + this.testTransform.TransformVector(new Vector3( 0.0f,  0.0f, -0.1f)), this.testTransform.position + this.testTransform.TransformVector(new Vector3(-1.0f, -2.0f, -0.1f)), this.testTransform.position + this.testTransform.TransformVector(new Vector3(-1.0f,  2.0f, -0.1f)), this.testTransform.position + this.testTransform.TransformVector(new Vector3( 1.0f, -2.0f, -0.1f)), this.testTransform.position + this.testTransform.TransformVector(new Vector3( 1.0f,  2.0f, -0.1f)), this.testTransform.TransformVector(new Vector3(0.0f, 0.0f, 0.2f))
        };

        // Ensure the portal does not intersect walls.
        var intersections = Physics.OverlapBox(checkPositions[0], checkExtents, this.testTransform.rotation, this.placementMask);

        if(intersections.Length > 1)
        {
            return false;
        }
        else if(intersections.Length == 1) 
        {
            // We are allowed to intersect the old portal position.
            if (intersections[0] != this.collider)
            {
                return false;
            }
        }

        // Ensure the portal corners overlap a surface.
        bool isOverlapping = true;

        for(int i = 1; i < checkPositions.Length - 1; ++i)
        {
            isOverlapping &= Physics.Linecast(checkPositions[i], 
                checkPositions[i] + checkPositions[checkPositions.Length - 1], this.placementMask);
        }

        return isOverlapping;
    }

    public void RemovePortal()
    {
        this.gameObject.SetActive(false);
        this.IsPlaced = false;
    }
}
