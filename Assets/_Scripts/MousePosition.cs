using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePosition : Singleton<MousePosition>
{ public Vector3 MousePos { get; private set; }



    // Update is called once per frame
    void Update()
    {
        MousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

}
