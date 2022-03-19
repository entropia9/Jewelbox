using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceContainer : MonoBehaviour
{
    public GamePiece piece;
    public bool finishMoving = false;
    private void Start()
    {
        piece = null; 
    }
    void FixedUpdate()
    {
        if (piece != null &&!finishMoving)
        {
            piece.transform.position = this.transform.position;
        }
    }
}
