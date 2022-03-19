using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PurpleSpecialAbility : SpecialAbility
{
    GamePiece pieceToMatch;
    GamePiece targetPiece;
    RaycastHit2D[] hits;
    Tile tile;
    private void FixedUpdate()
    {
        if (isMovementAllowed)
        {
            x = (int)(Mathf.Round(mousepos.x/tileSize)*tileSize);
            y = (int)(Mathf.Round(mousepos.y/tileSize)*tileSize);
            this.transform.position = new Vector3(Mathf.Clamp(x, 0, (m_board.width - 1)*tileSize), Mathf.Clamp(y, 0, (m_board.height - 1)*tileSize), transform.position.z);

        }
    }


    void OnMouseDown()
    {

            hits = Physics2D.RaycastAll(MousePosition.Instance.MousePos, new Vector3(0, 0, -1), 100.0F);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject.name.Contains("Tile"))
                {
                    tile = hit.collider.gameObject.GetComponent<Tile>();
                }

            }

       
        if (pieceToMatch == null)
        {
            pieceToMatch = m_board.m_allGamePieces[tile.xIndex, tile.yIndex];
        }
        else if (pieceToMatch != null)
        {   targetPiece= m_board.m_allGamePieces[tile.xIndex, tile.yIndex];
            if (pieceToMatch.matchValue != targetPiece.matchValue)
            {


                StartCoroutine(PurpleTransmuteMove());
                isMovementAllowed = false;
            }
        }
    }
    IEnumerator PurpleTransmuteMove()
    {
        List<GamePiece> piecesToTransmute = m_board.FindSameColorPieces(targetPiece.xIndex, targetPiece.yIndex);
        piecesToTransmute = piecesToTransmute.OrderBy(i => Random.value).ToList();
        ActivateAbility();
        EventManager.OnDepleteGauge(this.abilityValue.ToString());
        foreach (GamePiece piece in piecesToTransmute)
        {
            
            
            transform.position = new Vector3(piece.xIndex*tileSize, piece.yIndex*tileSize, 30);
            this.anim.SetTrigger("Activate");
            SoundManager.Instance.PlayClipAtPoint(abilitySound, Vector3.zero);
            float length = anim.runtimeAnimatorController.animationClips[0].length;
            yield return new WaitForSeconds(1.167f/2.0f);

            m_board.ChangeColor(piece, pieceToMatch);
            yield return new WaitForSeconds(1.167f / 2.0f); 

        }
        m_board.FindAndRefill();
        if (timesToUse == 0)
        {
            DisableAbility();
        } else
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


