using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    [SerializeField]Teleport oppositePortal;
    public BoxCollider2D teleportCollider;
    [SerializeField]AquamarineSpecialAbility portalSpecialAbility;
    public enum TeleportType
    {
        vertical,
        horizontal
    }
    public TeleportType teleportType;
    PieceContainer pieceContainer;
    [SerializeField]PieceContainer cloneContainer;
    [SerializeField] PortalControl portalControl;
    Vector3 enterDirection;
    Vector3 exitDirection;
    private void Start()
    {
        teleportCollider = this.GetComponent<BoxCollider2D>();
    }
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        pieceContainer = collision.GetComponent<PieceContainer>();
        enterDirection = portalControl.dummyDir;
        if (pieceContainer.piece != null&&pieceContainer!=cloneContainer)
        {
            
            
            DisableCollider();
            cloneContainer.transform.position = oppositePortal.transform.position;
            cloneContainer.piece = portalSpecialAbility.ClonePiece(pieceContainer.piece, oppositePortal.transform.position);
            cloneContainer.piece.spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        }
        
    }
    private void OnTriggerExit2D(Collider2D collision)
    {

        exitDirection = portalControl.dummyDir;
        if(teleportType==TeleportType.vertical && pieceContainer.piece!=null && cloneContainer.piece!=null)
        {
            if ((exitDirection.y <0 && enterDirection.y>0) || (exitDirection.y > 0 && enterDirection.y < 0))
            {
                cloneContainer.piece.gameObject.SetActive(false);
                cloneContainer.piece = null;
                cloneContainer.transform.position = new Vector3(-1000, -1000, 0);
                EnableColliders();
                Debug.Log("Exiting-DifferentDirection");
            }
            else if(pieceContainer.piece.matchValue==cloneContainer.piece.matchValue)
            {
                pieceContainer.piece.gameObject.SetActive(false);
                pieceContainer.transform.position = cloneContainer.transform.position;
                pieceContainer.piece = cloneContainer.piece;
                cloneContainer.piece = null;
                cloneContainer.transform.position = new Vector3(-1000, -1000, 0);
                Debug.Log("Exiting-SameDirection");
            }
        }
        if (teleportType == TeleportType.horizontal && pieceContainer.piece != null && cloneContainer.piece != null)
        {
            if ((exitDirection.x < 0 && enterDirection.x > 0) || (exitDirection.x > 0 && enterDirection.x < 0))
            {
                cloneContainer.piece.gameObject.SetActive(false);
                cloneContainer.piece = null;
                cloneContainer.transform.position = new Vector3(-1000, -1000, 0);
                EnableColliders();
                Debug.Log("Exiting-DifferentDirection");
            }
            else if (pieceContainer.piece.matchValue == cloneContainer.piece.matchValue)
            {
                pieceContainer.piece.gameObject.SetActive(false);
                pieceContainer.transform.position = cloneContainer.transform.position;
                pieceContainer.piece = cloneContainer.piece;
                cloneContainer.piece = null;
                cloneContainer.transform.position = new Vector3(-1000, -1000, 0);
                Debug.Log("Exiting-SameDirection");
            }
        }
    }

    private void DisableCollider()
    {   
        oppositePortal.teleportCollider.enabled = false;
    }

    void EnableColliders()
    {
        teleportCollider.enabled = true;
        oppositePortal.teleportCollider.enabled = true;
    }
}
