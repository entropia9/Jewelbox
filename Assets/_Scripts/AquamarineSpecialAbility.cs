using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AquamarineSpecialAbility : SpecialAbility
{
    [SerializeField]private Transform aquamarineVertical;
    [SerializeField]private Transform aquamarineHorizontal;
    [SerializeField]private GameObject horizontalPortal;
    [SerializeField]private GameObject verticalPortal;
    private bool draggingAllowed = true;
    private Animator horizontalPortal1;
    private Animator horizontalPortal2;
    private Animator verticalPortal1;
    private Animator verticalPortal2;
    private new void Start()
    {
        m_board = Board.Instance;
        specialAbilitiesManager = SpecialAbilitiesManager.Instance;
        specialAbilitiesManager.isAbilityEnabled = true;
        horizontalPortal1=horizontalPortal.transform.GetChild(0).GetComponent<Animator>();
        horizontalPortal2 = horizontalPortal.transform.GetChild(1).GetComponent<Animator>();
        verticalPortal1=verticalPortal.transform.GetChild(0).GetComponent<Animator>();
        verticalPortal2=verticalPortal.transform.GetChild(1).GetComponent<Animator>();
        if (this.GetComponent<Animator>() != null)
        {
            anim = this.GetComponent<Animator>();
        }
        isSuperCharged = false;
        m_board.portalEnabled = draggingAllowed;
    }
    private void FixedUpdate()
    {

        if (isMovementAllowed)
        {
            x = (int)(Mathf.Round(mousepos.x/tileSize)*tileSize);
            y = (int)(Mathf.Round(mousepos.y/tileSize)*tileSize);
            this.transform.position= new Vector3(Mathf.Clamp(x, 0, m_board.width*tileSize - 1*tileSize), Mathf.Clamp(y, 0, m_board.height*tileSize- 1*tileSize), transform.position.z);
            aquamarineVertical.transform.position = new Vector3(Mathf.Clamp(x, 0, m_board.width*tileSize - 1*tileSize), (m_board.height*tileSize - tileSize) / 2.0f, transform.position.z);
            aquamarineHorizontal.transform.position = new Vector3((m_board.width*tileSize - 1.0f*tileSize) / 2.0f, Mathf.Clamp(y, 0, (m_board.height - 1)*tileSize), transform.position.z);
        }
    }


    public void PortalAppear(Vector3 draggingDirection)
    {
        isMovementAllowed= false;
        draggingAllowed = true;
        aquamarineHorizontal.GetComponent<SpriteRenderer>().enabled = false;
        aquamarineVertical.GetComponent<SpriteRenderer>().enabled = false;
        horizontalPortal.SetActive(true);
        verticalPortal.SetActive(true);
        if (draggingDirection == Vector3.left || draggingDirection == Vector3.right)
        {
            
            horizontalPortal.transform.GetChild(0).gameObject.SetActive(true);
            horizontalPortal.transform.GetChild(1).gameObject.SetActive(true);
            verticalPortal.transform.GetChild(0).gameObject.SetActive(false);
            verticalPortal.transform.GetChild(1).gameObject.SetActive(false);
        }
        else if (draggingDirection == Vector3.up || draggingDirection == Vector3.down)
        {
            
            verticalPortal.transform.GetChild(0).gameObject.SetActive(true);
            verticalPortal.transform.GetChild(1).gameObject.SetActive(true);
            horizontalPortal.transform.GetChild(0).gameObject.SetActive(false);
            horizontalPortal.transform.GetChild(1).gameObject.SetActive(false);
        }
   
    }
    public List<GamePiece> FindRowOrColumn(Vector3 direction, int row, int column)
    { List<GamePiece> pieces = new List<GamePiece>();
        if(direction == Vector3Int.left || direction == Vector3Int.right)
        {

            pieces = m_board.GetRowPieces(row);
        } else if(direction == Vector3Int.up || direction == Vector3Int.down)
        {

            pieces = m_board.GetColumnPieces(column);
        }
        
        return pieces;
    }
    public GamePiece ClonePiece(GamePiece piece, Vector3 portalPosition)
    {
        GameObject gem = null;

        if (piece != null)
        {
            gem = m_board.GetObject(piece.matchValue.GetHashCode());
            if (gem == null)
            {
                Debug.LogWarning("BOARD: Invalid GamePiece");
            }

            gem.transform.position = portalPosition;
            gem.transform.rotation = Quaternion.identity;
        }




        if (gem != null)
        {
            gem.SetActive(true);
            return gem.GetComponent<GamePiece>();
        }
        else
        {
            return null;
        }

    }

    public void PortalDisappear(List<GamePiece> pieces, List<Tile> tiles)
    {
        isMovementAllowed = false;
        timesToUse -= 1;
        EventManager.OnDepleteGauge(this.abilityValue.ToString());
        

        horizontalPortal1.SetTrigger("disappear");
        horizontalPortal2.SetTrigger("disappear");
        verticalPortal1.SetTrigger("disappear");
        verticalPortal2.SetTrigger("disappear");

        GamePiece piece;
        Tile currentTile;
        int i = 0;
        while (tiles.Count != 0)
        {
            currentTile = tiles[0];
            piece = ReturnClosestPiece(currentTile, pieces);
            tiles.Remove(currentTile);
            pieces.Remove(piece);
            piece.gameObject.SetActive(true);
            piece.spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
            piece.Init(m_board);
            piece.PlaceGem(currentTile.xIndex, currentTile.yIndex);
            i++;
            if (i > m_board.width)
            {
                break;
            }
        }

        if (pieces.Count == 1)
        {
            pieces[0].gameObject.SetActive(false);
        }

        m_board.FindAndRefill();
        if (timesToUse <= 0)
        {   
            abilityEnabled = false;
            m_board.m_isSwitchingEnabled = true;
            DisableAbility();

        }
        else
        {
            StartCoroutine(ReenableAbilityRoutine());
        }
    }

    IEnumerator ReenableAbilityRoutine()
        {

            while (!m_board.isFinishedMoving)
            {
                yield return null;
            }
            aquamarineHorizontal.GetComponent<SpriteRenderer>().enabled = true;
            aquamarineVertical.GetComponent<SpriteRenderer>().enabled = true;
            isMovementAllowed = true;

            
        }


    GamePiece ReturnClosestPiece(Tile tile, List<GamePiece> pieces)
    {
        Vector3 _currentPiecePosition;
        float closestDistanceSqr = Mathf.Infinity;
        GamePiece closestPiece = null ;
        for (int i = 0; i < pieces.Count; i++)
        {

            {
                _currentPiecePosition = pieces[i].transform.position - tile.transform.position;
                float dSqr = _currentPiecePosition.sqrMagnitude;

                if (dSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqr;
                    closestPiece=pieces[i];
                }



            }
        }
        return closestPiece;
    }
    public List<Tile> GetTilesFromGamePieces(List<GamePiece> pieces)
    {
        List<Tile> tiles = new List<Tile>();
        foreach(GamePiece piece in pieces)
        {
            tiles.Add(m_board.m_allTiles[piece.xIndex, piece.yIndex]); 
        }
        return tiles;
    }
    }

