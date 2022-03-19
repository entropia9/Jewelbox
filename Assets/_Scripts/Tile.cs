using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TileType
{
    Normal,
    VeryCold,
    Cold,
    Cracked,
    Frozen,
    Flooded,
    Detached,
    Burnt
}

public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    Board m_board;
    
    public SpriteRenderer spriteRenderer;

    public TileType tileType = TileType.Normal;

    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        m_board = board;
    }

    private void Start()
    {
        startPos = this.transform.position;
        spriteRenderer=GetComponent<SpriteRenderer>();
        
    }
    Vector3 startPos;
    Vector3 currentPos;
    Vector3 distance;
    Vector3 distanceFromStart;
    Vector3 position;
    public List<Tile> curNeighbours;
    public string lockAxis;
    void OnMouseDown()
    {
        if (!SpecialAbilitiesManager.Instance.isAbilityEnabled)
        {
            if (m_board.m_clickedTile != this)
            {
                if (curNeighbours.Count == 0)
                {
                    FindNeighbours();
                }
                m_board.ClickTile(this);

                startPos = this.transform.position;
                currentPos = startPos;
            }


        }




    }
    public void FindNeighbours()
    {
        curNeighbours = new List<Tile>();
        if (m_board.IsWithinBounds(xIndex - 1, yIndex))
        {
            curNeighbours.Add(m_board.m_allTiles[xIndex - 1, yIndex]);
        }
        if (m_board.IsWithinBounds(xIndex + 1, yIndex))
        {
            curNeighbours.Add(m_board.m_allTiles[xIndex + 1, yIndex]);
        }
        if (m_board.IsWithinBounds(xIndex, yIndex - 1))
        {
            curNeighbours.Add(m_board.m_allTiles[xIndex, yIndex - 1]);
        }
        if (m_board.IsWithinBounds(xIndex, yIndex + 1))
        {
            curNeighbours.Add(m_board.m_allTiles[xIndex, yIndex + 1]);
        }

    }


    private void OnMouseDrag()
    {   
        if (m_board.m_clickedTile == this)
        {

            position = (Vector2)MousePosition.Instance.MousePos;
            Tile tile=null;


            RaycastHit2D hit = Physics2D.Raycast(position, new Vector3(0, 0, -1), 100.0F);
            if (hit.collider != null)
            {
                tile = hit.collider.GetComponent<Tile>();
                if (m_board.m_targetTile == null)
                {
                    if (tile != null)
                    {
                        if (tile.xIndex == this.xIndex)
                        {
                            lockAxis = "x";
                        }
                        if (tile.yIndex == this.yIndex)
                        {
                            lockAxis = "y";
                        }
                    }
                    m_board.DragToTile(tile);
                }
                if (m_board.m_clickedTile == tile)
                {
                    if (m_board.m_targetPiece != null && m_board.m_targetTile!=null)
                    {
                        m_board.m_targetPiece.Move(m_board.m_targetTile.xIndex, m_board.m_targetTile.yIndex, 0.1f, Vector3.zero);
                    }
                    m_board.m_targetTile = null;
                    lockAxis = "";
                }
            }
            if (lockAxis == "x")
            {
                position.x = m_board.m_clickedTile.xIndex * m_board.tileSize;
                
                position.y = Mathf.Clamp(position.y, (yIndex - 1) * m_board.tileSize, (yIndex + 1) * m_board.tileSize);

            }
            if (lockAxis == "y")
            {
                position.y = m_board.m_clickedTile.yIndex * m_board.tileSize;
                position.x = Mathf.Clamp(position.x, (xIndex - 1) * m_board.tileSize, (xIndex + 1) * m_board.tileSize);
     
            }
            if (lockAxis == "")
            {
                if (position.y > position.x)
                {
                    position.x = xIndex * m_board.tileSize;
                }
                else
                {
                    position.y = yIndex * m_board.tileSize;
                }
            }
            position.z = this.transform.position.z;

            distance = position - currentPos;
            
            currentPos = distance;

            distanceFromStart = position - startPos;
            if (curNeighbours.Contains(tile) || tile ==this)
            {
                m_board.DragPieces(position, distanceFromStart);
            }
            
 
        }

    }
    protected void OnMouseUp()
    {

        {
            m_board.ReleaseTile();
        }
    }
    
}
