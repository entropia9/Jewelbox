using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using TMPro;

public class ScoreManager : Singleton<ScoreManager>
{
    private int m_currentScore = 0;
    private int m_counterValue = 0;
    public bool scoreDisplayAllowed = true;
    Dictionary<int, int> ScoreValues;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text scoreText;
    readonly float extraGlow = 1f;
    readonly float normalGlow = 0.4f;
    [SerializeField] private ObjectPooler gemScoreTexts;
    public int scoreMultiplier { get; private set; } = 0;
    public bool isScoringAllowed { get; private set; }
    public float globalMultiplier;
    private void Start()
    {
        ScoreValues = new Dictionary<int, int>
    {
        {3,30 },
        {4,50 },
        {5,90 },
        {6,150 },
        {7,200 },
        {8,300 }
    };

    }

    public void RestartScore()
    {
        m_currentScore = 0;
        m_counterValue = 0;
        UpdateScoreText(m_currentScore);
    }
    public void UpdateScoreText(int scoreValue)
    {
        Debug.Log("SCORE: " + scoreValue.ToString());
        descriptionText.text = "SCORE:";
        scoreText.text = scoreValue.ToString();
        scoreText.fontMaterial.SetFloat("_GlowPower", extraGlow);

    }
    public void DisplayScoreOnGem(GamePiece gem, float scoreValue)
    {
        GameObject _scoreText = gemScoreTexts.GetPooledObject();
        _scoreText.SetActive(true);
        _scoreText.transform.position = gem.transform.position;
        TMP_Text gemScoreText = _scoreText.GetComponent<TMP_Text>();
        gemScoreText.text = scoreValue.ToString();
        StartCoroutine(DisappearScore(gemScoreText, _scoreText));

    }
    IEnumerator DisappearScore(TMP_Text gemScoreText, GameObject scoreText)
    {
        yield return new WaitForSeconds(0.2f);
        Color color = gemScoreText.color;
        while (color.a <= 0)
        {
            color.a -= 0.2f;
            yield return new WaitForSeconds(0.2f);
            gemScoreText.color = color;
        }
        yield return new WaitForSeconds(0.2f);
        color.a = 1.0f;
        gemScoreText.color = color;
        yield return new WaitForSeconds(0.2f);
        scoreText.SetActive(false);
    }
    public int GetScoreValue(int scoreValueKey)
    {

        if (ScoreValues != null)
        {
            if (ScoreValues.ContainsKey(scoreValueKey))
            {
                return ScoreValues[scoreValueKey];
            }
            else return 50;
        }
        else return 0;
    }
    public void AddScore(int value)
    {
        m_currentScore += value;
        StartCoroutine(CountScoreRoutine());
    }
    public void DisplayMultiplier(int value)
    {
        FindObjectOfType<ComboMeter>().IncreaseCombo();
        if (value > 1)
        {
            StartCoroutine(DisplayMultiplierRoutine(value));

            scoreDisplayAllowed = false;
        }

    }

    IEnumerator DisplayMultiplierRoutine(int value)
    { yield return null;
        descriptionText.text = " ";
        scoreText.text = "COMBO x" + value.ToString();

    }

    IEnumerator CountScoreRoutine()
    {

        while (!scoreDisplayAllowed)
        {
            yield return null;
        }
        yield return null;
        while (m_counterValue != m_currentScore)
        {
            UpdateScoreText(m_counterValue);
            m_counterValue++;
            yield return null;
            scoreText.fontMaterial.SetFloat("_GlowPower", normalGlow);
        }
        m_counterValue = m_currentScore;
        UpdateScoreText(m_counterValue);
        scoreText.fontMaterial.SetFloat("_GlowPower", normalGlow);
    }
    public void ScorePoints(float scoreValue, float bonus = 0.0f)
    {

        AddScore((int)((scoreValue + bonus) * (scoreMultiplier)));


    }
    public void SetScoreMultiplier(int multiplier)
    {
        scoreMultiplier = multiplier;
    }
    public void AllowScoring(bool canscore)
    {
        isScoringAllowed = true;
    }
}
