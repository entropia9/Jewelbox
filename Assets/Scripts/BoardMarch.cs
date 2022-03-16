using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BoardMarch : Board
{
    public new TileMarch[,] m_allTiles;
    int upperLimit = 3;
    int maxFloodLevelPerTile = 5;
    int currentFloodLevel = 0;
    [SerializeField] GameObject water;
    [SerializeField] GameObject waterLevel;
    [SerializeField] Slider waterSlider;
    [SerializeField] Splash splash;
    public bool collapsing = false;

    void Start()
    {
        m_allTiles = new TileMarch[width, height];
        m_allGamePieces = new GamePiece[width, height];
        m_nextGamePieces = new List<GamePiece>();
        m_boardDeadlock = GetComponent<BoardDeadlock>();
        numberOfGemsDestroyed = 0;
        //SetupBoard();
        water.transform.position = new Vector3(water.transform.position.x, -tileSize / 2, water.transform.position.z);
        EventManager.Turn += FloodRowEveryTurn;
        EventManager.GlyphUseVoid += GlyphClearAndRefill;

    }

    protected override void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(i * tileSize, j * tileSize, 0), Quaternion.identity) as GameObject;
                tile.name = "Tile(" + i + "," + j + ")";
                m_allTiles[i, j] = tile.GetComponent<TileMarch>();
                tile.transform.parent = transform;
                m_allTiles[i, j].Init(i, j, this);


            }
        }

    }
    public override void ClearPieceAt(int x, int y)
    {

        if (IsWithinBounds(x, y))
        {

            GamePiece pieceToClear = m_allGamePieces[x, y];
            if (pieceToClear != null)
            {
                m_allGamePieces[x, y] = null;
                StartCoroutine(ExplodeGem(pieceToClear));

            }
            if(m_allTiles[x, y].tileType != TileType.Detached&& m_allTiles[x, y].floodLevel>0)
            {
               ReduceFlood();
            }
        }




    }

    private void ReduceFlood()
    {
        currentFloodLevel--;
        int index = (int)Math.Floor((float)currentFloodLevel / maxFloodLevelPerTile);
        int floodLevel = currentFloodLevel % maxFloodLevelPerTile;

        waterSlider.value--;
        List<TileMarch> tiles = GetAllTilesInARow(index);
        foreach (TileMarch tile in tiles)
        {
            tile.SetFloodLevel(floodLevel);
        }

    }

    private void ChangeWaterLevel()
    {
        
        
    }

    public override List<GamePiece> CollapseColumn(int column, float collapseTime = 0.15f)
    {
        collapsing = true;
        //bool isFlooded = false;
        List<GamePiece> movingPieces = new List<GamePiece>();
        for (int i = 0; i < height - 1; i++)
        {
            if (m_allGamePieces[column, i] == null && (m_allTiles[column, i].tileType != TileType.Detached))
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (m_allGamePieces[column, j] && (m_allTiles[column, j].tileType != TileType.Detached))
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
                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, i].SetCoord(column, i);

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
        //splash.MoveToAndPlayAnimation("splash", column * tileSize);
        collapsing = false;
        return movingPieces;
        
    }

    protected override IEnumerator RefillRoutine()
    {
        int falseYOffset = 700;
        float moveTime = 0.2f;
        yield return null;
        if (!IsGoingLeft && !IsFillingOneByOne)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (m_allGamePieces[i, j] == null && (m_allTiles[i, j].tileType != TileType.Detached))
                    {
                        RefillPieceAt(falseYOffset, moveTime, i, j);
                        yield return new WaitForSeconds(moveTime);
                    }

                }
            }
        }
        if (IsGoingLeft && !IsFillingOneByOne)
        {
            for (int i = width - 1; i >= 0; i--)
            {
                for (int j = 0; j < height; j++)
                {
                    if (m_allGamePieces[i, j] == null && (m_allTiles[i, j].tileType != TileType.Detached))
                    {

                        RefillPieceAt(falseYOffset, moveTime, i, j);
                        yield return new WaitForSeconds(moveTime / 2);



                    }

                }
            }

        }

        if (IsFillingOneByOne && !IsGoingLeft)
        {
            List<int> nullsInColumn = new List<int>();

            for (int x = 0; x < width; x++)
            {
                nullsInColumn = CheckForNullsInColumn(x);
                if (nullsInColumn.Count != 0)
                {
                    if (m_allGamePieces[x, nullsInColumn[0]] == null && (m_allTiles[x, nullsInColumn[0]].tileType != TileType.Detached))
                    {
                        RefillPieceAt(falseYOffset, moveTime, x, nullsInColumn[0]);
                        yield return new WaitForSeconds(moveTime / 2);
                    }
                }
            }


        }
        if (IsFillingOneByOne && IsGoingLeft)
        {
            List<int> nullsInColumn = new List<int>();

            for (int x = width - 1; x >= 0; x--)
            {
                nullsInColumn = CheckForNullsInColumn(x);
                if (nullsInColumn.Count != 0)
                {
                    if (m_allGamePieces[x, nullsInColumn[0]] == null && (m_allTiles[x, nullsInColumn[0]].tileType != TileType.Detached))
                    {
                        RefillPieceAt(falseYOffset, moveTime, x, nullsInColumn[0]);
                        yield return new WaitForSeconds(moveTime / 2);
                    }

                }
            }


        }

        yield return null;
    }


    void FloodRowEveryTurn(int currentTurn)
    {   int indexToFlood;
        
        indexToFlood = (int)Math.Floor((float)currentFloodLevel / maxFloodLevelPerTile);
/*        if (m_allTiles[0, indexToFlood + 1].floodLevel != 0 || m_allTiles[0, indexToFlood + 1].spriteRenderer.color != Color.white)
        {
            List<TileMarch> upperTiles = GetAllTilesInARow(indexToFlood + 1);
            foreach (TileMarch tile in upperTiles)
            {
                tile.SetFloodLevel(0);
            }
        } */
        FloodRow(indexToFlood);
        currentFloodLevel++;
        waterSlider.value++;
    }


    List<TileMarch> GetAllTilesInARow(int row)
    {
        List<TileMarch> tiles=new List<TileMarch>();
        for(int i=0; i < width; i++)
        {
            tiles.Add(m_allTiles[i, row]);
        }

        return tiles;
    }
    
    public void FloodRow(int row)
    {
        List<TileMarch> tiles = new List<TileMarch>();
        tiles = GetAllTilesInARow(row);
        foreach (TileMarch tile in tiles)
        {
            tile.FloodTile();
        }
        if (CheckIfRowsBelowAreFlooded(row))
        {
            DetachRow(row - upperLimit);

            }

       
    }

    private bool CheckIfRowsBelowAreFlooded(int row)
    {
        if (row - upperLimit >= 0)
        {
            List<TileMarch> tiles = new List<TileMarch>();
            for (int i=row; i>=row-upperLimit; i--)
            {
                tiles.Union(GetAllTilesInARow(i));
            }
            if (tiles.All(x => x.tileType == TileType.Flooded))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
        
    }

    public void DetachRow(int row)
    {
        List<TileMarch> tiles = new List<TileMarch>();
        tiles = GetAllTilesInARow(row);
        List<GamePiece> pieces = new List<GamePiece>();

        foreach (TileMarch tile in tiles)
        {
            tile.tileType = TileType.Detached;
            pieces.Add(m_allGamePieces[tile.xIndex, tile.yIndex]);
            isScoringAllowed = false;
            ClearPieceAt(tile.xIndex, tile.yIndex);
            tile.spriteRenderer.enabled = false;
            
        }
        StartCoroutine(WaitForAnimationToEnd(pieces));
    }


    IEnumerator WaitForAnimationToEnd(List<GamePiece> gamePieces)
    {
        while (IsAnimationPlaying(gamePieces, "explode"))
        {
            yield return null;
        }
        m_isSwitchingEnabled = true;
        isScoringAllowed = true;
    }

}
