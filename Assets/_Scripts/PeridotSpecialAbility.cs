using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PeridotSpecialAbility : SpecialAbility
{

    private void FixedUpdate()
    {
        if (isMovementAllowed)
        {
            x = (int)(Mathf.Round(mousepos.x/tileSize)*tileSize);
            y = (int)(Mathf.Round(mousepos.y/tileSize)*tileSize);
            this.transform.position = new Vector3(Mathf.Clamp(x, 0, (m_board.width - 1)*tileSize), Mathf.Clamp(y, 0, (m_board.height - 1)*tileSize), transform.position.z);
        }
    }

    private void OnMouseDown()
    {
        isMovementAllowed = false;
        StartCoroutine(PeridotWipeMove());
    }


    IEnumerator PeridotWipeMove()
    {
        
        float length = anim.runtimeAnimatorController.animationClips[0].length;
        List<GamePiece> piecesToDelete = m_board.FindSameColorPieces(x/(int)tileSize,y/(int)tileSize);
        List<List<GamePiece>> listOfList = m_board.SplitLists(piecesToDelete,"xIndex");
        listOfList = listOfList.OrderBy(i => Random.value).ToList();
        ActivateAbility();

        EventManager.OnDepleteGauge(this.abilityValue.ToString());

        foreach (List<GamePiece> list in listOfList)
        {
            SoundManager.Instance.PlayClipAtPoint(abilitySound, Vector3.zero);
            transform.position = new Vector3(list[0].xIndex*tileSize, 3*tileSize, transform.position.z);
            yield return new WaitForSeconds(length);
            m_board.DeleteSameColorPieces(list);
            
            //yield return new WaitForSeconds(length);
        }
        m_board.CollapseAndRefill();
        if (timesToUse == 0)
        {  //             while (!m_board.m_IsFinishedMoving)
            {
                yield return null;
            }
            //m_board.m_isSwitchingEnabled=true;
            DisableAbility();
        }
        else
        {
            while (!m_board.isFinishedMoving)
            {
                yield return null;
            }
            this.EnableAbility(false);
            isMovementAllowed = true;
        }


    }


}
