using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class SpecialAbility : MonoBehaviour
{
    public bool abilityEnabled = false;
    protected Vector3 mousepos;
    [SerializeField] protected float destructionDelay=0.4f;
    [SerializeField] protected Gauge abilityGauge;
    public bool isMovementAllowed=true;
    protected int x;
    protected int y;
    protected bool isSuperCharged=false;
    protected Animator anim;
    protected SpecialAbilitiesManager specialAbilitiesManager;
    protected int timesToUse=1;
    protected float tileSize= 74.0f;
    public enum AbilityValue
    {
        Amber,
        Aquamarine,
        Darkred,
        Peridot,
        Purple,
        Ruby
    }

    public AbilityValue abilityValue;
    public AudioClip abilitySound;
    public Board m_board;
    // Start is called before the first frame update
    protected void Start()
    {
        m_board=Board.Instance;
        specialAbilitiesManager=SpecialAbilitiesManager.Instance;
        specialAbilitiesManager.isAbilityEnabled = true;

        if (this.GetComponent<Animator>() != null)
        {
            anim = this.GetComponent<Animator>();
        }
        isSuperCharged = false;
    }

    

    
    protected void Update()
    {
        mousepos = GetMousePosition();
    }
    public void EnableAbility(bool supercharged)
    {
        
        this.gameObject.SetActive(true);
        this.GetComponent<SpriteRenderer>().enabled = true;
        if (this.GetComponent<Animator>() != null)
        {
            
            this.GetComponent<Animator>().enabled = true;
            anim = this.GetComponent<Animator>();
            anim.SetBool("Activated", false);
        }
        abilityEnabled = true;
       

        isSuperCharged = supercharged;
        if (isSuperCharged)
        {
            timesToUse = 3;
        }
        else
        {
            timesToUse = 1;
        }
    }
    public void DisableAbility()
    {
        abilityEnabled = false;
        
        SpecialAbilitiesManager.Instance.DisableAbilityFromList();
        Destroy(this.gameObject);

    }


    protected Vector3 GetMousePosition()
    {
        if (this.abilityEnabled)
        { this.mousepos = MousePosition.Instance.MousePos; }

        return mousepos; 
    }


    public void ActivateAbility()
    {

        timesToUse -= 1;
        if (anim != null)
        {
            anim.SetBool("Activated", true);
        }
        
    }
}