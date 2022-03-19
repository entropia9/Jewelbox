using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressGlyphPart : MonoBehaviour
{
    public enum PartType
    {
        top,
        right,
        left
    }

    public PartType partType;
    float animationLength=0.668f;
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        
    }

    public void SubscribeToEvents()
    {
        EventManager.GlyphUse += UseGlyphPart;
        EventManager.sliderFull += OpenGlyphPart;
    }

    void OpenGlyphPart(string saveGlyphType)
    {   if(saveGlyphType == partType.ToString())
        {
            anim.SetTrigger("open");
            AnimateGlyph();
        }
        
        
    }

    void UseGlyphPart(string saveGlyphType)
    {
        if (saveGlyphType == partType.ToString())
        {
            anim.SetBool("used", true);
        }
        
    }

    void AnimateGlyph()
    {

            StartCoroutine("AnimateGlyphRoutine");

            
    }

    IEnumerator AnimateGlyphRoutine()
    {
        yield return new WaitForSeconds(animationLength);
        int n = 3;
        while (n > 0)
        {
            anim.SetTrigger("animate");
            yield return new WaitForSeconds(animationLength);
            n--;
        }
    }

    void LoopGlyph()
    {
        anim.SetTrigger("loop");
    }
}
