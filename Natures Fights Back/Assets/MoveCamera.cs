using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{

               public GameObject prefab;
               public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
                //for (int x = 0; x < 10; x++)
            //for (int z = 0; z < 10; z++){
            //if(x==0||y==0||x==9||y==9)
                    //Instantiate(prefab, new Vector3(x,-2,z), Quaternion.identity);
        //}
    }

    // Update is called once per frame
    void Update()
    {
transform.position = player.transform.position + new Vector3(0, 10, 0);
   }
}
