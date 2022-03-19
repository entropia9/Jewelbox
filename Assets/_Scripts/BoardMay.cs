using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardMay : Board
{
    List<TileMay> loop0;
    List<TileMay> loop1;
    List<TileMay> loop2;
    List<TileMay> loop3;
    public new TileMay[,] m_allTiles;
    TileMay m_clickedTileMay;
    public TileMay m_targetTileMay;
    public TileMay previousTargetTile;
    int initialIndex;
    int currentIndex;
    Vector3 mouseRef;
    GamePiece gamePieceToCheck;
    bool isLoopDoneMoving = true;
    int numberOfTilesMoved = 0;
    Vector3 mousepos;
    private Vector3 mouseOffset;
    int x;
    int y;
    // Start is called before the first frame update

    // Update is called once per frame

    void Start()
    {
        m_allTiles = new TileMay[width, height];
        m_allGamePieces = new GamePiece[width, height];
        m_nextGamePieces = new List<GamePiece>();
        m_boardDeadlock = GetComponent<BoardDeadlock>();
        numberOfGemsDestroyed = 0;
        mouseRef = new Vector3(tileSize * width / 2, tileSize * height / 2, 0);
        //SetupBoard();

        EventManager.GlyphUseVoid += GlyphClearAndRefill;

    }

    private void Update()
    {
        if (draggingToTile)
        {
            mouseOffset = (Input.mousePosition - mousepos);
            
            mousepos = Input.mousePosition;
            //x = (int)(Mathf.Round(mousepos.x / tileSize) * tileSize);
            //y = (int)(Mathf.Round(mousepos.y / tileSize) * tileSize);
            //this.transform.position = new Vector3(Mathf.Clamp(x, tileSize, (width - 2) * tileSize), Mathf.Clamp(y, tileSize, (height - 2) * tileSize), transform.position.z);
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
                m_allTiles[i, j] = tile.GetComponent<TileMay>();
                tile.transform.parent = transform;
                m_allTiles[i, j].Init(i, j, this);


            }
        }

        SetUpAllLoops();
    }
    List<TileMay> SetUpLoop(int start, int end, LoopNumber loopNumber)
    {
        List<TileMay> loop = new List<TileMay>();
        List<TileMay> loopA = new List<TileMay>();
        List<TileMay> loopB = new List<TileMay>();
        List<TileMay> loopC = new List<TileMay>();
        List<TileMay> loopD = new List<TileMay>();

        for (int i = start; i < end; i++)
        {
            loopA.Add(m_allTiles[i, start]);
            m_allTiles[i, start].loopNumber = loopNumber;
            m_allTiles[i, start].sprite = m_allTiles[i, start].sprites[start];
        }
        for (int i = start; i < end; i++)
        {
            loopB.Add(m_allTiles[end - 1, i]);
            m_allTiles[end - 1, i].loopNumber = loopNumber;
            m_allTiles[end - 1, i].sprite = m_allTiles[end - 1, i].sprites[start];
        }
        for (int i = end - 1; i >= start; i--)
        {
            loopC.Add(m_allTiles[i, end - 1]);
            m_allTiles[i, end - 1].loopNumber = loopNumber;
            m_allTiles[i, end - 1].sprite = m_allTiles[i, end - 1].sprites[start];

        }

        for (int i = end - 1; i >= start; i--)
        {
            loopD.Add(m_allTiles[start, i]);
            m_allTiles[start, i].loopNumber = loopNumber;
            m_allTiles[start, i].sprite= m_allTiles[start, i].sprites[start];
        }






        loop = loopA.Union(loopB).Union(loopC).Union(loopD).ToList();
        for(int y=0; y<loop.Count; y++)
        {
            loop[y].indexInLoop = y;
        }
        return loop;
    }



    public void ClickTile(TileMay tile)
    {

        if (m_clickedTile == null && isLoopDoneMoving)
        {
            m_clickedTile = tile;
            m_clickedTileMay = tile;
            m_clickedPiece = m_allGamePieces[m_clickedTile.xIndex, m_clickedTile.yIndex];
            gamePieceToCheck = m_clickedPiece;
            //highlightAllowed = !highlightAllowed;
            numberOfTilesMoved = 0;
            mousepos = Input.mousePosition;

        }
        initialIndex = tile.indexInLoop;

    }
    public void DragToTile(TileMay tile)
    {
        m_currentTargetTile = tile;


        if (m_clickedTile != null && DoesTileBelongToTheSameLoop(tile, m_clickedTileMay))
        {
           
            
            
            m_targetTile = tile;
            m_targetTileMay = tile; 
            int amountToMove = m_targetTileMay.indexInLoop - m_clickedTileMay.indexInLoop;
            if (draggingToTile)
            {
                amountToMove = m_targetTileMay.indexInLoop - previousTargetTile.indexInLoop;
                StartCoroutine(MoveGemsOnLoopRoutine(amountToMove, ChooseLoop(m_clickedTileMay)));
                //ChooseLoopAndMoveGems(m_clickedTileMay, amountToMove);
            }
            else
            {
                int alternativeAmountToMove = m_targetTileMay.indexInLoop - m_clickedTileMay.indexInLoop - ChooseLoop(m_clickedTileMay).Count;
                if (Mathf.Abs(alternativeAmountToMove) >= amountToMove)
                {
                    StartCoroutine(MoveGemsOnLoopRoutine(amountToMove, ChooseLoop(m_clickedTileMay)));
                }
                else
                {
                    StartCoroutine(MoveGemsOnLoopRoutine(alternativeAmountToMove, ChooseLoop(m_clickedTileMay)));
                }
                
            }
            Debug.Log(amountToMove.ToString());
            
            
            if (portalEnabled)
            {

                draggingDirection = new Vector2(m_clickedTile.xIndex - tile.xIndex, m_clickedTile.yIndex - tile.yIndex);
                //FindObjectOfType<AquamarineSpecialAbility>().PortalAppear(draggingDirection);
                //PortalMove(m_clickedTile, m_currentTargetTile);
            }


        }
        else if (m_clickedTile != null && !DoesTileBelongToTheSameLoop(tile, m_clickedTileMay))
        {
            /*  clickedOnce = false;
              m_clickedTile = tile;
              m_clickedPiece = m_allGamePieces[m_clickedTile.xIndex, m_clickedTile.yIndex];
              m_targetTile = null; */
  

                StartCoroutine(MoveGemsOnLoopRoutine(GetDirection(mouseRef, mousepos), ChooseLoop(m_clickedTileMay)));


        }



    }
    private int GetDirection(Vector3 manipPos, Vector3 newMousePos)
    {
        //Where in the coordinate system compared to the manipPos
        //is the mouse
        Vector3 coord = newMousePos - manipPos;

        //Gets the direction
        Vector3 dir = mouseOffset;

        if (coord.x >= 0 && coord.y >= 0)
        {
            if (dir.x >= 0 && dir.y <= 0)
                return -1;
            else if (dir.x <= 0 && dir.y >= 0)
                return 1;
        }
        else if (coord.x >= 0 && coord.y <= 0)
        {
            if (dir.x <= 0 && dir.y <= 0)
                return -1;
            else if (dir.x >= 0 && dir.y >= 0)
                return 1;
        }
        else if (coord.x <= 0 && coord.y <= 0)
        {
            if (dir.x <= 0 && dir.y >= 0)
                return -1;
            else if (dir.x >= 0 && dir.y <= 0)
                return 1;
        }
        else
        {
            if (dir.x >= 0 && dir.y >= 0)
                return -1;
            else if (dir.x <= 0 && dir.y <= 0)
                return 1;
        }

        return 0;
    }
    void ChooseLoopAndMoveGems(TileMay tile, int amountToMove)
    {
        Debug.Log(tile.loopNumber.ToString());
        switch (tile.loopNumber)
        {
            case LoopNumber.loop0:
                MoveGemsOnLoop(amountToMove, loop0);
                break;
            case LoopNumber.loop1:
                MoveGemsOnLoop(amountToMove, loop1);
                break;
            case LoopNumber.loop2:
                MoveGemsOnLoop(amountToMove, loop2);
                break;
            case LoopNumber.loop3:
                MoveGemsOnLoop(amountToMove, loop3);

                break;
            default:
                Debug.Log("Invalid choice");
                break;
        }
        
    }

    List<TileMay> ChooseLoop(TileMay tile)
    {
        Debug.Log(tile.loopNumber.ToString());
        List<TileMay> loop = new List<TileMay>();
        switch (tile.loopNumber)
        {
            case LoopNumber.loop0:
                loop = loop0;
                break;
            case LoopNumber.loop1:
                loop = loop1;
                break;
            case LoopNumber.loop2:
                loop = loop2;
                break;
            case LoopNumber.loop3:
                loop = loop3;

                break;
            default:
                loop = new List<TileMay>();
                break;
        }
        return loop;
    }

    void MoveGemsOnLoop(int amountToMove, List<TileMay> loopToUse)
    {
       
        
        if (amountToMove > 0)
        {   
            for(int i=0; i<loopToUse.Count; i++)
            {
                if (i + amountToMove < loopToUse.Count)
                {
                    m_allGamePieces[loopToUse[i].xIndex, loopToUse[i].yIndex].Move(loopToUse[i + amountToMove].xIndex, loopToUse[i + amountToMove].yIndex, swapTime, new Vector3(0, 0, 0));
                } else
                {
                    int index = i + amountToMove - loopToUse.Count;
                    m_allGamePieces[loopToUse[i].xIndex, loopToUse[i].yIndex].Move(loopToUse[index].xIndex, loopToUse[index].yIndex, swapTime, new Vector3(0, 0, 0));
                }
               
            }
            numberOfTilesMoved ++;
        }
       if (amountToMove < 0)
        {
            for (int i = loopToUse.Count-1; i >= 0; i--)
            {
                if (i + amountToMove >= 0)
                {
                    m_allGamePieces[loopToUse[i].xIndex, loopToUse[i].yIndex].Move(loopToUse[i + amountToMove].xIndex, loopToUse[i + amountToMove].yIndex, swapTime, new Vector3(0, 0, 0));
                }
                else
                {
                    int index = loopToUse.Count+(i+amountToMove);
                    m_allGamePieces[loopToUse[i].xIndex, loopToUse[i].yIndex].Move(loopToUse[index].xIndex, loopToUse[index].yIndex, swapTime, new Vector3(0, 0, 0));
                }

            }
            numberOfTilesMoved--;
        }
        
    }
    IEnumerator MoveGemsOnLoopRoutine(int amountToMove, List<TileMay> loopToUse)
    {
        isLoopDoneMoving = false;
        if (amountToMove > 0)
        {
            for (int i=1; i<=amountToMove; i++)
            {
                MoveGemsOnLoop(1, loopToUse);
                yield return new WaitForSeconds(swapTime);
            }
        }
        if (amountToMove < 0)
        {
            for (int i = -1; i >= amountToMove; i--)
            {
                MoveGemsOnLoop(-1, loopToUse);
                yield return new WaitForSeconds(swapTime);
            }
        }
        isLoopDoneMoving = true;

    }
    public override void ReleaseTile()
    {

        m_isDragging = false;

        if (m_clickedTile != null && m_targetTile != null && !SpecialAbilitiesManager.Instance.isAbilityEnabled)
        {
           SwitchTiles(m_clickedTile, m_targetTile);
        }
        m_clickedTile = null;
        m_targetTile = null;


    }

    protected override IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (m_isSwitchingEnabled)
        {
            
            GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];
            if (clickedPiece != null && targetPiece != null)
            {
                while (!isLoopDoneMoving)
                {
                    yield return null;
                }
                currentIndex = m_allTiles[gamePieceToCheck.xIndex, gamePieceToCheck.yIndex].indexInLoop;
                int amountToMove = -numberOfTilesMoved;
                List<GamePiece> piecesInTheLoop = new List<GamePiece>();

                //clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime, new Vector3(0, 0, 0));
                //targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime, new Vector3(0, 0, 0));
                //yield return new WaitForSeconds(swapTime);
                switch (m_allTiles[gamePieceToCheck.xIndex, gamePieceToCheck.yIndex].loopNumber)
                {
                    case LoopNumber.loop0:
                        piecesInTheLoop = FindPiecesInLoop(loop0);
                        break;
                    case LoopNumber.loop1:
                        piecesInTheLoop = FindPiecesInLoop(loop1);
                        break;
                    case LoopNumber.loop2:
                        piecesInTheLoop = FindPiecesInLoop(loop2);
                        break;
                    case LoopNumber.loop3:
                        piecesInTheLoop = FindPiecesInLoop(loop3);
                        break;
                    default:
                        Debug.Log("Invalid choice");
                        break;
                }

                List<GamePiece> possibleMatches = FindMatchesAt(piecesInTheLoop);
                if (possibleMatches.Count == 0)
                {
                    StartCoroutine(MoveGemsOnLoopRoutine(amountToMove, ChooseLoop(m_allTiles[gamePieceToCheck.xIndex, gamePieceToCheck.yIndex])));
                    //ChooseLoopAndMoveGems(m_allTiles[gamePieceToCheck.xIndex, gamePieceToCheck.yIndex], amountToMove);
                    while (!isLoopDoneMoving)
                    {
                        yield return null;
                    }
                    //yield return new WaitForSeconds(0.3f);
                }
                else
                {
                    //yield return new WaitForSeconds(swapTime);
                    if (timer.enabled == false)
                    {
                        ScoreManager.Instance.SetScoreMultiplier(0);
                    }
                    currentTurn++;
                    EventManager.OnTurnDone(currentTurn);
                    ClearAndRefillBoard(possibleMatches);

                }
            }
        }
    }

    List<GamePiece> FindPiecesInLoop(List<TileMay> loop)
    {
        List<GamePiece> pieces = new List<GamePiece>();
        foreach (TileMay tile in loop)
        {
            pieces.Add(m_allGamePieces[tile.xIndex, tile.yIndex]);
        }
        return pieces;
    }
    void SetUpAllLoops()
    {
        loop0 = SetUpLoop(0, width, LoopNumber.loop0);
        loop1 = SetUpLoop(1, width - 1, LoopNumber.loop1);
        loop2 = SetUpLoop(2, width - 2, LoopNumber.loop2);
        loop3 = SetUpLoop(3, width - 3, LoopNumber.loop3);
    }
    protected bool IsNextTo(TileMay start, TileMay end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex && DoesTileBelongToTheSameLoop(start, end))
        {
            return true;
        }
        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex && DoesTileBelongToTheSameLoop(start, end))
        {
            return true;
        }
        else
        {
            return false;
        }
    } 
    bool DoesTileBelongToTheSameLoop(TileMay tile1, TileMay tile2)
    {
        return tile1.loopNumber == tile2.loopNumber ? true : false;
    }
    
}
