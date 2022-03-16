using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboMeter : MonoBehaviour
{
    [SerializeField] List<Image> meters;
    Image currentActive;
    public int currentIndex=0;
    public float comboFillAmount=0.2f;
    float firstMeterFillAmount;
    readonly float updateSpeed = 0.3f;
    public bool depleting = false;
    Color currentColor;
    readonly float initialOpacity=0.15f;
    public bool isEmpty = true;
    bool stopDepleting = false;
    [SerializeField]TimerBehaviour timer;
     // Start is called before the first frame update
    void Start()
    {
        firstMeterFillAmount = 1f / 3;
        currentActive = meters[currentIndex];


    }

    public void IncreaseCombo()
    {
        isEmpty = false;
        
        if (currentIndex <= meters.Count)
        {
            StopAllCoroutines();
            stopDepleting = true;
            if (currentActive.fillAmount < 1f)
            {
                //currentActive.fillAmount += comboFillAmount;
                if (currentActive != meters[0])
                {
                    StartCoroutine(FillRoutine(currentActive.fillAmount + comboFillAmount));
                }
                else
                {
                    StartCoroutine(FillRoutine(currentActive.fillAmount + firstMeterFillAmount));
                }

            }
            else
            {
                currentIndex++;
                if (currentIndex < meters.Count)
                {
                    currentActive = meters[currentIndex];
                    IncreaseCombo();
                }
                else
                {
                    currentIndex = meters.Count - 1;
                }

            }
        }
        stopDepleting = false;
    }

    IEnumerator FillRoutine(float numberToChange)
    {
        float preChange = currentActive.fillAmount;
        float elapsed = 0f;
        
        while (elapsed < updateSpeed)
        {
            elapsed += Time.deltaTime;
            currentActive.fillAmount = Mathf.Lerp(preChange, numberToChange, elapsed / updateSpeed);

            yield return null;
        }

        currentActive.fillAmount = numberToChange;
        currentColor = currentActive.GetComponentInChildren<SpriteRenderer>().color;
        if (currentActive.fillAmount > initialOpacity)
        {
            currentColor.a = currentActive.fillAmount;
        }
        else
        {
            currentColor.a = initialOpacity;
        }
        

        currentActive.GetComponentInChildren<SpriteRenderer>().color = currentColor;

    }
    public void DepleteMeter()
    {
        StartCoroutine(DepleteMeterRoutine());
    }
    IEnumerator DepleteMeterRoutine()
    {
        yield return null;

            do
            {
            if (stopDepleting)
            {
                break;
            }
                if (currentActive != meters[0])
                {
                    StartCoroutine(FillRoutine(currentActive.fillAmount - comboFillAmount));
                }
                else
                {
                    StartCoroutine(FillRoutine(currentActive.fillAmount - firstMeterFillAmount));
                }
                yield return new WaitForSeconds(1f);
            } while (currentActive.fillAmount != 0f);
            
        if (currentIndex >= 1&&!stopDepleting)
        {
            currentIndex--;
            currentActive = meters[currentIndex];
            currentActive.GetComponentInChildren<GlowMaterialPropertyBlock>().Glow();
            timer.StartTimer();
        }
        else if (!stopDepleting)
        {   
            currentIndex = 0;
            currentActive = meters[currentIndex];
            isEmpty = true;
            timer.StartTimer();
        }

        
    }
    }
