using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splash : MonoBehaviour
{
    Animator anim;
    [SerializeField] GameObject splashAnim;
    [SerializeField] BoardMarch m_board;
    public GameObject splashPrefab;
    List<GameObject> splashes;
    List<Animator> anims;
    public int yOffset;
    // Start is called before the first frame update
    void Start()
    {
        splashes = new List<GameObject>();
        anims = new List<Animator>();
        SetupSplashes();
    }

    void SetupSplashes()
    {
        for (int i = 0; i < m_board.width; i++)
        {

                GameObject splash = Instantiate(splashPrefab, new Vector3(i * m_board.tileSize, this.transform.position.y+ yOffset, 0), Quaternion.identity) as GameObject;
                splash.name = "Splash(" + i + ")";
                splash.transform.parent = transform;
                splashes.Add(splash);
            anims.Add(splash.GetComponent<Animator>());


        }
    }
    // Update is called once per frame
    void PlayAnimation(string trigger, int x)
    {
        
        anims[x].SetTrigger(trigger);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.tag == "Gem"&& other.GetComponent<GamePiece>().m_isMoving)
        {
            PlayAnimation("splash", other.GetComponent<GamePiece>().xIndex);
        }
    }


}
