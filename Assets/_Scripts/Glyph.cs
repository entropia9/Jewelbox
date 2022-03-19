using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Glyph : MonoBehaviour
{
    public Slider slider;
    Animator capsuleAnimator;
    Animator glyphAnimator;
    Animator sliderAnimator;
    Board m_board;
    readonly float updateSpeed = 0.2f;
    public float globalMultiplier=0.1f;
    
    public enum GlyphState
    {
        closed,
        active,
        used, 
        disabled
    }
 
    public GlyphState currentGlyphState;
    public enum SliderState
    {
        isActive,
        isInactive
    }

    public SliderState currentSliderState = SliderState.isInactive;

    public enum ProgressGlyphBinding
    {
        top,
        right,
        left,
        none
    }

    public ProgressGlyphBinding progressGlyphBinding;
    // Start is called before the first frame update
    void Start()
    {
        currentGlyphState = GlyphState.closed;
        capsuleAnimator = this.transform.GetChild(0).GetComponent<Animator>();
        glyphAnimator = this.GetComponent<Animator>();
        m_board = Board.Instance;
        sliderAnimator = slider.GetComponentInChildren<Animator>();

        if (slider.minValue == 0)
        {
            SubscribeToEvents();
            currentSliderState=SliderState.isActive;

        }
        EventManager.GemDestroy += SubscribeToEvents;
        

    }
    
    private void SubscribeToEvents()
    {   if(m_board.numberOfGemsDestroyed == slider.minValue)
        {
            currentSliderState = SliderState.isActive;
            EventManager.GemDestroy += SliderUpdate;
            EventManager.sliderFullVoid += OpenCapsule;
            EventManager.sliderFullVoid += SetGlobalMultiplier;
        }
    }

    void SliderUpdate()
    {
        if (slider.value != slider.maxValue)
        {
            StartCoroutine(FillSlider(m_board.numberOfGemsDestroyed));
            sliderAnimator.SetTrigger("flash");
        }

        else if(glyphAnimator!=null && slider.value ==slider.maxValue)
        {
            glyphAnimator.SetTrigger("isActive");
        }

    }

    IEnumerator FillSlider(float numberToChange)
    {
        float preChange = slider.value;
        float elapsed = 0f;
        while (elapsed < updateSpeed)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(preChange, numberToChange, elapsed / updateSpeed);
            
            yield return null;
        }
        
        slider.value = numberToChange;

        if (slider.value == slider.maxValue && m_board.numberOfGemsDestroyed >= slider.maxValue)
        {
            currentSliderState = SliderState.isInactive;
            EventManager.OnSliderFull(progressGlyphBinding.ToString());

            
            if (glyphAnimator != null)
            {
                glyphAnimator.SetTrigger("isActive");
            }

            EventManager.sliderFullVoid -= OpenCapsule;
            EventManager.sliderFullVoid -= SetGlobalMultiplier;


        }
    }

    void OpenCapsule()
    {   
//        openedOnce = true;
        if (capsuleAnimator != null && slider.maxValue <= m_board.numberOfGemsDestroyed)
        {
            
            capsuleAnimator.SetTrigger("open/close");
            currentGlyphState = GlyphState.active;
        }
        
    }

    void SetGlobalMultiplier()
    {
        
            
            ScoreManager.Instance.globalMultiplier += globalMultiplier;


    }

    public void UseGlyph()
    {   
        if (glyphAnimator != null)
        {
            EventManager.GemDestroy -= SliderUpdate;
            glyphAnimator.SetTrigger("used");
            currentGlyphState=GlyphState.used;
        }
    }
    private void OnDisable()
    {
        EventManager.GemDestroy -= SliderUpdate;
        EventManager.sliderFullVoid -= OpenCapsule;
    }
    public void DisableGlyph()
    {
        currentGlyphState = GlyphState.disabled;
        EventManager.sliderFullVoid -= OpenCapsule;
        EventManager.sliderFullVoid -= SetGlobalMultiplier;
    }
    public static implicit operator ParticleSystemForceField(Glyph v)
    {
        throw new NotImplementedException();
    }

    public void RestartGlyph()
    {
        if (currentGlyphState != GlyphState.closed) {
            capsuleAnimator.SetTrigger("open/close");
        }


        Start();
    }
}
