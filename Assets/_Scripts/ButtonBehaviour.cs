using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBehaviour : MonoBehaviour
{
    Animator anim;
    void Start()
    {   
        anim = GetComponentInChildren<Animator>();
        EventManager.SwitchAllow += SetChangeAllowed;
    }

    public void PlayAnimation()
    {
        anim.SetTrigger("change");
    }

    void SetChangeAllowed(bool isChangeAllowed)
    {
        anim.SetBool("changeAllowed", isChangeAllowed);
    }

    private void OnDisable()
    {
        EventManager.SwitchAllow -= SetChangeAllowed;
    }
}
