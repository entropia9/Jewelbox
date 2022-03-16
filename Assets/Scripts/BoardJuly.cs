using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public class BoardJuly : Board
{   /*Heatwave. Similar to freezing. There are gems that are overheating and deed to be left alone. 
     * Any movement, including falling through make them hotter and can burn them out, leaving a permanent burnt gem that can't be interacted with directly.
       When burnt -> heat transfer to nearby
       HeatIncrease only if collapsing
        when matched with -> heat transfer;
                Heat heat = GetComponentInChildren<Heat>();
        if (heat != null)
        {
            heat.IncreaseHeatLevel();
        }
     */

    public int overheatingGemsSpawnRate = 5;
    List<GamePiece> warmGems;

    void Start()
    {
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
        m_nextGamePieces = new List<GamePiece>();
        m_boardDeadlock = GetComponent<BoardDeadlock>();
        numberOfGemsDestroyed = 0;
        //SetupBoard();


        StartCoroutine(LateStart(1f));
        //EventManager.GlyphUseVoid += GlyphClearAndRefill;
        //SetupTileFebruaries();
    }


    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        OverheatRandomGems();

        EventManager.Turn += OverheatGemsEveryXTurns;
        //Your Function You Want to Call
    }

    protected override IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (m_isSwitchingEnabled)
        {
            GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];
            if (clickedPiece != null && targetPiece != null)
            {
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
                    clickedPiece.GetComponentInChildren<Heat>().IncreaseHeatLevel();
                    targetPiece.GetComponentInChildren<Heat>().IncreaseHeatLevel();
                    yield return new WaitForSeconds(swapTime);
                    if (timer.enabled == false)
                    {
                        m_scoreMultiplier = 0;
                    }
                    currentTurn++;
                    EventManager.OnTurnDone(currentTurn);
                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());

                }
            }
        }
    }

    private void OverheatGemsEveryXTurns(int boardTurn)
    {
        if (currentTurn % overheatingGemsSpawnRate == 0)
        {
            OverheatRandomGems();
        }

    }
    void OverheatRandomGems()
    {
        GamePiece newWarmGem;
        warmGems = new List<GamePiece>();
        for (int i = 0; i < 2; i++)
        {
            newWarmGem = m_allGamePieces[Random.Range(1, width - 1), Random.Range(1, height - 1)];
            if (newWarmGem != null && !warmGems.Contains(newWarmGem))
            {
                warmGems.Add(newWarmGem);
                newWarmGem.GetComponentInChildren<Heat>().StartOverheating();
            }
            else
            {
                i--;
            }

        }
    }


    public override IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> heatedPieces = gamePieces.Where(x => x.GetComponentInChildren<Heat>().heatLevel >= 1).ToList();
        List<GamePiece> piecesToHeat = new List<GamePiece>();
        m_IsFinishedMoving = false;
        float delay = 0.25f;
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> nullPieces = new List<int>();
        List<GamePiece> matches = new List<GamePiece>();
        bool isFinished = false;
        float timeToWait = 0f;
        while (!isFinished)
        {
            if (heatedPieces.Count != 0)
            {
                foreach (GamePiece piece in heatedPieces)
                {
                     piecesToHeat=piecesToHeat.Union(FindNeighbours(piece.xIndex, piece.yIndex)).ToList();
                }
                piecesToHeat = piecesToHeat.Except(gamePieces).ToList();
            }
            nullPieces = new List<int>();
            movingPieces = new List<GamePiece>();
            List<GamePiece> lastMovingPieces = new List<GamePiece>();
            if (gamePieces != null)
            {
                SplitListsAndUpdate(gamePieces);
            }


            for (int i = 0; i < gamePieces.Count; i++)
            {
                if (gamePieces[i] != null && m_allTiles[gamePieces[i].xIndex, gamePieces[i].yIndex].tileType!=TileType.Burnt)
                {
                    nullPieces.Add(gamePieces[i].xIndex);
                    ClearPieceAt(gamePieces[i].xIndex, gamePieces[i].yIndex);
                    if (timeToWait < GetAnimationClipLength(gamePieces[i].anim, 0) - 0.3f)
                    {
                        timeToWait = GetAnimationClipLength(gamePieces[i].anim, 0) - 0.3f;
                    }
                    yield return new WaitForSeconds(delay / 2);

                }

            }


            yield return new WaitForSeconds(timeToWait / 3);




            for (int j = 0; j < nullPieces.Count; j++)
            {
                movingPieces.Union(CollapseColumn(nullPieces[j]).ToList());
                yield return new WaitForSeconds(delay / 3);
            }






            //m_collapsedOnce = true;

            while (!IsCollapsed(movingPieces))
            {
                yield return null;

            }
            foreach (GamePiece pieceToHeat in piecesToHeat)
            {
                pieceToHeat.GetComponentInChildren<Heat>().StartOverheating();
            }

            //           } while (!IsCollapsed(movingPieces));

            matches = FindMatchesAt(movingPieces);


            /*          while (IsAnimationPlaying(AllGamePieces(), "explode"))
                      {
                          yield return null;
                      } */

            if (matches.Count == 0)
            {

                isFinished = true;
                m_IsFinishedMoving = true;
                timeToWait = 0f;
                break;
            }
            else
            {

                //m_scoreMultiplier++;
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }

        //  m_IsFinishedMoving = true;

    }
    List<GamePiece> FindNeighbours(int xIndex, int yIndex)
    {
        List<GamePiece> neighbours = new List<GamePiece>();
        if (IsWithinBounds(xIndex - 1, yIndex))
        {
            neighbours.Add(m_allGamePieces[xIndex - 1, yIndex]);
        }
        if (IsWithinBounds(xIndex + 1, yIndex))
        {
            neighbours.Add(m_allGamePieces[xIndex + 1, yIndex]);

        }
        if (IsWithinBounds(xIndex, yIndex - 1))
        {
            neighbours.Add(m_allGamePieces[xIndex, yIndex - 1]);

        }
        if (IsWithinBounds(xIndex, yIndex + 1))
        {
            neighbours.Add(m_allGamePieces[xIndex, yIndex + 1]);
        }

        return neighbours;
    }
    public override List<GamePiece> CollapseColumn(int column, float collapseTime = 0.15f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        for (int i = 0; i < height - 1; i++)
        {
            if (m_allGamePieces[column, i] == null && (m_allTiles[column, i].tileType != TileType.Burnt))
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (m_allGamePieces[column, j] && (m_allTiles[column, j].tileType != TileType.Burnt))
                    {
                        Vector3 strength;
                        if (IsWithinBounds(column, j + 1) && m_allGamePieces[column, j + 1] != null && m_allGamePieces[column, j + 1].moveFactor > m_allGamePieces[column, j].moveFactor)
                        {
                            strength = new Vector3(0, m_allGamePieces[column, j + 1].moveFactor * tileSize, 0);
                        }
                        else
                        {
                            strength = new Vector3(0, m_allGamePieces[column, j].moveFactor * tileSize, 0);
                        }
                        m_allGamePieces[column, j].Move(column, i, collapseTime * (j - i), strength, 0.3f * (1 - m_allGamePieces[column, j].moveFactor), 5);
                        Heat heat = m_allGamePieces[column, j].GetComponentInChildren<Heat>();
                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, i].SetCoord(column, i);
                        if (heat.heatLevel > 0)
                        {
                            heat.IncreaseHeatLevel();
                        }
                        if (!movingPieces.Contains(m_allGamePieces[column, i]))
                        {
                            movingPieces.Add(m_allGamePieces[column, i]);
                        }

                        m_allGamePieces[column, j] = null;
                        break;

                    }
                }
            }
        }
        return movingPieces;

    }
}
