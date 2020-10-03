using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public GameObject camera;

    public float lookSensitivty, moveSpeed;

    public CharacterController playerController;

    float xRotation = 0.0F;

    Vector3 moveVector, moveAmount,smoothVelocity;

    // Start is called before the first frame update
    void Start()
    {
        Rigidbody rb = this.GetComponent<Rigidbody>();
        rb.useGravity = false;
        //rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        //rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        //playerController.enabled = true;
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivty * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivty * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90F, 90F);

        camera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        
        this.transform.Rotate(new Vector3(0, mouseX, 0));

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        moveVector = new Vector3(moveX, 0, moveZ);
        moveVector = moveVector.normalized;
        moveVector *= moveSpeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, moveVector, ref smoothVelocity, 0.15F);
        
    }

    private void FixedUpdate() 
    {
        this.GetComponent<Rigidbody>().MovePosition(transform.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

}
