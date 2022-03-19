using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Gauge : MonoBehaviour
    { public Sprite[] sprites; 
    public float framesPerSecond;
    private SpriteRenderer spriteRenderer;
    int m_index = 0;
    public GaugeValue gaugeValue;
    public Lever lever;
    public bool IsFull;
    float delay = 0.2f;
    public AudioClip switchSound;
    bool disabling = false;
    public Sprite lockedSprite;
    public enum GaugeValue
    {
        Amber,
        Aquamarine,
        Darkred,
        Peridot,
        Purple,
        Ruby
    }
    
    public enum GaugeState
    {
        active,
        supercharged,
        disabled
    }
    GaugeState gaugeState;
    protected Orb abilityOrb;
    public Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        lever = FindObjectOfType<Lever>();
        abilityOrb = FindObjectOfType<Orb>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[0];
        anim = this.GetComponent<Animator>();
       anim.enabled = false;
        gaugeState = GaugeState.active;
        EventManager.DepleteGauge += DepleteGauge;
    }


    public void UpdateGauge(int length)
    {
        if (!IsFull&& gaugeState!=GaugeState.disabled)
        {
            StartCoroutine(UpdateGaugeCoroutine(length));
        }
    }

    IEnumerator UpdateGaugeCoroutine(int length)
    {
        
        //Debug.Log(length);
        if (length == 3)
        {
            yield return null;
            m_index++;
            ChangeSprite(m_index);
        }
        if (length == 4)
        {
            yield return null;
            m_index++;
            ChangeSprite(m_index);
            yield return new WaitForSeconds(delay);
            m_index++;
            ChangeSprite(m_index);
        }
        if (length == 5)
        {for (int i = 0; i < length; i++)
            {
                //yield return null;
                yield return new WaitForSeconds(delay);
                m_index++;
                ChangeSprite(m_index);
            }

        }
        if (length == 6)
        {
            for (int i = 0; i < 8; i++)
            {
                //yield return null;
                yield return new WaitForSeconds(delay);
                m_index++;
                ChangeSprite(m_index);
            }

        }
        if (length == 7)
        {
            for (int i = 0; i < 12; i++)
            {
                //yield return null;
                yield return new WaitForSeconds(delay);
                m_index++;
                ChangeSprite(m_index);
            }

        }
        if (length > 7)
        {
            gaugeState = GaugeState.supercharged;
            EventManager.OnAbilitySupercharged(gaugeValue.ToString());
            
            
        }


        if (m_index == 24)
        {
            if (SpecialAbilitiesManager.Instance.currentSelected!=null && SpecialAbilitiesManager.Instance.currentSelected.abilityValue.ToString() != this.gaugeValue.ToString())
            {
                string disappearType = SpecialAbilitiesManager.Instance.currentSelected.abilityValue.ToString();
                string appearType = this.gaugeValue.ToString();
 //               abilityOrb.Transition(disappearType, appearType);
                SpecialAbilitiesManager.Instance.SelectAbility(appearType);
                PlaySelected();
                IsFull = true;
                if (lever.FindFullGauges().Count > 1)
                {
                    lever.DragLeverUp();
                }
                
            }
            else if (SpecialAbilitiesManager.Instance.currentSelected == null)
            {
                SpecialAbilitiesManager.Instance.SelectAbility(gaugeValue.ToString());
                PlaySelected();
                IsFull = true;
                if (lever.FindFullGauges().Count > 1)
                {
                    lever.DragLeverUp();
                }
 //               abilityOrb.PlayAppear(this.gaugeValue.ToString());
            }


        }

    }

    private void ChangeSprite(int index)
    {
        if (index < sprites.Length)
        {
            spriteRenderer.sprite = sprites[index];
        }
        else {
            index = 24;
            m_index = 24;
            spriteRenderer.sprite = sprites[index];
            if(SpecialAbilitiesManager.Instance.currentSelected != null && SpecialAbilitiesManager.Instance.currentSelected.abilityValue.ToString() != this.gaugeValue.ToString()&&!IsFull)
            {
                string disappearType = SpecialAbilitiesManager.Instance.currentSelected.abilityValue.ToString();
                string appearType = this.gaugeValue.ToString();
   //             abilityOrb.Transition(disappearType, appearType);
                SpecialAbilitiesManager.Instance.SelectAbility(appearType);
                PlaySelected();
                IsFull = true;
                if (lever.FindFullGauges().Count > 1)
                {
                    lever.DragLeverUp();
                }
            }
            else if(SpecialAbilitiesManager.Instance.currentSelected == null)
            {
                SpecialAbilitiesManager.Instance.SelectAbility(gaugeValue.ToString());
                PlaySelected();
                IsFull = true;
                if (lever.FindFullGauges().Count > 1)
                {
                    lever.DragLeverUp();
                }
//                abilityOrb.PlayAppear(this.gaugeValue.ToString());
            }
            
    

        }
    }

    public void PlaySelected()
    {   
        this.anim.enabled = true;
        
        this.anim.SetBool("IsSelected", true);
        
        
    }
    public void StopPlaying(int index)
    {
        this.anim.SetBool("IsSelected", false);
        spriteRenderer.sprite = sprites[index];
        this.anim.enabled=false;
    }

    public void DepleteGauge(string gaugeValue)
    {
        if (gaugeValue == this.gaugeValue.ToString())
        {
            m_index = 0;
            StartCoroutine(DepleteRoutine());
            
            IsFull = false;

            
        }
        
        SpecialAbilitiesManager.Instance.DisableAbilityFromList();
        abilityOrb.anim.SetBool("loop", false);
        


    }

    IEnumerator DepleteRoutine()
    {
        IsFull = false;
        this.anim.SetBool("deplete", true);
        abilityOrb.PlayDisappear(gaugeValue.ToString());
        lever.selectedGauge = null;
        yield return new WaitForSeconds(2f);
        this.anim.SetBool("deplete", false);
        
        StopPlaying(0);
        List<Gauge> fullGauges = lever.FindFullGauges();
        if (fullGauges.Count > 0 && !disabling)
        {
            SpecialAbilitiesManager.Instance.SelectAbility(fullGauges[0].gaugeValue.ToString());
        }
        else if(SpecialAbilitiesManager.Instance.currentSelected!=null && SpecialAbilitiesManager.Instance.currentSelected.abilityValue.ToString()==gaugeValue.ToString())
        {
            abilityOrb.PlayDisappear(this.gaugeValue.ToString());
        }
        if(disabling)
        {
            anim.enabled = true;
            this.anim.SetTrigger("lock");
            disabling = false;
        }
    }
    public void DisableGauge()
    {
        disabling = true;
        DepleteGauge(gaugeValue.ToString());
        gaugeState = GaugeState.disabled;
        
        EventManager.DepleteGauge -= DepleteGauge;
       //disabling = false;
    }
    private void OnDisable()
    {
        EventManager.DepleteGauge -= DepleteGauge;
    }
}
