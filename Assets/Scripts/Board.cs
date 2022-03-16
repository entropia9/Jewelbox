using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

[RequireComponent(typeof(BoardDeadlock))]
public class Board : Singleton<Board>
{   
    public int width=8;
    public int height=8;
    protected int n = 0;
    public Vector2 draggingDirection;
    public GameObject tilePrefab;
    public GameObject[] gamePiecePrefabs;
    public ObjectPooler[] gamePieceObjectPoolers;
    public bool m_IsFinishedMoving;
    public Tile[,] m_allTiles;
    public GamePiece[,] m_allGamePieces;
    protected List<GamePiece> m_nextGamePieces;
    public int numberOfNextPieces = 7;
    public GameObject NextPieces;
    public Tile m_clickedTile;
    public Tile m_targetTile;
    public bool IsGoingLeft;
    public bool IsFillingOneByOne;
    public float swapTime = 0.5f;
    public int nextPiecesXposition=10;
    public int nextPiecesYpositon=1;
    public GameObject pretendPiece;
    public GameObject pretendPiece2;
    protected bool highlightAllowed;
    public int numberOfPossibleCombinations;
    public Lever lever;
    protected int m_scoreMultiplier = 0;
    public GamePiece m_clickedPiece;
    public GamePiece m_targetPiece;
    public bool m_isSwitchingEnabled = false;
    protected bool m_RefillingDone;
    public StartingObject[] startingGamePieces;
    public List<Gauge> m_gauges;
    public bool draging;
    public int numberOfGemsDestroyed=0;
    protected bool isScoringAllowed;
    protected Tile m_currentTargetTile;
    public bool m_isDragging=false;
    public int tileSize=74;
    [SerializeField] ComboMeter comboMeter;
    [SerializeField] protected TimerBehaviour timer;
    public bool draggingToTile = false;
    public int currentTurn=0;
    public Random.State randomSeed;
    enum BoardState
    {   
        Matching,
        Clearing,
        Refilling
    }
    public float portalSwapTime;
    protected BoardDeadlock m_boardDeadlock;
    public float m_globalMultiplier;
    public bool m_isCollapsing=false;
    [System.Serializable]
    public class StartingObject
    {
        public GameObject prefab;
        public int x;
        public int y;
        public int z;
    }

    MainMenu menu;
    List<List<GamePiece>> sortedPicks;
    int highlightIndex = 0;
    int sortedPicksCount;
    public bool portalEnabled=false;
    [SerializeField]ObjectPooler amberParticlePooler;
    [SerializeField] ObjectPooler aquamarineParticlePooler;
     public override void DoSomethingInAwake()
        {
        menu=MainMenu.Instance;
        if (menu != null)
        {
            if (menu.isRestarting)
            {
                Random.state = menu.storedRandomSeed;
            }
        }

    }
    void Start()
    {
        randomSeed = Random.state;
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
        m_nextGamePieces = new List<GamePiece>();
        m_boardDeadlock = GetComponent<BoardDeadlock>();
        numberOfGemsDestroyed = 0;

        EventManager.GlyphUseVoid += GlyphClearAndRefill;

    }
    #region Setup
    public void SetupBoard()
    {
        isScoringAllowed = false;
        SetupTiles();
        randomSeed = Random.state;
        FillBoard();
        SetupNextGamePieces();
        UpdatePossibleCombinations();
        m_clickedTile = null;
        m_targetTile = null;
        m_targetPiece = null;
        m_clickedPiece = null;
        isScoringAllowed = true;
    }
    protected virtual void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(i*tileSize, j*tileSize, 0), Quaternion.identity) as GameObject;
                tile.name = "Tile(" + i + "," + j + ")";
                m_allTiles[i, j] = tile.GetComponent<Tile>();
                tile.transform.parent = transform;
                m_allTiles[i, j].Init(i, j, this);


            }
        }
    }
    void SetupGamePieces()
    {
        foreach (StartingObject sPiece in startingGamePieces)
        {
            if (sPiece != null)
            {
                int index=sPiece.prefab.GetComponent<GamePiece>().matchValue.GetHashCode();
                
                GameObject piece = GetObject(index);
                piece.SetActive(true);
                FillPieceAt(piece.GetComponent<GamePiece>(), sPiece.x, sPiece.y);
            }
        }
    }
    void SetupNextGamePieces()
    {
        for (int i = 0; i < numberOfNextPieces; i++)
        {

            
            GameObject randomGem = GetRandomObject();
            randomGem.transform.position = NextPieces.transform.position;
            randomGem.transform.position = new Vector3(NextPieces.transform.position.x,NextPieces.transform.position.y+ i*tileSize, 0);
           randomGem.SetActive(true);
            if (randomGem != null)
            {
                randomGem.GetComponent<GamePiece>().Init(this);
              
                m_nextGamePieces.Add(randomGem.GetComponent<GamePiece>());


            }

        }
    }

    #endregion

    #region TileOperations
    public void ClickTile(Tile tile)
    {
        
        if (m_clickedTile == null && m_isSwitchingEnabled)
        {
            m_clickedTile = tile;
            m_clickedPiece = m_allGamePieces[m_clickedTile.xIndex, m_clickedTile.yIndex];

            //highlightAllowed = !highlightAllowed;


        }


    }
    public virtual void DragToTile(Tile tile)
    {   
       // m_currentTargetTile = tile;
        if (m_isSwitchingEnabled)
        {

            if (m_clickedTile != null && IsNextTo(tile, m_clickedTile) && m_clickedTile != tile)
            {
                Debug.Log("Tile:" + tile.xIndex.ToString() + tile.yIndex.ToString());
                m_targetTile = tile;
                m_targetPiece = m_allGamePieces[m_targetTile.xIndex, m_targetTile.yIndex];


            }
            else if (m_clickedTile != null && m_clickedTile == tile && m_targetPiece != null)
            {
                m_targetPiece.Move(m_targetPiece.xIndex, m_targetPiece.yIndex, 0.2f, Vector3.zero);
                m_targetTile = null;
            }

        }



    }
    public void DragPieces(Vector3 position, Vector3 distance)
    {
        if (m_isSwitchingEnabled)
        {
            draggingToTile = true;
            if (m_targetTile != null)
            {
                if (m_targetTile.xIndex == m_clickedTile.xIndex)
                {

                    
                    m_targetPiece.transform.position = new Vector2(position.x, m_targetTile.yIndex * tileSize - distance.y);

                }
                else if (m_targetTile.yIndex == m_clickedTile.yIndex)
                {
                    m_targetPiece.transform.position = new Vector2(m_targetTile.xIndex * tileSize - distance.x, position.y);
                }

            }

            m_clickedPiece.transform.position = position;
        }

        
    }
    public virtual void ReleaseTile()
    {
        if (m_isSwitchingEnabled)
        {   
            m_isDragging = false;
            if (m_clickedTile != null && m_targetTile != null && !SpecialAbilitiesManager.Instance.isAbilityEnabled)
            {
                timer.StopTimer();
                highlightIndex = 0;
                SwitchTiles(m_clickedTile, m_targetTile);
            }
            if (m_clickedTile != null && m_targetTile == null)
            {
                m_clickedPiece.Move(m_clickedTile.xIndex, m_clickedTile.yIndex, 0.05f, Vector3.zero);
            }
            m_clickedTile = null;
            m_targetTile = null;
            m_targetPiece = null;
            m_clickedPiece = null;
        }


    }
    protected void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));

    }
    protected virtual IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (m_isSwitchingEnabled)
        {

            GamePiece clickedPiece = m_clickedPiece;
            GamePiece targetPiece = m_targetPiece;
            if (clickedPiece != null && targetPiece != null)
            {
                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime, new Vector3(0,0,0));
                targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime, new Vector3(0, 0, 0));
                yield return new WaitForSeconds(swapTime);
                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
                if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime, new Vector3((targetPiece.xIndex - clickedPiece.xIndex) *tileSize* 0.25f, (targetPiece.yIndex - clickedPiece.yIndex) *tileSize* 0.25f, 0), 0.3f, 10, 0.75f);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime, new Vector3((clickedPiece.xIndex - targetPiece.xIndex) *tileSize* 0.25f, (clickedPiece.yIndex - targetPiece.yIndex) *tileSize* 0.25f, 0), 0.3f, 10, 0.75f);
                    
                    yield return new WaitForSeconds(0.3f);


                }
                else
                {
                   yield return new WaitForSeconds(swapTime);
                    currentTurn++;
                    EventManager.OnTurnDone(currentTurn);


                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());

                }
            }
        }
    }


    #endregion
    // Gem Operations



    GameObject GetRandomObject()
    {

        int randomIndx = Random.Range(0, gamePieceObjectPoolers.Length);
        
        if (gamePieceObjectPoolers[randomIndx] == null)
        {
            Debug.LogWarning("BOARD:" + randomIndx + "does not have valid GamePiece prefab");
        }
        GameObject gem = gamePieceObjectPoolers[randomIndx].GetPooledObject();
        gem.transform.parent = gamePieceObjectPoolers[randomIndx].transform;
        return gamePieceObjectPoolers[randomIndx].GetPooledObject();
    }

    public GameObject GetObject(int index)
    {


        if (gamePieceObjectPoolers[index] == null)
        {
            Debug.LogWarning("BOARD:" + index + "does not have valid GamePiece prefab");
        }
        GameObject gem = gamePieceObjectPoolers[index].GetPooledObject();
        gem.transform.parent = gamePieceObjectPoolers[index].transform;
        return gamePieceObjectPoolers[index].GetPooledObject();
    }
    public void PlaceGem(GamePiece gem, int x, int y)
    {
        if (gem == null)
        {
            Debug.LogWarning("BOARD: Invalid GamePiece");
        }

        gem.transform.position = new Vector3(x*tileSize, y*tileSize, 0);
        gem.transform.rotation = Quaternion.identity;
        if (IsWithinBounds(x, y))
        { m_allGamePieces[x, y] = gem; }
        gem.SetCoord(x, y);
    }
    protected IEnumerator ExplodeGem(GamePiece piece)
    {
        float timeToWait2 = 0.01f;

        if (piece != null)
        {
            
            piece.anim.Play("explode");
            SoundManager.Instance.PlayClipAtPoint(piece.explodeSound, Vector3.zero);
           // piece.ReleaseParticle();
            float timeToWait = GetAnimationClipLength(piece.anim, 0);
            if (isScoringAllowed)
            {   
                yield return new WaitForSeconds(Random.Range(0f, 0.01f));
                numberOfGemsDestroyed++;

                EventManager.OnGemDestroyed();
            }
            yield return new WaitForSeconds(timeToWait / 2);
            piece.ReleaseParticles();
            yield return new WaitForSeconds(timeToWait/2);

            DestroyGem(piece);
            yield return new WaitForSeconds(timeToWait2);

        }


        yield return null;



    }
    private void DestroyGem(GamePiece piece)
    {

   //     m_allGamePieces[x, y] = null;
        piece.transform.position = new Vector3(-1000, -1000, 0);
        piece.gameObject.SetActive(false);

    }



    //Fill

    GamePiece FillPieceAt(GamePiece piece, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {


        if (piece != null && IsWithinBounds(x, y))
        {
            piece.GetComponent<GamePiece>().Init(this);
            PlaceGem(piece.GetComponent<GamePiece>(), x, y);

            if (falseYOffset != 0)
            {
                piece.transform.position = new Vector3(x*tileSize, y*tileSize + falseYOffset, 0);
                piece.GetComponent<GamePiece>().Move(x, y, moveTime, new Vector3(0,piece.moveFactor*tileSize,0));
            }
            //piece.transform.parent = transform;

            return piece.GetComponent<GamePiece>();
        }

        return null;
    }
    GamePiece FillRandomAt(int x, int y)
    {
        GameObject randomGem = GetRandomObject();
        randomGem.transform.position = Vector3.zero;
        randomGem.SetActive(true);
        if (randomGem != null)
        {
            randomGem.GetComponent<GamePiece>().Init(this);
            PlaceGem(randomGem.GetComponent<GamePiece>(), x, y);
            //randomGem.transform.parent = transform;
            return randomGem.GetComponent<GamePiece>();
        }
        return null;
    }
    void FillBoard()
    {
        int maxIterations = 100;
        int iteration;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePieces[i, j] == null)
                {
                    GamePiece piece = FillRandomAt(i, j);
                    iteration = 0;
                    while (HasMatchOnFill(i, j))
                    {
                        DestroyGem(piece);
                        piece = FillRandomAt(i, j);
                        iteration++;
                        if (iteration > maxIterations)
                        {

                            break;
                        }
                    }
                }
            }
        }
    }

    // Find Matches

    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if (upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : null;

    }
    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        var combinedMatches = rightMatches.Union(leftMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : null;

    }
    protected List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePiece> vertMatches = FindVerticalMatches(x, y, minLength);

        if (horizMatches == null)
        {
            horizMatches = new List<GamePiece>();
        }

        if (vertMatches == null)
        {
            vertMatches = new List<GamePiece>();
        }

        var combinedMatches = horizMatches.Union(vertMatches).ToList();

        return combinedMatches;
    }
    protected List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        foreach (GamePiece piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
        }
        return matches;
    }
    protected List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                List<GamePiece> matches = FindMatchesAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }
        return combinedMatches;
    }

    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        GamePiece startPiece = null;

        if (IsWithinBounds(startX, startY))
        {
            startPiece = m_allGamePieces[startX, startY];
        }

        if (startPiece != null)
        {
            matches.Add(startPiece);
        }

        else
        {
            return null;
        }

        int nextX;
        int nextY;

        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = m_allGamePieces[nextX, nextY];
            if (nextPiece == null)
            {
                break;
            }
            else
            {
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
                {
                    matches.Add(nextPiece);
                }
                
                else
                {
                    break;
                }
            }

        }

        if (matches.Count >= minLength)
        {
            return matches;
        }

        return null;

    }

    // Clear

    public virtual void ClearPieceAt(int x, int y)
    {
        
        if (IsWithinBounds(x, y))
        {

            GamePiece pieceToClear = m_allGamePieces[x, y];
            if (pieceToClear != null)
            {
            m_allGamePieces[x, y] = null;
                StartCoroutine(ExplodeGem(pieceToClear));

            }
        }




    }
    public void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //ClearPieceAt(i, j);

                if (IsWithinBounds(i, j))
                {

                    GamePiece pieceToClear = m_allGamePieces[i, j];
                    if (pieceToClear != null)
                    {
                        m_allGamePieces[i, j] = null;

                    }
                }
            }
        }
    }
    public virtual void ClearPieceAt(List<GamePiece> gamePieces)
    {
        int scoreValue= ScoreManager.Instance.GetScoreValue(gamePieces.Count);
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {   

                if (isScoringAllowed)
                {

                        m_scoreMultiplier = comboMeter.currentIndex+1;
                    float singleScore = (scoreValue) * (m_scoreMultiplier+ m_globalMultiplier) / gamePieces.Count;
                   // ScoreManager.Instance.DisplayScoreOnGem(piece, singleScore);
                   // ScorePoints((float)scoreValue, (float)m_scoreMultiplier);
                }
                

                ClearPieceAt(piece.xIndex, piece.yIndex);
            }
        }

    }


    public void ScorePoints(float scoreValue, float multiplier = 1.0f, float bonus = 0.0f)
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore((int)((scoreValue + bonus) * (multiplier + m_globalMultiplier)));
        }

    }
    // Refill

    void RefillBoard(int falseYOffset = 0, float moveTime = 0.2f)
    {

        if (!IsGoingLeft && !IsFillingOneByOne)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (m_allGamePieces[i, j] == null)
                    {
                        RefillPieceAt(falseYOffset, moveTime, i, j);

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
                    if (m_allGamePieces[i, j] == null)
                    {

                        RefillPieceAt(falseYOffset, moveTime, i, j);




                    }

                }
            }

        }

        if (IsFillingOneByOne && !IsGoingLeft)
        {
            List<int> nullsInColumn=new List<int>();

            for (int x = 0; x < width; x++)
            {
                nullsInColumn = CheckForNullsInColumn(x);
                if (nullsInColumn.Count != 0)
                {
                    RefillPieceAt(falseYOffset, moveTime, x, nullsInColumn[0]);

                }
            }


        }
        if (IsFillingOneByOne && IsGoingLeft)
        {
            List<int> nullsInColumn=new List<int>();

            for (int x = width - 1; x >= 0; x--)
            {
                nullsInColumn = CheckForNullsInColumn(x);
                if (nullsInColumn.Count != 0)
                {
                    RefillPieceAt(falseYOffset, moveTime, x, nullsInColumn[0]);

                }
            }


        }

    }
    protected void RefillPieceAt(int falseYOffset, float moveTime, int i, int j)
    {
        GamePiece piece = m_nextGamePieces[n];
        FillPieceAt(piece, i, j, falseYOffset, moveTime);
        m_nextGamePieces.RemoveAt(n);
        RefillNextPieces();
    }


    private void RefillNextPieces(float moveTime = 0.1f)
    {
        for (int i = 0; i < m_nextGamePieces.Count; i++)
        {
            
            m_nextGamePieces[i].MoveNextPieces(NextPieces.transform.position.x, NextPieces.transform.position.y + i * tileSize, moveTime);

        }
        if (m_nextGamePieces.Count < numberOfNextPieces)
        {
            
            GameObject randomGem = GetRandomObject();
            
            randomGem.transform.position = new Vector3(NextPieces.transform.position.x, NextPieces.transform.position.y + height * tileSize, 0);
            randomGem.SetActive(true);
            
            if (randomGem != null)
            {
                randomGem.GetComponent<GamePiece>().Init(this);
                m_nextGamePieces.Add(randomGem.GetComponent<GamePiece>());
            }
            randomGem.GetComponent<GamePiece>().MoveNextPieces(NextPieces.transform.position.x, NextPieces.transform.position.y + (numberOfNextPieces - 1.0f) * tileSize, moveTime);
        }
    }

    // Clear, Collapse Refill

    public virtual List<GamePiece> CollapseColumn(int column, float collapseTime = 0.15f)
    {
        m_isCollapsing = true;
        List<GamePiece> movingPieces = new List<GamePiece>();
        for (int i = 0; i < height - 1; i++)
        {
            if (m_allGamePieces[column, i] == null)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (m_allGamePieces[column, j])
                    {
                        Vector3 strength;
                        if(IsWithinBounds(column, j + 1) && m_allGamePieces[column, j + 1]!=null&& m_allGamePieces[column, j+1].moveFactor> m_allGamePieces[column, j].moveFactor)
                        {
                            strength = new Vector3(0, m_allGamePieces[column, j + 1].moveFactor*tileSize, 0);
                        }
                        else
                        {
                            strength = new Vector3(0, m_allGamePieces[column, j].moveFactor*tileSize, 0);
                        }
                        m_allGamePieces[column, j].Move(column, i, collapseTime * (j - i), strength,0.3f*(1- m_allGamePieces[column, j].moveFactor), 5);
                        
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


    List<GamePiece> CollapseColumn(List<int> columns)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        foreach (int column in columns)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;

    }
    protected void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {

        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        m_clickedTile = null;
        m_targetTile = null;
        m_targetPiece = null;
        m_clickedPiece = null;
        highlightAllowed = false;
        m_isSwitchingEnabled = false;
        //EventManager.OnSwitchingAllowed(m_isSwitchingEnabled);
        List<GamePiece> matches = gamePieces;
        
        do
        {
            
            if (gamePieces.Count >= 6)
            {
                if (SplitLists(gamePieces, "matchValue").Count > 1)
                {
                    
                }
            }


            // clear and collapse
            //ScoreManager.Instance.DisplayMultiplier(m_scoreMultiplier);
            comboMeter.IncreaseCombo();
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));

            //refill



            if (IsFillingOneByOne)
            {
                while (AreThereNullPieces())
                {
                    yield return new WaitForSeconds(0.25f);
                    yield return StartCoroutine(RefillRoutine());
                    yield return null;
                }
            }


        
            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatches();

        }
        while (matches.Count != 0);

        UpdatePossibleCombinations();
        
        
        highlightAllowed = true;

        yield return new WaitForSeconds(0.5f);
        m_clickedTile = null;
        m_targetTile = null;
        m_targetPiece = null;
        m_clickedPiece = null;
        m_RefillingDone = true;
        m_isSwitchingEnabled = true;
        EventManager.OnSwitchingAllowed(m_isSwitchingEnabled);
        m_IsFinishedMoving = false;
        timer.StartTimer(3f);
        ScoreManager.Instance.scoreDisplayAllowed = true;
    }

    public void HandleTimerEnd()
    {
        if (comboMeter.isEmpty)
        {
            ShineOnHints();
        }
        else
        {
            comboMeter.DepleteMeter();
        }
        
    }
    public virtual IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        m_clickedTile = null;
        m_targetTile = null;
        m_targetPiece = null;
        m_clickedPiece = null;
        m_IsFinishedMoving = false;
        float delay = 0.25f;
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> nullPieces = new List<int>();
        List<GamePiece> matches = new List<GamePiece>();
        bool isFinished = false;
        float timeToWait=0f;
        while (!isFinished)
        {
            nullPieces = new List<int>();
            movingPieces = new List<GamePiece>();
            List<GamePiece> lastMovingPieces = new List<GamePiece>();
            if (gamePieces != null)
            {
                SplitListsAndUpdate(gamePieces);
            }


            for(int i=0; i<gamePieces.Count; i++)
            {   if (gamePieces[i] != null)
                {   nullPieces.Add(gamePieces[i].xIndex);
                    ClearPieceAt(gamePieces[i].xIndex, gamePieces[i].yIndex);
                    if (timeToWait < GetAnimationClipLength(gamePieces[i].anim, 0) - 0.3f)
                    {
                        timeToWait = GetAnimationClipLength(gamePieces[i].anim, 0) - 0.3f;
                    }
                    yield return new WaitForSeconds(delay / 2);
                   
                }

            }


            yield return new WaitForSeconds(timeToWait/3);




           for(int j=0; j<nullPieces.Count; j++)
            {   
                movingPieces.Union(CollapseColumn(nullPieces[j]).ToList());
                yield return new WaitForSeconds(delay / 3);
            }

                while (!IsCollapsed(movingPieces))
                {
                    yield return null;

                }
            m_isCollapsing = false;



            matches = FindMatchesAt(movingPieces);



   
            if (matches.Count == 0)
            {

                isFinished = true;
                m_IsFinishedMoving = true;
                break;
            }
            else
            {

                //m_scoreMultiplier++;
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }



    }
   protected virtual IEnumerator RefillRoutine()
    {
        m_clickedTile = null;
        m_targetTile = null;
        m_targetPiece = null;
        m_clickedPiece = null;
        //m_isSwitchingEnabled = false;
        int falseYOffset = 700;
        float moveTime = 0.2f;
        yield return null;
        //RefillBoard(700, 0.2f);
        if (!IsGoingLeft && !IsFillingOneByOne)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (m_allGamePieces[i, j] == null)
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
                    if (m_allGamePieces[i, j] == null)
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
                    RefillPieceAt(falseYOffset, moveTime, x, nullsInColumn[0]);
                    yield return new WaitForSeconds(moveTime / 2);

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
                    RefillPieceAt(falseYOffset, moveTime, x, nullsInColumn[0]);
                    yield return new WaitForSeconds(moveTime / 2);
                }
            }


        }

        yield return null;
        m_clickedTile = null;
        m_targetTile = null;
        m_targetPiece = null;
        m_clickedPiece = null;
        if (m_RefillingDone)
        {

        }
    }

    public void FindAndRefill()
    {
        List<GamePiece> matches = FindAllMatches();
        if (matches.Count != 0)
        {
            ClearAndRefillBoard(matches);
        }
        
    }

    // Special Ability Related 
    void UpdateGaugeOnBoard(int length, GamePiece piece)
    {
        foreach (Gauge gauge in m_gauges)
        {
            if (gauge.gaugeValue.ToString() == piece.matchValue.ToString())
            {

                gauge.UpdateGauge(length);
            }
        }
    }
    public void DeleteColumn(int column)
    {
        List<GamePiece> piecesToRemove;
        piecesToRemove = GetColumnPieces(column);
        ClearAndRefillBoard(piecesToRemove);
    }
    public void DeleteRow(int row)
    {
        List<GamePiece> piecesToRemove;
        piecesToRemove = GetRowPieces(row);
        ClearAndRefillBoard(piecesToRemove);
    }
    public void DeleteAdjacentPieces(int x, int y)
    {
        List<GamePiece> piecesToRemove;
        piecesToRemove = GetAdjacentPieces(x, y);
        ClearAndRefillBoard(piecesToRemove);
    }
    public void DeleteSameColorPieces(List<GamePiece> piecesToRemove)
    {
        m_scoreMultiplier++;

        ClearPieceAt(piecesToRemove);
        
    }
    public void CollapseAndRefill()
    {
        StartCoroutine(CollapseAndRefillRoutine());
    }
    IEnumerator CollapseAndRefillRoutine()
    {
        m_clickedTile = null;
        m_targetTile = null;
        m_targetPiece = null;
        m_clickedPiece = null;
        List<int> nullPieces=new List<int>();
        List<GamePiece> movingPieces = new List<GamePiece>();
        bool isFinished = false;
        List<GamePiece> matches = new List<GamePiece>();
        
        while (!isFinished)
        {

            while (IsAnimationPlaying(AllGamePieces(), "explode"))
            {
                yield return null;
            }

            nullPieces = FindNullGamePieces();

            movingPieces = CollapseColumn(nullPieces);

            //m_collapsedOnce = true;

            while (!IsCollapsed(movingPieces))
            {
                m_IsFinishedMoving = false;
                yield return null;

            }

            //           } while (!IsCollapsed(movingPieces));

            matches = FindMatchesAt(movingPieces);


            while (IsAnimationPlaying(AllGamePieces(), "explode"))
            {
                yield return null;
            }

            if (matches.Count == 0)
            {
                // m_isDestroyed = true;
                isFinished = true;
                m_IsFinishedMoving = true;
                StartCoroutine(RefillRoutine());
                StartCoroutine(ClearAndRefillBoardRoutine(FindAllMatches()));
                break;
            }
            else
            {

                //m_scoreMultiplier++;
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }

    }
    public void ChangeColor(GamePiece pieceToChange, GamePiece pieceToMatch)
    {
        if (pieceToChange.matchValue != pieceToMatch.matchValue)
        {
            int x = pieceToChange.xIndex;
            int y = pieceToChange.yIndex;
            int index = pieceToMatch.matchValue.GetHashCode();
            Debug.Log("Index " + index);
            DestroyGem(pieceToChange);
            GameObject newGem = GetObject(index);
            
            if (newGem != null)
            {
                newGem.GetComponent<GamePiece>().Init(this);
                PlaceGem(newGem.GetComponent<GamePiece>(), x, y);
                newGem.SetActive(true);
                //newGem.transform.parent = gamePieceObjectPoolers[index].transform;
                //newGem.transform.parent = transform;

            }


        }
    }



    //Highlights
    #region Highlights
    void HighlightHints()
    {
        timer.StopTimer();
        if (highlightIndex == 0)
        {
            List<List<GamePiece>> possibleMatches = m_boardDeadlock.currentPossibleMatches;
            sortedPicks=possibleMatches.OrderByDescending(x => x.Count).ToList();
            sortedPicksCount = sortedPicks.Count;
        }
        if (highlightIndex < sortedPicksCount)
        {
            StartCoroutine(HighlightHintsRoutine(sortedPicks));
            
            highlightIndex++;
        }
        else
        {
            timer.StopTimer();
        }
        

       
    }

    IEnumerator HighlightHintsRoutine(List<List<GamePiece>> sortedPicks, float delay=0.2f)
    {
        foreach (GamePiece piece in sortedPicks[highlightIndex])
        {   
            piece.anim.Play("shiny");
            yield return new WaitForSeconds(delay);
        }
        timer.StartTimer();
    }
   

    #endregion
    void UpdatePossibleCombinations()
    {   
        numberOfPossibleCombinations = m_boardDeadlock.FindNumberOfPossibleCombinations(m_allGamePieces);
        PossibleCombinatios.Instance.UpdatePossibleCombinations(numberOfPossibleCombinations);
        if (numberOfPossibleCombinations == 0)
        {
            CallZeroCombinations();
        }
    }

    public void CallZeroCombinations()
    {
        EventManager.OnZeroCombinationsReached();
    }

    protected void GlyphClearAndRefill()
    {
        StartCoroutine(GlyphClearAndRefillRoutine());
    }
    IEnumerator GlyphClearAndRefillRoutine()
    {
        isScoringAllowed = false;
        yield return new WaitForSeconds(0.5f);
        List<GamePiece> pieces=new List<GamePiece>();
        List<int> nullPieces = new List<int>();
        List<GamePiece> movingPieces = new List<GamePiece>();

        foreach (GamePiece piece in m_allGamePieces)
        {
            pieces.Add(piece);
        }
        ClearPieceAt(pieces);
       
        yield return new WaitForSeconds(2f);

        nullPieces = FindNullGamePieces();

        movingPieces = CollapseColumn(nullPieces);
        FillBoard();
        isScoringAllowed = true;
        //m_isSwitchingEnabled = true;
    }
    // Helper Functions

    public List<List<T>> SplitLists<T>(List<T> initialList,string property)
    {
        List<List<T>> listOfList = new List<List<T>>();
        if (initialList.Count != 0)
        {
            return listOfList = initialList.GroupBy(item => item.GetType().GetField(property).GetValue(item))
                                                          .Select(group => group.ToList())
                                                          .ToList();

        }
        else return null;
    }
    public void ShineOnHints()
    {
       // if(highlightAllowed)
        {
            HighlightHints(); }
        //timer.StartTimer();
        highlightAllowed = false;
  
    }
    public bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }
    bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);
        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }
        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }
        return (leftMatches.Count > 0 || downwardMatches.Count > 0);
    }
    protected bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }
        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    protected void SplitListsAndUpdate(List<GamePiece> initialList)
    {
        if (!SpecialAbilitiesManager.Instance.isAbilityEnabled)
        {   
            //List<List<GamePiece>> listOfList = new List<List<GamePiece>>();
            if (initialList.Count != 0)
            {

                //listOfList = SplitLists(initialList, "matchValue");
                var listOfList = initialList.GroupBy(item => item.matchValue);
                foreach (var list in listOfList)
                {
                    if (list != null)
                    {   
                        UpdateGaugeOnBoard(list.Count(), list.ToList()[0]);

                    }
                    int scoreValue=ScoreManager.Instance.GetScoreValue(list.ToList().Count);
                    if (isScoringAllowed)
                    {

                            m_scoreMultiplier = comboMeter.currentIndex+1;
                        
                        ScorePoints((float)scoreValue, (float)m_scoreMultiplier);
                        foreach (GamePiece piece in list)
                        {
                            float singleScore = (scoreValue) * (m_scoreMultiplier + m_globalMultiplier)/list.ToList().Count;
                            ScoreManager.Instance.DisplayScoreOnGem(piece, singleScore);
                        }
                    }

                }
            }
        }

    }
    protected bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                if (piece.transform.position.y - (float)piece.yIndex*tileSize > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }
    protected float GetAnimationClipLength(Animator animator, int clipIndex)
    {
        return animator.runtimeAnimatorController.animationClips[clipIndex].length;

    }
    public bool IsAnimationPlaying(List<GamePiece> pieces, string animationName)
    {
        AnimatorClipInfo[] m_CurrentClipInfo;
        Animator m_Animator;
        List<GamePiece> animatingPieces = new List<GamePiece>();
        foreach (GamePiece piece in pieces)
        {
            if (piece != null)
            {
                m_Animator = piece.GetComponent<Animator>();
                //Fetch the current Animation clip information for the base layer
                m_CurrentClipInfo = m_Animator.GetCurrentAnimatorClipInfo(0);


                if (m_Animator.GetCurrentAnimatorStateInfo(0).IsTag(animationName))
                {
                    animatingPieces.Add(piece);
                }
            }
        }

        if (animatingPieces.Count != 0)
        {

            return true;
        }
        else
        {

            return false;
        }
    }
    public List<int> FindNullGamePieces()
    {
        List<int> nullPieces = new List<int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (m_allGamePieces[x, y] == null)
                {
                    nullPieces.Add(x);
                }
            }
        }
        return nullPieces;
    }
    protected List<int> CheckForNullsInColumn(int column)
    {
        List<int> nullPieces=new List<int>();
        for (int y = 0; y < height; y++)
        {
            if (m_allGamePieces[column, y] == null)
            {
                nullPieces.Add(y);
            }
        }

        return nullPieces;
    }
    bool AreThereNullPieces()
    {
        List<int> nullPieces=new List<int>();
        nullPieces = FindNullGamePieces();
        if (nullPieces.Count > 0)
        {
            return true;
        }
        else return false;
    }
    List<GamePiece> AllGamePieces()
    { List<GamePiece> allPieces = new List<GamePiece>();
        for (int x =0; x<width; x++)
        {
            for(int y=0; y < height; y++)
            {
                if (m_allGamePieces[x, y] != null)
                {
                    allPieces.Add(m_allGamePieces[x, y]);
                }
            }
        }

        return allPieces;
    }
    public List<GamePiece> GetRowPieces(int row)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();
        for (int i=0; i<width; i++)
        {
            if (m_allGamePieces[i, row] != null)
            {
                gamePieces.Add(m_allGamePieces[i, row]);
            }
        }
        return gamePieces;
    }
    public List<GamePiece> GetColumnPieces(int column)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();
        for (int i = 0; i < height; i++)
        {
            if (m_allGamePieces[column, i] != null)
            {
                gamePieces.Add(m_allGamePieces[column, i]);
            }
        }
        return gamePieces;
    }
    List<GamePiece> GetAdjacentPieces(int x, int y, int offset = 1)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for(int i=x-offset; i<= x+offset; i++)
        {
            for (int j=y-offset; j<=y+offset; j++)
            {
                if (IsWithinBounds(i, j))
                {
                    gamePieces.Add(m_allGamePieces[i, j]);
                }
            }
        }
        return gamePieces;
    }
    protected List<GamePiece> FindSameColorPieces(GamePiece piece)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (m_allGamePieces[x, y] != null)
                {
                    if (piece.matchValue == m_allGamePieces[x, y].matchValue)
                    {
                        gamePieces.Add(m_allGamePieces[x, y]);
                    }
                }
            }
        }
        return gamePieces;
    }

    public virtual List<GamePiece> FindSameColorPieces(int x, int y)
    {
        return FindSameColorPieces(m_allGamePieces[x, y]);
    }
    List<GamePiece> PiecesSortedByPiece(GamePiece piece, Vector2 searchDirection)
    {
        List<GamePiece> pieces = new List<GamePiece>();

        if (searchDirection == new Vector2 (1, 0)) { 
            for (int i=piece.xIndex; i< width; i++)
            {  
                pieces.Add(m_allGamePieces[i, piece.yIndex]);
            }
            if (pieces.Count != width)
            {
                for(int j=0; j<piece.xIndex; j++)
                {
                    pieces.Add(m_allGamePieces[j, piece.yIndex]);
                }
            }
        }

        return pieces;
    }

    public void ChangeFillingMethod()
    {
        if (m_isSwitchingEnabled)
        {
            IsFillingOneByOne = !IsFillingOneByOne;
            if (IsFillingOneByOne)
            {
                SoundManager.Instance.PlaySwitchModeSound(0, SoundManager.Instance.VerticalHorizontalButtonSounds);
            }
            else
            {
                SoundManager.Instance.PlaySwitchModeSound(1, SoundManager.Instance.VerticalHorizontalButtonSounds);
            }
        }
            

    }

    public void GovingLeftOrRight()
    {
        if (m_isSwitchingEnabled)
        {
            IsGoingLeft = !IsGoingLeft;
            if (IsGoingLeft)
            {
                SoundManager.Instance.PlaySwitchModeSound(0, SoundManager.Instance.LeftRightButtonSounds);
            } else
            {
                SoundManager.Instance.PlaySwitchModeSound(1, SoundManager.Instance.LeftRightButtonSounds);
            }
        }

        

    }

    private void OnDestroy()
    {
        EventManager.GlyphUseVoid -= GlyphClearAndRefill;
    }
}
