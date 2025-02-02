using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PortalableObject : MonoBehaviour
{
    private GameObject cloneObject;

    private int inPortalCount = 0;
    
    private Portal inPortal;
    private Portal outPortal;

    private new Rigidbody rigidbody;
    protected new Collider collider;

    private static readonly Quaternion halfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);

    protected virtual void Awake()
    {
        this.cloneObject = new GameObject();
        this.cloneObject.SetActive(false);
        var meshFilter = this.cloneObject.AddComponent<MeshFilter>();
        var meshRenderer = this.cloneObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = this.GetComponent<MeshFilter>().mesh;
        meshRenderer.materials = this.GetComponent<MeshRenderer>().materials;
        this.cloneObject.transform.localScale = this.transform.localScale;

        this.rigidbody = this.GetComponent<Rigidbody>();
        this.collider = this.GetComponent<Collider>();
    }

    private void LateUpdate()
    {
        if(this.inPortal == null || this.outPortal == null)
        {
            return;
        }

        if(this.cloneObject.activeSelf && this.inPortal.IsPlaced && this.outPortal.IsPlaced)
        {
            var inTransform = this.inPortal.transform;
            var outTransform = this.outPortal.transform;

            // Update position of clone.
            Vector3 relativePos = inTransform.InverseTransformPoint(this.transform.position);
            relativePos = halfTurn * relativePos;
            this.cloneObject.transform.position = outTransform.TransformPoint(relativePos);

            // Update rotation of clone.
            Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * this.transform.rotation;
            relativeRot = halfTurn * relativeRot;
            this.cloneObject.transform.rotation = outTransform.rotation * relativeRot;
        }
        else
        {
            this.cloneObject.transform.position = new Vector3(-1000.0f, 1000.0f, -1000.0f);
        }
    }

    public void SetIsInPortal(Portal inPortal, Portal outPortal, Collider wallCollider)
    {
        this.inPortal = inPortal;
        this.outPortal = outPortal;

        Physics.IgnoreCollision(this.collider, wallCollider);

        this.cloneObject.SetActive(false);

        ++this.inPortalCount;
    }

    public void ExitPortal(Collider wallCollider)
    {
        Physics.IgnoreCollision(this.collider, wallCollider, false);
        --this.inPortalCount;

        if (this.inPortalCount == 0)
        {
            this.cloneObject.SetActive(false);
        }
    }

    public virtual void Warp()
    {
        var inTransform = this.inPortal.transform;
        var outTransform = this.outPortal.transform;

        // Update position of object.
        Vector3 relativePos = inTransform.InverseTransformPoint(this.transform.position);
        relativePos = halfTurn * relativePos;
        this.transform.position = outTransform.TransformPoint(relativePos);

        // Update rotation of object.
        Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * this.transform.rotation;
        relativeRot = halfTurn * relativeRot;
        this.transform.rotation = outTransform.rotation * relativeRot;

        // Update velocity of rigidbody.
        Vector3 relativeVel = inTransform.InverseTransformDirection(this.rigidbody.linearVelocity);
        relativeVel = halfTurn * relativeVel;
        this.rigidbody.linearVelocity = outTransform.TransformDirection(relativeVel);

        // Swap portal references.
        var tmp = this.inPortal;
        this.inPortal = this.outPortal;
        this.outPortal = tmp;
    }
}
