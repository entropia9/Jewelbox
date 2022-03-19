using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VictoryLight : MonoBehaviour
{
    public bool isLit=false;
    SpriteRenderer lightSprite;
    [SerializeField] protected Slider lastslider;
    void Start()
    {
        lightSprite = this.GetComponent<SpriteRenderer>();
        TurnLightOff();
        
    }



    public void TurnLightOn()
    {
        
        if (lastslider.value==lastslider.maxValue)
        {
            lightSprite.enabled = true;
            isLit = true;
        }

    }
    public void TurnLightOff()
    {

        lightSprite.enabled = false;
        isLit = false;
    }
}
