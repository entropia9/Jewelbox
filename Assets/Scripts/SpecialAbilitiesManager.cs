using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAbilitiesManager : Singleton<SpecialAbilitiesManager>
{
    SortedList<string, GameObject> specialAbilities;
    SortedList<string, bool> superChargedValues;
    public SpecialAbility currentEnabled;
    public SpecialAbility currentSelected;
    [SerializeField] List<GameObject> abilitiesPrefabs;
    public bool isAbilityEnabled;
    Orb abilityOrb;
    Lever lever;

    private void Start()
    {

        specialAbilities = new SortedList<string, GameObject>();
        superChargedValues = new SortedList<string, bool>();
        foreach (GameObject ability in abilitiesPrefabs)
        {
            if (ability != null)
            {
                specialAbilities.Add(ability.GetComponent<SpecialAbility>().abilityValue.ToString(), ability);
                
            }
            superChargedValues.Add(ability.GetComponent<SpecialAbility>().abilityValue.ToString(), false);
            
        }
        lever = FindObjectOfType<Lever>();
        abilityOrb = FindObjectOfType<Orb>();
        EventManager.AbilitySupercharge += PassSuperChargedValue;
    }
    public void SelectAbility(string abilityType)
    {
        if (currentSelected != null)
        {
            string disappearType = currentSelected.abilityValue.ToString();
            abilityOrb.Transition(disappearType, abilityType);
        }
        else
        {
            abilityOrb.PlayAppear(abilityType);
        }
        currentSelected = specialAbilities[abilityType].GetComponent<SpecialAbility>(); 
        if (currentEnabled!=null)
        {
            EnableAbilityFromList(currentSelected.abilityValue.ToString());
        }
    }
    public void EnableAbilityFromList(string abilityType)
    {
        
        if (currentEnabled == null)
        {
            GameObject specialAbility = Instantiate(specialAbilities[abilityType], new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            abilityOrb.anim.SetBool("loop", true);
            currentEnabled = specialAbility.GetComponent<SpecialAbility>();
            

        } else
        {
            currentEnabled.DisableAbility();
            abilityOrb.materialPropertyBlock.GlowDown();
            GameObject specialAbility = Instantiate(specialAbilities[abilityType], new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            abilityOrb.anim.SetBool("loop", true);
            currentEnabled = specialAbility.GetComponent<SpecialAbility>();
            currentSelected = currentEnabled;
        }
        
        currentEnabled.GetComponent<SpecialAbility>().EnableAbility(superChargedValues[abilityType]);
        isAbilityEnabled = true;
    }

    void PassSuperChargedValue(string abilityValue)
    {
        superChargedValues[abilityValue] = true;
        Debug.Log("Ability" + abilityValue + "is supercharged");
    }
    public void DisableAbilityFromList()
    {
        isAbilityEnabled = false;
        abilityOrb.materialPropertyBlock.GlowDown();
        if (currentEnabled == null)
        {
            return;
        }
        else
        {

            //           currentEnabled.DisableAbility();

            currentSelected = null;
            currentEnabled = null;
        }
        
    }
}
