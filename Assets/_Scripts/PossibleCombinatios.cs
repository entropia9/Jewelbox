using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PossibleCombinatios : Singleton<PossibleCombinatios>
{

    [SerializeField] TMP_Text possibleMatchestText;
    int m_currentValue=0;
    int m_counterValue=0;
    float extraGlow=1f;
    float normalGlow=0.4f;

    public void UpdateScoreText(int updateValue)
    {   
        if (updateValue < 10)
        {
            
            possibleMatchestText.text = "0" + updateValue.ToString();
        }
        else
        {
            possibleMatchestText.text = updateValue.ToString();
        }
        possibleMatchestText.fontMaterial.SetFloat("_GlowPower", extraGlow);

    }

    public void UpdatePossibleCombinations(int possbileMatchesCount)
    {
        m_currentValue = possbileMatchesCount;
        StartCoroutine(CountRoutine());
        
    }

    IEnumerator CountRoutine()
    {
        yield return null;

        while (m_currentValue!= m_counterValue)
        {
            if (m_counterValue > m_currentValue)
            {
                m_counterValue--;
            }
            else
            {
                m_counterValue++;
            }
            UpdateScoreText(m_counterValue);
            yield return new WaitForSeconds(0.05f);
            possibleMatchestText.fontMaterial.SetFloat("_GlowPower", normalGlow);
        }
        possibleMatchestText.fontMaterial.SetFloat("_GlowPower", normalGlow);
    }


}
