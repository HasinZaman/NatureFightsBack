using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    float CameraSpeed = 0.01f;
               public GameObject prefab;
    // Start is called before the first frame update
    void Start()
    {
        
                for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++){
            if(x==0||y==0||x==9||y==9)
                    Instantiate(prefab, new Vector2(x,y), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 posOffset = new Vector3(0,0,0);
    if (Input.GetKey(KeyCode.W))
        posOffset.y += CameraSpeed;
    if (Input.GetKey(KeyCode.S))
        posOffset.y -= CameraSpeed;
    if (Input.GetKey(KeyCode.A))
        posOffset.x -= CameraSpeed;
    if (Input.GetKey(KeyCode.D))
        posOffset.x += CameraSpeed;
    transform.position += posOffset;
   }
}
