
using System.Collections;

using UnityEditor;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class GamePiece : MonoBehaviour
{   public int xIndex;
    public int yIndex;
    Board m_board;
    public bool m_isMoving = false;
    public Sprite sprite;
    public MatchValue matchValue;

    public SpriteRenderer spriteRenderer;
    public enum MatchValue
    {
        Amber,
        Aquamarine,
        Darkred,
        Peridot,
        Purple,
        Ruby
    }
    public Animator anim;
    public float moveFactor=0.3f;
    public AudioClip explodeSound;
    ObjectPooler particlePool;
    public void Init(Board board)
    {
        m_board = board;
        anim = this.GetComponent<Animator>();


    }
    private void Awake()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            sprite = spriteRenderer.sprite;
        }
        
    }
    void Start()
    {
        List<ObjectPooler> poolers = FindObjectsOfType<ObjectPooler>().ToList();
        foreach (ObjectPooler pooler in poolers)
        {
            GameObject pooledParticle = pooler.GetPooledObject();
            pooledParticle.SetActive(true);
            Particles currentParticles = pooledParticle.GetComponent<Particles>();
            if (currentParticles!=null && matchValue.ToString() == currentParticles.particleValue.ToString())
            {
                particlePool = pooler;
            }
            pooledParticle.SetActive(false);
        }
    }
    void OnEnable()
    {
        if (sprite != null)
        {
            this.GetComponent<SpriteRenderer>().sprite = sprite;
        }

        anim = this.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("IsMatched", false);
            anim.SetBool("hint", false);
            anim.Play("NoAnimation", 0);
        }


    }
    private void OnDisable()
    {
        spriteRenderer.enabled=true;
        transform.position = new Vector3(-1000, -1000, transform.position.z);
        xIndex = -1;
        yIndex = -1;

    }

    public void ReleaseParticles()
    {
        GameObject particles = particlePool.GetPooledObject();
        particles.SetActive(true);
        particles.GetComponent<Particles>().ReleaseParticle(transform.position);
    }

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;

    }
    public void PlaceGem(int x, int y)
    {


        if (m_board.IsWithinBounds(x, y))
        {
            transform.position = new Vector3(x * m_board.tileSize, y * m_board.tileSize, 0);
            transform.rotation = Quaternion.identity;
            m_board.m_allGamePieces[x, y] = this;
            SetCoord(x, y);
        } 

    }
    public void Move(int destX, int destY, float timeToMove, Vector3 strength, float duration = 0.3f, int vibratio = 3, float elasticity = 0.25f)
    {  if (!m_isMoving)
        {
            StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove, strength, duration, vibratio,elasticity));
        }
    }

    public void MoveNextPieces(float destX, float destY, float timeToMove)
    {
        StartCoroutine(MoveNextPiecesRoutine(new Vector3(destX, destY, 0), timeToMove));
    }



    IEnumerator MoveRoutine(Vector3 destination, float timeToMove, Vector3 strength, float duration = 0.3f, int vibratio = 0, float elasticity = 1f)
    {
        Vector3 startPostion =this.transform.position;
        bool reachedDestination = false;
        float elapsedTime = 0f;
        m_isMoving = true;
        m_board.isFinishedMoving = false;
        Vector3 multiplyMovement = new Vector3(m_board.tileSize, m_board.tileSize, 0);
        Vector3 finalDestination = Vector3.Scale(destination, multiplyMovement);
        while (!reachedDestination)
        {
            if(Vector3.Distance(transform.position, finalDestination)<0.01f)
            {
                reachedDestination = true;
                if (m_board != null)
                {
                    
                    PlaceGem((int)destination.x, (int)destination.y);
                    
                }
            }
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);
            // t = Mathf.Sin(t * Mathf.PI * 0.5f); //ease out
            // t= 1- Mathf.Cos(t*Mathf.PI*0.5f); // ease in
            t = t * t * (3 - t * 2); //smoothstep
            transform.position = Vector3.Lerp(startPostion, finalDestination, t);
            
            yield return null;
        }


        
        JiggleGem(strength, duration, vibratio, elasticity);
        m_isMoving = false;
    }



    void JiggleGem(Vector3 strength, float duration=0.3f, int vibratio=3, float elasticity=0.25f)
    {
        this.gameObject.transform.DOPunchPosition(strength, duration, vibratio, elasticity);

    }



    IEnumerator MoveNextPiecesRoutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPostion = transform.position;
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
            t = t * t * (3 - t * 2); //smoothstep
            transform.position = Vector3.Lerp(startPostion, destination, t);
            yield return null;
        }

    }

    public void ExpolodeGem()
    {
        StartCoroutine(ExplodeGemRoutine());
    }
    protected IEnumerator ExplodeGemRoutine()
    {
        float timeToWait2 = 0.01f;



            anim.Play("explode");
            SoundManager.Instance.PlayClipAtPoint(explodeSound, Vector3.zero);
            float timeToWait = GetAnimationClipLength(anim, 0);
            if (ScoreManager.Instance.isScoringAllowed)
            {
                yield return new WaitForSeconds(Random.Range(0f, 0.01f));
                //numberOfGemsDestroyed++;

                EventManager.OnGemDestroyed();
            }
            yield return new WaitForSeconds(timeToWait / 2);
            ReleaseParticles();
            yield return new WaitForSeconds(timeToWait / 2);

            DestroyGem();
            yield return new WaitForSeconds(timeToWait2);

        


        yield return null;



    }

    public void DestroyGem()
    {


        transform.position = new Vector3(-1000, -1000, 0);
        gameObject.SetActive(false);

    }
    protected float GetAnimationClipLength(Animator animator, int clipIndex)
    {
        return animator.runtimeAnimatorController.animationClips[clipIndex].length;

    }
}
