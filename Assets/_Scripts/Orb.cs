using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    public Animator anim;
    Lever lever;
    int counter;
    AnimatorClipInfo[] clipInfo;
    public GlowMaterialPropertyBlock materialPropertyBlock;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        lever = FindObjectOfType<Lever>();
        materialPropertyBlock = GetComponent<GlowMaterialPropertyBlock>();
    }



    
    public void PlayAppear(string type)
    {
        counter = 0;
        anim.SetInteger("Counter", counter);
        anim.Play(type + "Appear");
        


    }
    
    public void IncrementCounter()
    {   counter++;
        anim.SetInteger("Counter", counter);
    }
    public void PlayDisappear(string type)
    {
        anim.Play(type+"Disappear");
    }
    public void Transition(string disappearType, string appearType)
    {
        StartCoroutine(TransitionRoutine(disappearType, appearType));
    }
    IEnumerator TransitionRoutine(string disappearType, string appearType)
    {
        PlayDisappear(disappearType);
        clipInfo = anim.GetCurrentAnimatorClipInfo(0);
        float length = clipInfo[0].clip.length;
        yield return new WaitForSeconds(length);
        PlayAppear(appearType);
    }
    private void OnMouseDown()
    {
        if (!SpecialAbilitiesManager.Instance.isAbilityEnabled)
        {
            List<Gauge> fullGauges = lever.FindFullGauges();
            if (fullGauges.Count > 0)
            {
                Debug.Log(SpecialAbilitiesManager.Instance.currentSelected);
                anim.Play(SpecialAbilitiesManager.Instance.currentSelected.abilityValue.ToString() + "Loop");
                SpecialAbilitiesManager.Instance.EnableAbilityFromList(SpecialAbilitiesManager.Instance.currentSelected.abilityValue.ToString());
                materialPropertyBlock.GlowUp();

            }
        }
        else
        {
            anim.SetBool("loop", false);
            anim.Play(SpecialAbilitiesManager.Instance.currentEnabled.abilityValue.ToString() + "Ready");
            SpecialAbilitiesManager.Instance.currentEnabled.DisableAbility();
            materialPropertyBlock.GlowDown();

        }
    }
}
