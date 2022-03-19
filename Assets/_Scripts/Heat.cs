using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heat : MonoBehaviour
{
    public int heatLevel = 0;
    int maxHeatLevel = 6;
    int coolDownInterval = 4;
    int turnSinceMovement=0;
    SpriteRenderer spriteRenderer;
    public SpriteRenderer gemSpriteRenderer;
    GamePiece gamePiece;
    Color color;
    Animator fireAnimator;
    BoardJuly m_board;
    // Start is called before the first frame update
    private void Start()
    {
        m_board = FindObjectOfType<BoardJuly>();
        gamePiece = this.GetComponentInParent<GamePiece>();
    }


    void CoolGemDown(int currentTurn)
    {
        if(m_board.IsWithinBounds(gamePiece.xIndex, gamePiece.yIndex))
        {
            if (m_board.m_allTiles[gamePiece.xIndex, gamePiece.yIndex].tileType != TileType.Burnt)
            {
                if (turnSinceMovement == coolDownInterval)
                {
                    if (heatLevel > 0)
                    {
                        heatLevel--;
                        color.a = heatLevel * 0.2f;
                        GetComponent<SpriteRenderer>().color = color;
                    }
                    turnSinceMovement = 0;
                }
            }
            else
            {
                EventManager.Turn -= IncreaseTurnSinceHeating;
                EventManager.Turn -= CoolGemDown;
            }
        }


    }

    public void StartOverheating()
    {   if (heatLevel == 0)
        {
            heatLevel = 1;
            color.a = heatLevel * 0.2f;
            GetComponent<SpriteRenderer>().color = color;
            EventManager.Turn += IncreaseTurnSinceHeating;
            EventManager.Turn += CoolGemDown;
        }
        else
        {
            IncreaseHeatLevel();
        }


    }
    void IncreaseTurnSinceHeating(int currentTurn)
    {
        turnSinceMovement++;
    }
    public void IncreaseHeatLevel()
    {

            if (heatLevel!=0 && m_board.m_allTiles[gamePiece.xIndex, gamePiece.yIndex].tileType != TileType.Burnt)
            {
                turnSinceMovement = 0;
                heatLevel++;
                color.a = heatLevel * 0.2f;
                GetComponent<SpriteRenderer>().color = color;
                if (heatLevel >= maxHeatLevel)
                {
                    StartCoroutine(BurnGemDown());
                }
            }
        

    }




    
    IEnumerator BurnGemDown()
    {
        fireAnimator.SetTrigger("Burn");
        yield return new WaitForSeconds(0.7f);
        gemSpriteRenderer.enabled = false;
        spriteRenderer.enabled = false;
        yield return new WaitForSeconds(0.1f);
        m_board.m_allTiles[gamePiece.xIndex, gamePiece.yIndex].tileType = TileType.Burnt;
    }

    private void OnEnable()
    {
        heatLevel = 0;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        

            color = spriteRenderer.color;
            color.a = 0f;
            GetComponent<SpriteRenderer>().color = color;
        
        fireAnimator = GetComponentInChildren<Animator>();
        
    }



    private void OnDisable()
    {
        heatLevel = 0;
        
    }

}
