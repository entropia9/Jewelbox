using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResize : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Camera.main.orthographicSize = (7.5f / Screen.width * Screen.height / 1.0f);
    }

    // Update is called once per frame
}
