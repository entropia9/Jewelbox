using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


public class TileFebruary : Tile
{   
    public int coldProgress = 0;
    int maxColdProgress = 4;
    int coldRate = 2;
    int currentTurn=0;
    public List<TileFebruary> neighbours;
    BoardFebruary m_boardf;
    int thawRate=5;
    int turnWhenStartedThawing=0;
    int turnWhenStaredFreezing = 0;
    int freezingRate = 3;
    public GamePiece frozenPiece;
    public List<TileFebruary> currentNeighbours;
    Animator anim;
    public void Init(int x, int y, BoardFebruary board)
    {
        xIndex = x;
        yIndex = y;
        m_boardf = board;
        

    }

    public void FindNeighbours()
    {
        neighbours = new List<TileFebruary>();
        currentNeighbours = new List<TileFebruary>();
        if (m_boardf.IsWithinBounds(xIndex - 1, yIndex))
        {
            neighbours.Add(m_boardf.m_allTiles[xIndex - 1, yIndex]);
            currentNeighbours.Add(m_boardf.m_allTiles[xIndex - 1, yIndex]);
        }
        if (m_boardf.IsWithinBounds(xIndex + 1, yIndex))
        {
            neighbours.Add(m_boardf.m_allTiles[xIndex + 1, yIndex]);
            currentNeighbours.Add(m_boardf.m_allTiles[xIndex + 1, yIndex]);
        }
        if (m_boardf.IsWithinBounds(xIndex, yIndex - 1))
        {
            neighbours.Add(m_boardf.m_allTiles[xIndex, yIndex - 1]);
            currentNeighbours.Add(m_boardf.m_allTiles[xIndex, yIndex-1]);
        }
        if (m_boardf.IsWithinBounds(xIndex, yIndex + 1))
        {
            neighbours.Add(m_boardf.m_allTiles[xIndex, yIndex + 1]);
            currentNeighbours.Add(m_boardf.m_allTiles[xIndex, yIndex+1]);
        }
        
    }

    bool cracked;
    // Start is called before the first frame update
    void Start()
    {

        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();


    }

    void OnMouseDown()
    {
 //       if (tileType != TileType.Frozen)
        {
            if (m_boardf.m_clickedTile != this)
            {
                m_boardf.ClickTile(this);

            }
        }



    }

    new void OnMouseEnter()
    {
        if (m_boardf.m_clickedTile != null && (this.tileType!=TileType.Frozen || this.tileType != TileType.Cracked))
        {
            m_boardf.DragToTile(this);
            m_boardf.draggingToTile = true;
        }




    }


    new void OnMouseUp()
    {
        if ( (this.tileType != TileType.Frozen || this.tileType != TileType.Cracked))
        {
            m_boardf.ClickTile(this);
            if (m_boardf.draggingToTile == true)
            {
                m_boardf.ReleaseTile();
                m_boardf.draggingToTile = false;
            }

        }
        else if (m_boardf.m_clickedTile != this && (this.tileType != TileType.Frozen || this.tileType != TileType.Cracked))
        {

            m_boardf.ReleaseTile();
            m_boardf.draggingToTile = false;
        }
        else
        {
            m_boardf.ReleaseTile();
        }

    }


    //ColdMechanics


    public void ClearCold()
    {
        switch (tileType)
        {
            case TileType.Cold:
                MakeNormal();
                break;
            case TileType.VeryCold:
                MakeNormal();
                break;
            case TileType.Cracked:
                MakeCold();
                break;
            case TileType.Frozen:
                MakeVeryCold();
                break;
            default:
                break;

        }
        currentNeighbours = currentNeighbours.Union(neighbours).ToList();
    }

    private void MakeNormal()
    {
        coldProgress = 0;
        this.anim.SetTrigger("Normal");
        this.tileType = TileType.Normal;
    }

    void MakeCold()
    {

        coldProgress = 1;
        this.anim.SetTrigger("Cold");
        this.tileType = TileType.Cold;
        StartThawing();


    }
    
    bool CheckIfAnyNeighboursIsOfType(TileType type)
    {
        return neighbours.Any(n=>n.tileType==type);
    }
    private void MakeVeryCold()
    {
        coldProgress = 2;
        this.anim.SetTrigger("VeryCold");
        this.tileType = TileType.VeryCold;
        StartFreezing();
    }

    private void MakeCracked()
    {
        this.tileType = TileType.Cracked;
        coldProgress = 3;
        this.anim.SetTrigger("Cracked");
    }


    public void Freeze()
    {
        currentTurn = m_boardf.currentTurn;
        frozenPiece = m_boardf.m_allGamePieces[xIndex, yIndex];
        this.tileType = TileType.Frozen;

        this.anim.SetTrigger("Frozen");
        EventManager.Turn += ChillNeighbourTiles;
    }

    void ChillNeighbourTiles(int boardTurn)
    {
        
        if(boardTurn == currentTurn + coldRate)
        {
            if (this.tileType == TileType.Frozen || this.tileType == TileType.Cracked)
            {
                  for (int i=0; i<neighbours.Count; i++)
                {
                    neighbours[i].MakeColder();
                }


            } else
            {
                EventManager.Turn -= ChillNeighbourTiles;
            }

            currentTurn = m_boardf.currentTurn;

        }
        
    }
    public void MakeColder()
    {
        switch (tileType)
        {
            case TileType.Normal:
                MakeCold();
                break;
            case TileType.Cold:
                MakeVeryCold();
                break;
            case TileType.VeryCold:
                MakeCracked();
                EventManager.Turn += ChillNeighbourTiles;
                break;
            case TileType.Cracked:
                Freeze();
                break;
            default:
                break;

        }


    }

    void MakeWarmer()
    {
        switch (tileType)
        {
            case TileType.Cold:
                MakeNormal();
                break;
            case TileType.VeryCold:
                MakeCold();
                break;
            case TileType.Cracked:
                MakeVeryCold();
                break;
            case TileType.Frozen:
                MakeCracked();
                break;
            default:
                break;

        }


    }


    public void WarmNeighbours()
    {
        for (int i=0; i<currentNeighbours.Count; i++)
        {
            currentNeighbours[i].MakeWarmer();
        }
        currentNeighbours=currentNeighbours.Union(neighbours).ToList();
    }
    void StartThawing()
    {
        if (!CheckIfAnyNeighboursIsOfType(TileType.Frozen))
        {
            turnWhenStartedThawing = m_boardf.currentTurn;
            EventManager.Turn += Thaw;
            
        }
    }
    void Thaw(int boardTurn)
    {
        if (!CheckIfAnyNeighboursIsOfType(TileType.Frozen))
        {
            if (this.tileType == TileType.Cold)
            {
                if (boardTurn == turnWhenStartedThawing + thawRate)
                {
                    MakeNormal();
                }



            }
            else
            {
                EventManager.Turn -= Thaw;
            }
        }

    }

   void StartFreezing()
    {
        if (!CheckIfAnyNeighboursIsOfType(TileType.Frozen))
        {
            turnWhenStaredFreezing = m_boardf.currentTurn;
            EventManager.Turn += FreezeVeryCold;
        }
    }
    void FreezeVeryCold(int boardTurn)
    {
        if (!CheckIfAnyNeighboursIsOfType(TileType.Frozen))
        {
            if (this.tileType == TileType.VeryCold)
            {
                if (boardTurn == turnWhenStaredFreezing+freezingRate)
                {
                    Freeze();
                }



            }
            else
            {
                EventManager.Turn -= FreezeVeryCold;
            }
        }
    }
    List<TileFebruary> RemoveNeighbour (TileFebruary tile)
    {
        if (currentNeighbours.Contains(tile))
        {
           currentNeighbours.Remove(tile);
        }
        return currentNeighbours;
    }
    public List<TileFebruary> RemoveNeighbour(List<TileFebruary> tiles)
    {   
        foreach (TileFebruary tile in tiles)
        {
            currentNeighbours.Union(RemoveNeighbour(tile));
        }
        return currentNeighbours;
    }
    private void OnDisable()
    {
        EventManager.Turn -= ChillNeighbourTiles;
        EventManager.Turn -= Thaw;
    }
}
