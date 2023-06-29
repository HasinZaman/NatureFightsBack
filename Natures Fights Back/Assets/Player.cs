using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
        float CameraSpeed = 0.01f;
    void Update()
    {
    Vector3 posOffset = new Vector3(0,0,0);
    if (Input.GetKey(KeyCode.W))
        posOffset.z += CameraSpeed;
    if (Input.GetKey(KeyCode.S))
        posOffset.z -= CameraSpeed;
    if (Input.GetKey(KeyCode.A))
        posOffset.x -= CameraSpeed;
    if (Input.GetKey(KeyCode.D))
        posOffset.x += CameraSpeed;
    transform.position += posOffset;
    }
}
