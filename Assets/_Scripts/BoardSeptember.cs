using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardSeptember : Board
{
    


    protected override IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (m_isSwitchingEnabled)
        {
            GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];
            if (clickedPiece != null && targetPiece != null)
            {
                //List<GamePiece> sameColoredPieces = FindSameColorPieces(clickedPiece);
                //sameColoredPieces.Remove(clickedPiece);
                GamePiece.MatchValue matchValue = clickedPiece.matchValue;
                Vector2 direction = new Vector2(targetPiece.xIndex - clickedPiece.xIndex, targetPiece.yIndex - clickedPiece.yIndex);
                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime, new Vector3(0, 0, 0));
                targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime, new Vector3(0, 0, 0));
                yield return new WaitForSeconds(swapTime);
                
                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
                if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime, new Vector3((targetPiece.xIndex - clickedPiece.xIndex) * tileSize * 0.25f, (targetPiece.yIndex - clickedPiece.yIndex) * tileSize * 0.25f, 0), 0.3f, 10, 0.75f);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime, new Vector3((clickedPiece.xIndex - targetPiece.xIndex) * tileSize * 0.25f, (clickedPiece.yIndex - targetPiece.yIndex) * tileSize * 0.25f, 0), 0.3f, 10, 0.75f);
                    
                    yield return new WaitForSeconds(0.3f);
                } 
                else
                {
                    yield return new WaitForSeconds(swapTime);
                    if (timer.enabled == false)
                    {
                        ScoreManager.Instance.SetScoreMultiplier(0);

                    }

                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
                    while (!m_isSwitchingEnabled)
                    {
                        yield return null;
                    }
                    List<GamePiece> sameColoredPieces = FindSameColorPieces(matchValue);
                    StartCoroutine(SwitchSameColoredPiecesRoutine(sameColoredPieces, direction));
                    //currentTurn++;
                    //EventManager.OnTurnDone(currentTurn);
                    //

                } 
            }
        }
    }

    IEnumerator SwitchSameColoredPiecesRoutine(List<GamePiece> pieces, Vector2 direction)
    {
        m_isSwitchingEnabled = false;
        yield return null;
        if (direction == new Vector2(0, -1))
        {
            pieces = pieces.OrderBy(n => n.yIndex).ToList();
        }
        if(direction == new Vector2(0, 1))
        {
            pieces = pieces.OrderByDescending(n => n.yIndex).ToList();
        }
        if (direction == new Vector2(1, 0))
        {
            pieces = pieces.OrderByDescending(n => n.xIndex).ToList();
        }
        if (direction == new Vector2(-1, 0))
        {
            pieces = pieces.OrderBy(n => n.xIndex).ToList();
        }

        for (int i=0; i<pieces.Count; i++)
        { if (pieces[i] != null)
            {
                GamePiece clickedPiece = m_allGamePieces[pieces[i].xIndex, pieces[i].yIndex];
                int x = pieces[i].xIndex;
                int y = pieces[i].yIndex;
                GamePiece targetPiece;
                if (IsWithinBounds(pieces[i].xIndex + (int)direction.x, pieces[i].yIndex + (int)direction.y))
                {
                    targetPiece = m_allGamePieces[pieces[i].xIndex + (int)direction.x, pieces[i].yIndex + (int)direction.y];
                }
                else
                {
                    targetPiece = null;
                }

                if (clickedPiece != null && targetPiece != null)
                {
                    while (clickedPiece.m_isMoving)
                    {
                        yield return null;
                    }
                    while (targetPiece.m_isMoving)
                    {
                        yield return null;
                    }
                    clickedPiece = m_allGamePieces[x, y];
                    targetPiece = m_allGamePieces[x + (int)direction.x, y + (int)direction.y];
                    clickedPiece.Move(targetPiece.xIndex, targetPiece.yIndex, swapTime, new Vector3(0, 0, 0));
                    targetPiece.Move(clickedPiece.xIndex, clickedPiece.yIndex, swapTime, new Vector3(0, 0, 0));



                }
                else
                {
                    //yield return new WaitForSeconds(swapTime);
                    Debug.Log("ClickePieced:" + (clickedPiece != null).ToString() + " TargetPiece:" + (targetPiece != null).ToString());
                }
            }

        }
        yield return new WaitForSeconds(swapTime);
        ClearAndRefillBoard(FindAllMatches());
        //m_isSwitchingEnabled = true;
    }


    protected List<GamePiece> FindSameColorPieces(GamePiece.MatchValue matchValue)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (m_allGamePieces[x, y] != null)
                {
                    if (matchValue == m_allGamePieces[x, y].matchValue)
                    {
                        gamePieces.Add(m_allGamePieces[x, y]);
                    }
                }
            }
        }
        return gamePieces;
    }
}

