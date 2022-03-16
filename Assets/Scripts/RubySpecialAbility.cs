using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubySpecialAbility : SpecialAbility
{

    private void FixedUpdate()
    {   
        if (isMovementAllowed)
        {   x = (int)(Mathf.Round(mousepos.x/tileSize)*tileSize);
            y = (int)(Mathf.Round(mousepos.y/tileSize)*tileSize);
            this.transform.position = new Vector3(Mathf.Clamp(x, tileSize, (m_board.width - 2)*tileSize), Mathf.Clamp(y, tileSize, (m_board.height - 2)*tileSize), transform.position.z);
        }

    }

    void OnMouseDown()
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
        m_board.DeleteAdjacentPieces(Mathf.Clamp(x/(int)tileSize, 1, m_board.width - 2), Mathf.Clamp(y/(int)tileSize, 1, m_board.height - 2));
        if (timesToUse <= 0)
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

