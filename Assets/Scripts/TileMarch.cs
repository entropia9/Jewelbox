using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TileMarch : Tile
{

    public int floodLevel=0;
    int maxFloodLevel=5;

    // Start is called before the first frame update


    public void ReduceFloodLevel()
    {
        if (floodLevel > 1)
        {

            floodLevel--;
            if (floodLevel == maxFloodLevel-1)
            {
                tileType = TileType.Normal;
            }
            
        } else if (floodLevel == 1)
        {
            floodLevel--;

        }
    }

    public void SetFloodLevel(int level)
    {
        floodLevel = level;

    }
    public void FloodTile()
    {
        if (floodLevel != maxFloodLevel)
        {
            floodLevel++;
           
        }
        if (floodLevel == maxFloodLevel)
        {

            tileType = TileType.Flooded;
        }
    }
}
