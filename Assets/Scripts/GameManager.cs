using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    PossibleCombinatios combinatios;
    bool m_isGameOver;
    bool m_isReadyToBegin;
    bool m_isLoopFinished=false;
    public Board m_board;
    BoardFebruary boardFebruary;
    int numberOfLoops = 6;
    public int m_currentLoop = 0;
    public MessageWindow messageWindow;
    public MessageWindow loopFinishedWindow;
    bool continuePlaying = false;
    VictoryLight victoryLight;

    [SerializeField] List<Glyph> glyphs;
    // Start is called before the first frame update
    void Start()
    {
        //messageWindow.GetComponent<RectXformMover>().MoveOff();
        m_board = FindObjectOfType<Board>().GetComponent<Board>();
        boardFebruary = FindObjectOfType<BoardFebruary>();
        combinatios = FindObjectOfType<PossibleCombinatios>();
        victoryLight = FindObjectOfType<VictoryLight>();
        StartCoroutine("ExecuteGameLoop");
    }

    // Update is called once per frame

    IEnumerator ExecuteGameLoop()
    {
        if (!continuePlaying)
        {
            yield return StartCoroutine("StartGameRoutine");
        }
        yield return StartCoroutine("PlayGameRoutine");
        yield return StartCoroutine("EndGameRoutine");
    }

    IEnumerator StartGameRoutine()
    {   while (!m_isReadyToBegin)
        {
            yield return null;
            yield return new WaitForSeconds(0.1f);
            m_isReadyToBegin = true;
        }
        if (m_board != null)
        {
            
            m_board.SetupBoard();
        }
        if (boardFebruary != null)
        {
            
        }

    }

    IEnumerator PlayGameRoutine()
    {
        while (!m_isGameOver)
        {
            if (m_board.numberOfPossibleCombinations == 0 && !CheckIfGaugesAreFull(m_board.m_gauges) && CheckIfAnyGlyphsCanBeUsed(glyphs))
            {
                EventManager.OnZeroCombinationsReached();
            }
            if (m_board.numberOfPossibleCombinations == 0 && !CheckIfGaugesAreFull(m_board.m_gauges) && !CheckIfAnyGlyphsCanBeUsed(glyphs))
            {
                m_isGameOver = true;
            }
            if (victoryLight.isLit == true)
            {
                numberOfLoops--;
                if (numberOfLoops != 0)
                {
                    m_isLoopFinished = true;
                    if (loopFinishedWindow != null)
                    {
                        Time.timeScale = 0;
                        victoryLight.TurnLightOff();
                        loopFinishedWindow.ShowMessage("Advancing to the next level requires a feature to be sacrificed. Choose wisely:");
                        loopFinishedWindow.GetComponent<SacrificeChoice>().FindOptionsToChooseFrom();
                        loopFinishedWindow.GetComponent<RectXformMover>().MoveOn();
                    }
                }
                else
                {
                    m_isLoopFinished = false;
                }
                    //m_isGameOver = true;
                
                
            }
            yield return null;
        }

        m_board.m_isSwitchingEnabled = false;
        EventManager.OnSwitchingAllowed(m_board.m_isSwitchingEnabled);

    }
    
    IEnumerator EndGameRoutine()
    {
        if (numberOfLoops==0)
        {

            Debug.Log("YOU WIN!");
        }
        else
        {
            if (messageWindow != null)
            {
                messageWindow.GetComponent<RectXformMover>().MoveOn();
                messageWindow.ShowMessage("You Lose");
            }
            SoundManager.Instance.PlayLoseSound();
            Debug.Log("YOU LOSE!");
        }

        yield return null;

    }
    public void ContinuePlaying()
    {
        m_isGameOver = false;
        StopCoroutine("EndGameRoutine");
        continuePlaying = true;
        m_board.m_isSwitchingEnabled = true;
        EventManager.OnSwitchingAllowed(m_board.m_isSwitchingEnabled);
        StartCoroutine("ExecuteGameLoop");
    }
    bool CheckIfGaugesAreFull(List<Gauge> gauges)
    {
        bool gaugesStatus = gauges.Any(gauge => gauge.IsFull == true);
        return gaugesStatus;
    }

    bool CheckIfAnyGlyphsCanBeUsed(List<Glyph> glyphs)
    {
        bool glyphsStatus = glyphs.Any(glyph => glyph.currentGlyphState==Glyph.GlyphState.active);
        return glyphsStatus;
    }

    public void Test()
    {
        m_isLoopFinished = true;
        m_isGameOver = true;
    }
}
