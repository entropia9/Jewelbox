
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : SingletonPersistent<MainMenu>
{
    AsyncOperation operation;
    [SerializeField] SpriteRenderer circuit1;
    [SerializeField] Light2D spriteLight;
    [SerializeField] Light2D spriteLightRight;
    [SerializeField] RectXformMover Left;
    [SerializeField] RectXformMover Right;
    [SerializeField] RectXformMover Top;
    [SerializeField] RectXformMover Bottom;
    [SerializeField] Slider sliderLeft;
    [SerializeField] Slider sliderRight;
    Canvas menuCanvas;
    private bool canPause = false;
    [SerializeField] Button restartButton;
    [SerializeField] Image Resume;
    [SerializeField] Image Play;
    private bool isPaused = false;
    public Random.State storedRandomSeed { get; private set; }
    public bool isRestarting { get; private set; } = false;
    [SerializeField] Animator[] leftZaps;
    [SerializeField] Animator[] rightZaps;
    bool switchingState;
    Board board;
    bool isMoving = false;
    bool isLoading = false;

    public override void DoSomethingInAwake()
    {
        spriteLight.gameObject.SetActive(false);
        spriteLightRight.gameObject.SetActive(false);
        menuCanvas = GetComponentInChildren<Canvas>(true);

    }


    void Update() {

        if (canPause && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }


    public void LoadScene(string sceneName)
    {
        UpdateProgressUI(0);

        StartCoroutine(BeginLoad(sceneName));
    }

    private IEnumerator BeginLoad(string sceneName)
    {
        isLoading = true;
        operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        spriteLight.gameObject.SetActive(true);
        spriteLightRight.gameObject.SetActive(true);


        do
        {
            UpdateProgressUI(operation.progress+0.1f);
            yield return new WaitForSeconds(0.1f);

        } while (sliderLeft.value<1.0f) ;


        circuit1.gameObject.GetComponent<Animator>().SetTrigger("stop");
        yield return new WaitForSeconds(0.4f);
        circuit1.gameObject.SetActive(false);
        operation.allowSceneActivation = true;
        menuCanvas.worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        
        for (int i = 10; i >= 0; i--)
        {
            UpdateProgressUI((float)i / 10f);
            yield return new WaitForSeconds(0.1f);
        }
        

        Bottom.MoveOff();
        yield return new WaitForSeconds(0.3f);
        Top.MoveOff();
        yield return new WaitForSeconds(0.3f);

        Right.MoveOff();
        yield return new WaitForSeconds(0.3f);
        Left.MoveOff();
        canPause = true;
        operation = null;
        isRestarting = false;
        board = Board.Instance;
        isLoading = false;
        isPaused = false;

    }

    private void UpdateProgressUI(float progress)
    {
        sliderLeft.value = progress;
        sliderRight.value = progress;


    }
    IEnumerator ZapRoutine()
    {
        List<int> zappedLeft = new List<int>();
        List<int> zappedRight = new List<int>();
        for (int i=0; i<leftZaps.Length; i++)
        {

            int left = Random.Range(0, leftZaps.Length - 1);
            int right = Random.Range(0, rightZaps.Length - 1);
       

            while (zappedLeft.Contains(left))
            {
               
                left = Random.Range(0, leftZaps.Length);

            }

            while (zappedRight.Contains(right))
            {
              
                right = Random.Range(0, leftZaps.Length);

            }

            zappedLeft.Add(left);
            zappedRight.Add(right);
            leftZaps[left].SetTrigger("zap");
            rightZaps[right].SetTrigger("zap");
            yield return new WaitForSeconds(0.2f);
        }

    }
    public void PlayGame()
    {
        if (!isMoving)
        {
            if (!isPaused && !isLoading)
            {
                LoadScene("Game");
                StartCoroutine(ZapRoutine());
            }
            else if (!isRestarting)
            {
                StartCoroutine(MovePiecesApart());
            }
        }


    }
    public void PauseGame()
    {
        if (!isPaused&&canPause)
        {
            StartCoroutine(PauseGameRoutine());
        } else if(!isMoving)
        {
            StartCoroutine(MovePiecesApart());
        }
    }
    
    IEnumerator PauseGameRoutine()
    {
        Time.timeScale = 0;
        isPaused = true;
        switchingState = board.m_isSwitchingEnabled;
        board.m_isSwitchingEnabled = false;
        restartButton.gameObject.SetActive(true);
        Play.gameObject.SetActive(false);
        Resume.gameObject.SetActive(true);
        spriteLight.gameObject.SetActive(false);
        spriteLightRight.gameObject.SetActive(false);
        isMoving = true;
        Bottom.MoveOn();
        yield return new WaitForSecondsRealtime(0.3f);
        Top.MoveOn();
        yield return new WaitForSecondsRealtime(0.3f);
        Right.MoveOn();
        yield return new WaitForSecondsRealtime(0.3f);
        Left.MoveOn();
        yield return new WaitForSecondsRealtime(0.3f);
        yield return new WaitForSecondsRealtime(0.5f);
        circuit1.gameObject.SetActive(true);
        if (sliderLeft.value != 1.0f)
        {
            for (int i = 0; i <= 10; i++)
            {
                UpdateProgressUI((float)i / 10f);
                yield return new WaitForSecondsRealtime(0.05f);
            }
        }
        
        isMoving = false;
    }

    IEnumerator MovePiecesApart()
    {
        isMoving = true;
        spriteLight.gameObject.SetActive(true);
        spriteLightRight.gameObject.SetActive(true);
        circuit1.gameObject.GetComponent<Animator>().SetTrigger("stop");
        for (int i = 10; i >= 0; i--)
        {
            UpdateProgressUI((float)i / 10f);
            yield return new WaitForSecondsRealtime(0.05f);
        }
        spriteLight.gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(0.1f);
        Bottom.MoveOff();
        yield return new WaitForSecondsRealtime(0.3f);
        Top.MoveOff();
        yield return new WaitForSecondsRealtime(0.3f);
        Right.MoveOff();
        yield return new WaitForSecondsRealtime(0.3f);
        Left.MoveOff();

        circuit1.gameObject.SetActive(false);
        board.m_isSwitchingEnabled = switchingState;
        yield return null;
        yield return new WaitForSecondsRealtime(0.5f);
        isMoving = false;
        isPaused = false;
        canPause = true;
        Time.timeScale = 1;


    }
    public void PlayMonthly()
    {
        //throw new NotImplementedException("NotImplementedYet");
    }
    public void Restart()
    {
        isRestarting = true;
        canPause = false;
        storedRandomSeed=board.randomSeed;
        board.ClearBoard();
        GlyphManager.Instance.RestartGlyphs();
        Time.timeScale = 1;
        circuit1.gameObject.SetActive(true);
        LoadScene("Game");
        //throw new NotImplementedException("NotImplementedYet");
    }

    public void OpenSettings()
    {
        //throw new NotImplementedException("NotImplementedYet");
    }
    public void OpenLeaderBoard()
    {
        //throw new NotImplementedException("NotImplementedYet");
    }
    public void OpenCustomize()
    {
        //throw new NotImplementedException("NotImplementedYet");
    }
    public void OpenProfiles()
    {
        //throw new NotImplementedException("NotImplementedYet");
    }
    public void ShowCredits()
    {
        //throw new NotImplementedException("NotImplementedYet");
    }
    public void QuitGame()
    {
        Application.Quit();
    }

}
