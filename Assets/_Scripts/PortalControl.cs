using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PortalControl : MonoBehaviour
{

    public AquamarineSpecialAbility portalSpecialAbility;
    RaycastHit2D[] hits;
    public List<GamePiece> pieces;
    Tile tile;
    Vector3 direction;
    Vector3 currentPos;
    Vector3 prevPos;
    Vector3 normalizedDirection;
    public List<GamePiece> piecesAdded;
    [SerializeField]List<PieceContainer> dummyPieces;
    List<Tile> tiles;
    public Vector3 dummyDir;
    
    private void Start()
    {
        piecesAdded = new List<GamePiece>();
        tiles = new List<Tile>();
        pieces = new List<GamePiece>();
        portalSpecialAbility = this.GetComponentInParent<AquamarineSpecialAbility>();
    }

    private void OnMouseDown()
    {
        Debug.Log("clicked-Aqua");
        portalSpecialAbility.isMovementAllowed = false;
        
    }
    private void OnMouseDrag()
    {
        Debug.Log("dragging");
        if (hits == null)
        {
            
            currentPos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            hits = Physics2D.RaycastAll(MousePosition.Instance.MousePos, new Vector3(0, 0, -1), 100.0F);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject.name.Contains("Tile"))
                {
                    tile = hit.collider.gameObject.GetComponent<Tile>();
                }

            }
      
        }
        prevPos = currentPos;
        currentPos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        direction = currentPos - prevPos;
        if (direction.normalized != Vector3.zero)
        {
            dummyDir = direction.normalized;
        }

        if (direction!=Vector3.zero && pieces.Count==0)
        {
            
            normalizedDirection = direction.normalized;
            pieces = portalSpecialAbility.FindRowOrColumn(normalizedDirection, tile.yIndex, tile.xIndex);
            for (int i=0; i<pieces.Count; i++)
            {
                pieces[i].spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                dummyPieces[i].transform.position = pieces[i].transform.position;
                dummyPieces[i].piece = pieces[i];
            }
            portalSpecialAbility.PortalAppear(normalizedDirection);
            

            tiles = portalSpecialAbility.GetTilesFromGamePieces(pieces);
        }


        if (pieces.Count>0)
        {



            for (int i=0; i<dummyPieces.Count; i++)
            {   
                dummyPieces[i].transform.position += Vector3.Scale(direction, normalizedDirection);
                if (i != dummyPieces.Count - 1 && dummyPieces[i].piece == null)
                {
                    Debug.Log("Something went wrong");
                }
                


            }

        }

    }

    private void OnMouseUp()
    {   
        foreach (PieceContainer pieceContainer in dummyPieces)
        {
            pieceContainer.finishMoving = true;
            if (pieceContainer.piece != null)
            {
                piecesAdded.Add(pieceContainer.piece);
            }
        }
        portalSpecialAbility.PortalDisappear(piecesAdded, tiles);
    }

}
