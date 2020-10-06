using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerController.thisPlayer != null && !PlayerController.thisPlayer.dead)
        {
            Vector3 targetPos = PlayerController.thisPlayer.transform.position;
            targetPos.z = -10;
            transform.position = targetPos;
        }
    }
}
