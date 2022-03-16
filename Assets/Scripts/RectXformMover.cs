using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class RectXformMover : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 onscreenPosition;
    public Vector3 endPosition;

    public float timeToMove = 3f;

    RectTransform m_rectTransform;
    bool m_isMoving;
    // Start is called before the first frame update
    void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame


    private void Move(Vector3 startPos, Vector3 endPos, float timeToMove)
    {
        if (!m_isMoving)
        {
            StartCoroutine(MoveRoutine(startPos, endPos, timeToMove));
        }
    }

    IEnumerator MoveRoutine(Vector3 startPos, Vector3 endPos, float timeToMove)
    {
        if (m_rectTransform != null)
        {
            m_rectTransform.anchoredPosition = startPos;

        }
        bool reachedDestination = false;
        float elapsedTime = 0f;
        m_isMoving = true;
        while (!reachedDestination)
        {
            if (Vector3.Distance(m_rectTransform.anchoredPosition, endPos) < 0.01f)
            {
                m_rectTransform.anchoredPosition = endPos;
                reachedDestination = true;
                break;
            }
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);
            t = t * t * t*(t * (t * 6 - 15) + 10);
            if(m_rectTransform != null)
            {
                m_rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);

            }
            yield return null;
        }
        m_isMoving = false;
    }

    public void MoveOn()
    {
        Move(endPosition, onscreenPosition, timeToMove);
    }
    public void MoveOff()
    {
        Move(onscreenPosition, endPosition, timeToMove);
    }
}
