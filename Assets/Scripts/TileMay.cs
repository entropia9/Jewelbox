using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LoopNumber
{
    loop0,
    loop1,
    loop2,
    loop3
}
public class TileMay : Tile
{   
    public LoopNumber loopNumber;
    public int indexInLoop;
    BoardMay m_boardm;
    public void Init(int x, int y, BoardMay board)
    {
        xIndex = x;
        yIndex = y;
        m_boardm = board;
    }

    public List<Sprite> sprites;

    public Sprite sprite;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
    }
    void OnMouseDown()
    {

        if (m_boardm.m_clickedTile != this)
        {
            m_boardm.ClickTile(this);

        }




    }

    protected new void OnMouseEnter()
    { 
        if (m_boardm.m_clickedTile != null)
        {
            m_boardm.previousTargetTile = m_boardm.m_targetTileMay;
            m_boardm.DragToTile(this);
            m_boardm.draggingToTile = true;
        }



        
    }


    protected new void OnMouseUp()
    {


        if (m_boardm.m_clickedTile != this)
        {

            m_boardm.ReleaseTile();
            m_boardm.draggingToTile = false;
        }
        else
        {
            m_boardm.ReleaseTile();
        }

    }

}
