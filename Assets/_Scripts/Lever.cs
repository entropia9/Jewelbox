using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    readonly private float sensitivity = 0.5f;
    private Vector3 mouseReference;
    private Vector3 mouseOffset;
    private Vector3 rotation = Vector3.zero;
    private Quaternion initialRotation;
    public Transform lowerThreshold;
    public Transform higherThreshold;
    private bool m_isRotating;
    public float amountToMove;
    public List<Gauge> m_gauges;
    Vector3 downPosition;
    public Gauge selectedGauge=null;
    bool IsOut;
    bool m_isReleased=false;
    bool clockwise;
    List<Gauge> fullGauges;
    int nextIndex=0;
    public Orb abilityOrb;
    [SerializeField] SpecialAbilitiesManager specialAbilitiesManager;
    // Start is called before the first frame update
    void Start()
    {
        IsOut = false;
        initialRotation = transform.rotation;
        downPosition = transform.position;
        specialAbilitiesManager = FindObjectOfType<SpecialAbilitiesManager>();

        EventManager.DepleteGauge += LeverDown;

    }

    // Update is called once per frame
    void Update()
    { 

 
        if (m_isRotating)
        {
          // if (lowerThreshold.transform.rotation.z < transform.rotation.z && transform.rotation.z < higherThreshold.transform.rotation.z)
            {
                // offset
                mouseOffset = (Input.mousePosition - mouseReference);
            // apply rotation
            
            
            rotation.z = -(mouseOffset.x + mouseOffset.y) * sensitivity; // rotate


                transform.Rotate(rotation); // store new mouse position
                mouseReference = Input.mousePosition;
                // rotate
                //transform.Rotate(_rotation);
                if(lowerThreshold.transform.rotation.z < transform.rotation.z && transform.rotation.z < higherThreshold.transform.rotation.z)
                {
                    transform.eulerAngles += rotation;
                }
                else
                {
                    transform.eulerAngles -= rotation;
                }

            }

            // store mouse
        }
        if (m_isReleased)
        {
            List<Gauge> fullGauges = FindFullGauges();
            if (fullGauges.Count != 0)
            {
                if (clockwise)
                {

                    fullGauges = FindFullGauges();
                    SelectGaugeClockwise(fullGauges);
                }
                if (!clockwise)
                {
                    fullGauges = FindFullGauges();
                    SelectGaugeCounterClockwise(fullGauges);


                }
            }

        }
    }

    public void LeverDown(string abilityValue)
    {
        if (FindFullGauges().Count <= 1)
        {
            DragLeverDown();
            selectedGauge = null;
        }
    }

    private void OnMouseDown()
    {
        if (IsOut)
        {
            m_isRotating = true;
            
            // store mouse position
            mouseReference = Input.mousePosition;
        }
    }
    private void OnMouseUp()
    {
        //_isRotating = false;
        if (IsOut)
        {
            if (transform.eulerAngles.z >= 160 && transform.eulerAngles.z <= 360.0)
            {

                clockwise = true;
                Debug.Log("clockwise");
            }
            else if (transform.eulerAngles.z >= 0 && transform.eulerAngles.z <= 159)
            {
                clockwise = false;
                Debug.Log("counterclockwise");
            }

            if (transform.rotation.z > 0 || transform.rotation.z < 0)
            {

                transform.rotation = initialRotation;
                transform.eulerAngles = Vector3.zero;// store new mouse position
                                                     //transform.eulerAngles += _rotation;
                m_isRotating = false;
                m_isReleased = true;


            }
        }



       
    }
    

    void SelectGaugeClockwise(List<Gauge> gauges)
    {
       
 
            for (int i = 0; i < gauges.Count; i++)
            {   
                if (specialAbilitiesManager.currentSelected.abilityValue.ToString() == gauges[i].gaugeValue.ToString())
                {
                    
                    //selectedGauge.StopPlaying(24);
                    nextIndex = i+1;

                    if (nextIndex<gauges.Count)
                    {   
                        selectedGauge = gauges[nextIndex];

                    } else if (nextIndex >= gauges.Count)
                    {
                        nextIndex -= gauges.Count;
                        selectedGauge = gauges[nextIndex];
                    }

                //abilityOrb.PlayAppear(selectedGauge.gaugeValue.ToString());
                //selectedGauge.PlaySelected();
                SoundManager.Instance.PlayClipAtPoint(selectedGauge.switchSound, Vector3.zero);
                    specialAbilitiesManager.SelectAbility(selectedGauge.gaugeValue.ToString());
                    break;
                }

            }

       // }
        m_isReleased = false;
    }
    void SelectGaugeCounterClockwise(List<Gauge> gauges)
    {
        m_isReleased = false;

            for (int i = 0; i < gauges.Count; i++)
            {
                if (specialAbilitiesManager.currentSelected.abilityValue.ToString() == gauges[i].gaugeValue.ToString())
                {

                    //selectedGauge.StopPlaying(24);
                    nextIndex = i - 1;
                    if (nextIndex < gauges.Count && nextIndex>=0)
                    {
                        selectedGauge = gauges[nextIndex];
                    }
                    else if (nextIndex < 0)
                    {
                        nextIndex += gauges.Count;
                        selectedGauge = gauges[nextIndex];
                    }
                    Debug.Log("New Selected:" + selectedGauge.gaugeValue.ToString());
                //abilityOrb.PlayAppear(selectedGauge.gaugeValue.ToString());
                //                    selectedGauge.PlaySelected();
                SoundManager.Instance.PlayClipAtPoint(selectedGauge.switchSound, Vector3.zero);
                specialAbilitiesManager.SelectAbility(selectedGauge.gaugeValue.ToString());
                    break;
                }

            }

       // }

    }
    public void DragLeverUp()
    {
        if (!IsOut)
        {
            Vector3 startPosition = transform.position;

            float x = startPosition.x;
            float y = startPosition.y;
            float newX = x - amountToMove;
            float newY = y + amountToMove;
            Vector3 destination = new Vector3(newX,newY,0);
            StartCoroutine(DragLeverOutRoutine(startPosition, destination, 0.2f));
            IsOut = true;
        }

    }
    public void DragLeverDown()
    {
        if (IsOut)
        {
            transform.rotation = initialRotation;
            Vector3 startPosition = transform.position;
            float x = startPosition.x;
            float y = startPosition.y;
            float newX = downPosition.x;
            float newY = downPosition.y;
            Vector3 destination = new Vector3(newX, newY,0);
            StartCoroutine(DragLeverOutRoutine(startPosition, destination, 0.2f));
            IsOut = false;
        }
    }

    IEnumerator DragLeverOutRoutine(Vector3 startPosition, Vector3 destination, float timeToMove)
    {
        bool reachedDestination = false;
        float elapsedTime = 0f;

        while (!reachedDestination)
        {
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                reachedDestination = true;
            }
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);
            //t = Mathf.Sin(t * Mathf.PI * 0.5f); //ease out
             t= 1- Mathf.Cos(t*Mathf.PI*0.5f); // ease in
            //t = t * t * (3 - t * 2); //smoothstep
            transform.position = Vector3.Lerp(startPosition,destination, t);
            yield return null;
        }

    }

    public List<Gauge> FindFullGauges()
    {
        fullGauges=new List<Gauge>();
        foreach(Gauge gauge in m_gauges)
        {
            if (gauge.IsFull)
            {
                fullGauges.Add(gauge);
            }
            if(!gauge.IsFull && fullGauges.Contains(gauge)){
                fullGauges.Remove(gauge);
            }
        }
        return fullGauges;
    }
}
