using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CameraMove : MonoBehaviour
{
    private const float moveSpeed = 7.5f;
    private const float cameraSpeed = 3.0f;

    public Quaternion TargetRotation { private set; get; }
    
    private Vector3 moveVector = Vector3.zero;
    private float moveY = 0.0f;

    private new Rigidbody rigidbody;

    private void Awake()
    {
        this.rigidbody = this.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;

        this.TargetRotation = this.transform.rotation;
    }

    private void Update()
    {
        // Rotate the camera.
        var rotation = new Vector2(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
        var targetEuler = this.TargetRotation.eulerAngles + (Vector3)rotation * cameraSpeed;
        if(targetEuler.x > 180.0f)
        {
            targetEuler.x -= 360.0f;
        }
        targetEuler.x = Mathf.Clamp(targetEuler.x, -75.0f, 75.0f);
        this.TargetRotation = Quaternion.Euler(targetEuler);

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, this.TargetRotation, 
            Time.deltaTime * 15.0f);

        // Move the camera.
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        this.moveVector = new Vector3(x, 0.0f, z) * moveSpeed;

        this.moveY = Input.GetAxis("Elevation");
    }

    private void FixedUpdate()
    {
        Vector3 newVelocity = this.transform.TransformDirection(this.moveVector);
        newVelocity.y += this.moveY * moveSpeed;
        this.rigidbody.linearVelocity = newVelocity;
    }

    public void ResetTargetRotation()
    {
        this.TargetRotation = Quaternion.LookRotation(this.transform.forward, Vector3.up);
    }
}
