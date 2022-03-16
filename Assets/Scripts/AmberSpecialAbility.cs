using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmberSpecialAbility : SpecialAbility
{
   
    private void FixedUpdate()
    {
        if (isMovementAllowed)
        {
            y = (int)(Mathf.Round(mousepos.y/tileSize)*tileSize);
            this.transform.position = new Vector3((m_board.width*tileSize - 1.0f*tileSize) / 2.0f, Mathf.Clamp(y, 0, m_board.height*tileSize - 1*tileSize), transform.position.z);
        }
       
    }

    private void OnMouseDown()
    {
        isMovementAllowed = false;
        StartCoroutine(Activate());
    }

    IEnumerator Activate()
    {
        ActivateAbility();
        SoundManager.Instance.PlayClipAtPoint(abilitySound, Vector3.zero);
        EventManager.OnDepleteGauge(this.abilityValue.ToString());
        yield return new WaitForSeconds(destructionDelay);
        m_board.DeleteRow(Mathf.Clamp(y, 0, (int)(m_board.height*tileSize - 1*tileSize))/(int)tileSize);
        
        if (timesToUse == 0)
        {
            DisableAbility();
        }
        else
        {
            while (!m_board.m_IsFinishedMoving)
            {
                yield return null;
            }
            this.EnableAbility(false);
            isMovementAllowed = true;
        }
    }
}
