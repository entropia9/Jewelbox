using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkredSpecialAbility : SpecialAbility
{

    private void FixedUpdate()
    {
        if (isMovementAllowed)
        {
            x = (int)(Mathf.Round(mousepos.x/tileSize)*tileSize);
            this.transform.position = new Vector3(Mathf.Clamp(x, 0, (m_board.width - 1)*tileSize), (m_board.height*tileSize - 1.0f*tileSize) / 2.0f, transform.position.z);
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
        m_board.DeleteColumn(Mathf.Clamp(x/(int)tileSize, 0, m_board.width - 1));
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
