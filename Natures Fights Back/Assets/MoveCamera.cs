using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
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
        posOffset.y += 0.01f;
    if (Input.GetKey(KeyCode.S))
        posOffset.y -= 0.01f;
    if (Input.GetKey(KeyCode.A))
        posOffset.x -= 0.01f;
    if (Input.GetKey(KeyCode.D))
        posOffset.x += 0.01f;
    transform.position += posOffset;
   }
}
