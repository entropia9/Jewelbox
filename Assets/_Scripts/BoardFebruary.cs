using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;



public class BoardFebruary : Board
{
    int frozenTilesSpawnRate = 10;
    List<TileFebruary> frozenTiles;
    int numberOfTilesToFreeze=2;
    Board m_board;
    GamePiece frozenPiece = null;
    public new TileFebruary[,] m_allTiles;
    bool frozen = false;
    public TileFebruary[,] tileFebruaries;
    void Start()
    {
        m_allTiles = new TileFebruary[width, height];
        m_allGamePieces = new GamePiece[width, height];
        m_nextGamePieces = new List<GamePiece>();
        m_boardDeadlock = GetComponent<BoardDeadlock>();
        numberOfGemsDestroyed = 0;
        //SetupBoard();


        StartCoroutine(LateStart(1f));
        EventManager.GlyphUseVoid += GlyphClearAndRefill;
        //SetupTileFebruaries();
    }

    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        FreezeTiles();
        //Your Function You Want to Call
    }
    private void FreezeTilesEveryXTurns(int boardTurn)
    {   
        if (currentTurn % frozenTilesSpawnRate == 0)
        {
            if (frozen == false)
            {
                FreezeTiles();
                frozen = true;
            }
        }
        else
        {
            frozen = false;
        }
    }

    protected override void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(i * tileSize, j * tileSize, 0), Quaternion.identity) as GameObject;
                tile.name = "Tile(" + i + "," + j + ")";
                m_allTiles[i, j] = tile.GetComponent<TileFebruary>();
                tile.transform.parent = transform;
                m_allTiles[i, j].Init(i, j, this);


            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {

                m_allTiles[i, j].FindNeighbours();


            }
        }

        
        EventManager.Turn += FreezeTilesEveryXTurns;
    }

    public void SetupTileFebruaries()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tileFebruaries[i, j] = m_allTiles[i, j].GetComponent<TileFebruary>();
            }
        }
    }

    public void ClickTile(TileFebruary tile)
    {

        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
            m_clickedPiece = m_allGamePieces[m_clickedTile.xIndex, m_clickedTile.yIndex];

            //highlightAllowed = !highlightAllowed;


        }


    }

    public void DragToTile(TileFebruary tile)
    {
        m_currentTargetTile = tile;

        if (tile.tileType != TileType.Frozen && tile.tileType != TileType.Cracked)
        {
            if (m_clickedTile != null && m_clickedTile.tileType != TileType.Frozen && m_clickedTile.tileType != TileType.Cracked && IsNextTo(tile, m_clickedTile))
            {

                m_targetTile = tile;
                if (portalEnabled)
                {

                    draggingDirection = new Vector2(m_clickedTile.xIndex - tile.xIndex, m_clickedTile.yIndex - tile.yIndex);
                    //FindObjectOfType<AquamarineSpecialAbility>().PortalAppear(draggingDirection);
                    //PortalMove(m_clickedTile, m_currentTargetTile);
                }


            }
            else if (m_clickedTile != null && m_clickedTile.tileType != TileType.Frozen && m_clickedTile.tileType != TileType.Cracked && !IsNextTo(tile, m_clickedTile))
            {

                m_clickedTile = tile;
                m_clickedPiece = m_allGamePieces[m_clickedTile.xIndex, m_clickedTile.yIndex];
                m_targetTile = null;
            }
        }


    }
    public override void ReleaseTile()
    {

        m_isDragging = false;
        if (m_clickedTile != null && m_clickedTile.tileType != TileType.Frozen && m_clickedTile.tileType != TileType.Cracked && m_targetTile != null && m_targetTile.tileType != TileType.Frozen && m_targetTile.tileType != TileType.Cracked && !SpecialAbilitiesManager.Instance.isAbilityEnabled)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }
        m_clickedTile = null;
        m_targetTile = null;


    }
    public override void ClearPieceAt(List<GamePiece> gamePieces)
    {
        int scoreValue = ScoreManager.Instance.GetScoreValue(gamePieces.Count);
        List<TileFebruary> Tiles=new List<TileFebruary>();
        foreach (GamePiece piece in gamePieces)
        {
            Tiles.Add(m_allTiles[piece.xIndex, piece.yIndex]);



        }
        foreach (TileFebruary tile in Tiles)
        {
            tile.RemoveNeighbour(Tiles);
        }

        foreach (GamePiece piece in gamePieces)
            {
                if (piece != null)
                {
                    m_allTiles[piece.xIndex, piece.yIndex].ClearCold();
                    m_allTiles[piece.xIndex, piece.yIndex].WarmNeighbours();
                    ClearPieceAt(piece.xIndex, piece.yIndex);

                    if (ScoreManager.Instance.isScoringAllowed)
                    {

                        ScoreManager.Instance.ScorePoints((float)scoreValue);
                    }
                }
            }
        

        /*  
                if (IsWithinBounds(x, y))
                {
                    if (m_allTiles[x, y].tileType != TileType.Normal)
                    {
                        m_allTiles[x, y].ClearCold();
                        GamePiece pieceToClear = m_allGamePieces[x, y];

                        if (pieceToClear != null)
                        {
                            int index = pieceToClear.matchValue.GetHashCode();
                            m_allGamePieces[x, y] = null;
                            StartCoroutine(ExplodeGem(x, y, pieceToClear));
                            if (m_allTiles[x, y].tileType == TileType.VeryCold)
                            {
                                GameObject newGem = GetObject(index);

                                if (newGem != null)
                                {
                                    newGem.GetComponent<GamePiece>().Init(this);
                                    PlaceGem(newGem.GetComponent<GamePiece>(), x, y);
                                    newGem.SetActive(true);
                                    //newGem.transform.parent = gamePieceObjectPoolers[index].transform;
                                    //newGem.transform.parent = transform; }


                                }

                            }
                        }

                    }
                    else
                    {   
                        GamePiece pieceToClear = m_allGamePieces[x, y];
                        if (pieceToClear != null)
                        {
                            m_allGamePieces[x, y] = null;
                            StartCoroutine(ExplodeGem(x, y, pieceToClear));

                        }
                    }
                    m_allTiles[x, y].WarmNeighbours(); 

                } */




    }
    public override IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {

        isFinishedMoving = false;
        float delay = 0.25f;
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> nullPieces = new List<int>();
        List<GamePiece> matches = new List<GamePiece>();
        bool isFinished = false;
        float timeToWait = 0f;
        while (!isFinished)
        {
            nullPieces = new List<int>();
            movingPieces = new List<GamePiece>();
            List<GamePiece> lastMovingPieces = new List<GamePiece>();
            List<TileFebruary> Tiles = new List<TileFebruary>();
            foreach (GamePiece piece in gamePieces)
            {   
                    Tiles.Add(m_allTiles[piece.xIndex, piece.yIndex]);
                    
                

            }
            foreach (TileFebruary tile in Tiles)
            {
                tile.RemoveNeighbour(Tiles);
            }

            if (gamePieces != null)
            {
                SplitListsAndUpdate(gamePieces);
            }


            for (int i = 0; i < gamePieces.Count; i++)
            {
                if (gamePieces[i] != null)
                {
                    nullPieces.Add(gamePieces[i].xIndex);
                    Tiles[i].WarmNeighbours();
                    Tiles[i].ClearCold();
                    int index = gamePieces[i].matchValue.GetHashCode();
                    ClearPieceAt(gamePieces[i].xIndex, gamePieces[i].yIndex);
                    if (Tiles[i].tileType == TileType.VeryCold)
                    {
                        GameObject newGem = GetObject(index);

                        if (newGem != null)
                        {
                            GamePiece newGamePiece = newGem.GetComponent<GamePiece>();
                            newGamePiece.Init(this);
                            newGamePiece.PlaceGem(Tiles[i].xIndex, Tiles[i].yIndex);
                            newGem.SetActive(true);


                        }

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

            //           } while (!IsCollapsed(movingPieces));

            matches = FindMatchesAt(movingPieces);


            /*          while (IsAnimationPlaying(AllGamePieces(), "explode"))
                      {
                          yield return null;
                      } */

            if (matches.Count == 0)
            {

                isFinished = true;
                isFinishedMoving = true;
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
    public override List<GamePiece> CollapseColumn(int column, float collapseTime = 0.15f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        for (int i = 0; i < height - 1; i++)
        {
            if (m_allGamePieces[column, i] == null && (m_allTiles[column, i].tileType != TileType.Frozen && m_allTiles[column, i].tileType != TileType.Cracked))
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (m_allGamePieces[column, j] && (m_allTiles[column, j].tileType != TileType.Frozen && m_allTiles[column, j].tileType != TileType.Cracked))
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
        return movingPieces;

    }

    protected override IEnumerator RefillRoutine()
    {
        int falseYOffset = 700;
        float moveTime = 0.2f;
        yield return null;
        //RefillBoard(700, 0.2f);
        if (!IsGoingLeft && !isFillingOneByOne)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (m_allGamePieces[i, j] == null && (m_allTiles[i, j].tileType != TileType.Frozen && m_allTiles[i, j].tileType != TileType.Cracked))
                    {
                        RefillPieceAt(falseYOffset, moveTime, i, j);
                        yield return new WaitForSeconds(moveTime);
                    }

                }
            }
        }
        if (IsGoingLeft && !isFillingOneByOne)
        {
            for (int i = width - 1; i >= 0; i--)
            {
                for (int j = 0; j < height; j++)
                {
                    if (m_allGamePieces[i, j] == null && (m_allTiles[i, j].tileType != TileType.Frozen && m_allTiles[i, j].tileType != TileType.Cracked))
                    {

                        RefillPieceAt(falseYOffset, moveTime, i, j);
                        yield return new WaitForSeconds(moveTime / 2);



                    }

                }
            }

        }

        if (isFillingOneByOne && !IsGoingLeft)
        {
            List<int> nullsInColumn = new List<int>();

            for (int x = 0; x < width; x++)
            {
                nullsInColumn = CheckForNullsInColumn(x);
                if (nullsInColumn.Count != 0)
                {
                    if (m_allGamePieces[x, nullsInColumn[0]] == null && (m_allTiles[x, nullsInColumn[0]].tileType != TileType.Frozen && m_allTiles[x, nullsInColumn[0]].tileType != TileType.Cracked))
                    {
                        RefillPieceAt(falseYOffset, moveTime, x, nullsInColumn[0]);
                        yield return new WaitForSeconds(moveTime / 2);
                    }
                }
            }


        }
        if (isFillingOneByOne && IsGoingLeft)
        {
            List<int> nullsInColumn = new List<int>();

            for (int x = width - 1; x >= 0; x--)
            {
                nullsInColumn = CheckForNullsInColumn(x);
                if (nullsInColumn.Count != 0)
                {
                    if (m_allGamePieces[x, nullsInColumn[0]] == null && (m_allTiles[x, nullsInColumn[0]].tileType != TileType.Frozen && m_allTiles[x, nullsInColumn[0]].tileType != TileType.Cracked))
                    {
                        RefillPieceAt(falseYOffset, moveTime, x, nullsInColumn[0]);
                        yield return new WaitForSeconds(moveTime / 2);
                    }

                }
            }


        }

        yield return null;
    }

    public void FreezeTiles()
    {   TileFebruary newFrozenTile;
        frozenTiles = new List<TileFebruary>();
        for(int i=0; i < 2; i++)
        {
            newFrozenTile=m_allTiles[Random.Range(1, width-1), Random.Range(1, height-1)];
            if (newFrozenTile!=null && !frozenTiles.Contains(newFrozenTile))
            {
                frozenTiles.Add(newFrozenTile);
                newFrozenTile.Freeze();
            }
            else
            {
                i--;
            }
            
        }
    }

    public override List<GamePiece> FindSameColorPieces(int x, int y)
    {   if(m_allTiles[x,y].tileType!=TileType.Frozen && m_allTiles[x, y].tileType != TileType.Cracked)
        {
            return FindSameColorPieces(m_allGamePieces[x, y]);
        }
        else
        {
            return FindSameColorPieces(m_allTiles[x, y].frozenPiece);
        }
            
    }
    private void OnDisable()
    {
        EventManager.Turn -= FreezeTilesEveryXTurns;
    }
}
