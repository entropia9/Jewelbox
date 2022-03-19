using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardDeadlock : MonoBehaviour
{
    public List<List<GamePiece>> currentPossibleMatches;
    SortedList<GamePiece, List<GamePiece>> matchesByUnmatchedPiece;

    void Start()
    {
        matchesByUnmatchedPiece = new SortedList<GamePiece, List<GamePiece>>(new GamePieceComparer());
    }
    List<GamePiece> GetRowOrColumnList(GamePiece[,] allPieces, int x, int y, int listLength = 3, bool checkRow = true)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);

        List<GamePiece> piecesList = new List<GamePiece>();

        for (int i = 0; i < listLength; i++)
        {
            if (checkRow)
            {
                if (x + i < width && y < height)
                {
                    if (allPieces[x + i, y] != null)
                    {
                        piecesList.Add(allPieces[x + i, y]);
                    }
                    
                }
            }
            else
            {
                if (x < width && y + i < height)
                {
                    if (allPieces[x, y + i] != null)
                    {
                        piecesList.Add(allPieces[x, y + i]);
                    }
                }
            }
        }
        return piecesList;
    }

    List<GamePiece> GetMinimumMatches(List<GamePiece> gamePieces, int minForMatch = 2)
    {
        List<GamePiece> matches = new List<GamePiece>();

        var groups = gamePieces.GroupBy(n => n?.matchValue);

        foreach (var grp in groups)
        {
            if (grp.Count() >= minForMatch)
            {
                matches = grp.ToList();
            }
        }
        return matches;
    }

    List<GamePiece> GetNeighbors(GamePiece[,] allPieces, int x, int y)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);

        List<GamePiece> neighbors = new List<GamePiece>();

        Vector2[] searchDirections = new Vector2[4]
        {
            new Vector2(-1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, -1f)
        };

        foreach (Vector2 dir in searchDirections)
        {
            if (x + (int)dir.x >= 0 && x + (int)dir.x < width && y + (int)dir.y >= 0 && y + (int)dir.y < height)
            {
                if (allPieces[x + (int)dir.x, y + (int)dir.y] != null)
                {
                    if (!neighbors.Contains(allPieces[x + (int)dir.x, y + (int)dir.y]))
                    {
                        neighbors.Add(allPieces[x + (int)dir.x, y + (int)dir.y]);
                    }
                }
            }
        }
        return neighbors;
    }

    bool HasMoveAt(GamePiece[,] allPieces, int x, int y, int listLength = 3, bool checkRow = true)
    {
        List<GamePiece> pieces = GetRowOrColumnList(allPieces, x, y, listLength, checkRow);
        List<GamePiece> matches = GetMinimumMatches(pieces, listLength - 1);
        GamePiece unmatchedPiece = null;
        List<GamePiece> neighbors = new List<GamePiece>();
       //bool added=false;
        if (pieces != null && matches != null)
        {
            if (pieces.Count == listLength && matches.Count == listLength - 1)
            {
                unmatchedPiece = pieces.Except(matches).FirstOrDefault();
            }

            if (unmatchedPiece != null)
            {   
                neighbors = GetNeighbors(allPieces, unmatchedPiece.xIndex, unmatchedPiece.yIndex);
                neighbors = neighbors.Except(matches).ToList();
                neighbors = neighbors.FindAll(n => n?.matchValue == matches[0]?.matchValue);
                matches = matches.Union(neighbors).ToList();
            }

            if (matches.Count >= listLength)
            {

                

                string rowColStr = (checkRow) ? " row " : " column ";
                Debug.Log("======= AVAILABLE MOVE ================================");
                Debug.Log("Move " + matches[0].matchValue + " piece to " + unmatchedPiece.xIndex + "," +
                    unmatchedPiece.yIndex + " to form matching " + rowColStr);

                


  /*                 if (currentPossibleMatches != null)
                       {
                           for (int i = 0; i < currentPossibleMatches?.Count; i++)
                           {   
                                   if (currentPossibleMatches[i].Contains(neighbors[0]))
                                   {   if(IsNextTo(unmatchedPiece, currentPossibleMatches[i][0]))
                            {
                                currentPossibleMatches[i] = currentPossibleMatches[i].Union(matches).ToList();
                                added = true;
                            }
                                      
                                   }


                           }
                       }
                       if (!added)
                       {
                           currentPossibleMatches.Add(matches);
                       } */

                return true;
            }
        }
        return false;
    }

    List<GamePiece> FindMatches(GamePiece[,] allPieces, int x, int y, int listLength = 3, bool checkRow = true)
    {
        List<GamePiece> pieces = GetRowOrColumnList(allPieces, x, y, listLength, checkRow);
        List<GamePiece> matches = GetMinimumMatches(pieces, listLength - 1);
        GamePiece unmatchedPiece = null;
        List<GamePiece> neighbors = new List<GamePiece>();
        //bool added=false;
        if (pieces != null && matches != null)
        {
            if (pieces.Count == listLength && matches.Count == listLength - 1)
            {
                unmatchedPiece = pieces.Except(matches).FirstOrDefault();
            }

            if (unmatchedPiece != null)
            {
                neighbors = GetNeighbors(allPieces, unmatchedPiece.xIndex, unmatchedPiece.yIndex);
                neighbors = neighbors.Except(matches).ToList();
                neighbors = neighbors.FindAll(n => n?.matchValue == matches[0]?.matchValue);
                matches = matches.Union(neighbors).ToList();
            }

            if (matches.Count >= listLength)
            {



                string rowColStr = (checkRow) ? " row " : " column ";
                Debug.Log("======= AVAILABLE MOVE ================================");
                Debug.Log("Move " + matches[0].matchValue + " piece to " + unmatchedPiece.xIndex + "," +
                    unmatchedPiece.yIndex + " to form matching " + rowColStr);




                /*                 if (currentPossibleMatches != null)
                                     {
                                         for (int i = 0; i < currentPossibleMatches?.Count; i++)
                                         {   
                                                 if (currentPossibleMatches[i].Contains(neighbors[0]))
                                                 {   if(IsNextTo(unmatchedPiece, currentPossibleMatches[i][0]))
                                          {
                                              currentPossibleMatches[i] = currentPossibleMatches[i].Union(matches).ToList();
                                              added = true;
                                          }

                                                 }


                                         }
                                     }
                                     if (!added)
                                     {
                                         currentPossibleMatches.Add(matches);
                                     } */

                
            }
        }
        return matches;
    }

    protected bool IsNextTo(GamePiece start, GamePiece end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }
        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }
        else
        {
            return false;
        }
    }






    bool IsDeadlocked(GamePiece[,] allPieces, int listLength = 3)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);
        bool isDeadlocked = true;

        for (int i=0; i<width; i++)
        {
            for(int j=0; j<height; j++)
            {   
                if (HasMoveAt(allPieces, i, j, listLength, true) || HasMoveAt(allPieces, i, j, listLength, false))
                {
                    isDeadlocked = false;
                }
            }
        }
        if (isDeadlocked)
        {
            Debug.Log("Board deadlocked");
        }
        return isDeadlocked;
    }

    public int FindNumberOfPossibleCombinations(GamePiece[,] allPieces, int listLength = 3)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);
        List<GamePiece> possibleMatches = new List<GamePiece>();
        currentPossibleMatches = new List<List<GamePiece>>();
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                List<GamePiece> horizMatches = FindMatches(allPieces, i, j, listLength, true);
                List<GamePiece> vertMatches = FindMatches(allPieces, i, j, listLength,false);
                if (currentPossibleMatches.Count > 0)
                {
                    for (int x=0; x<currentPossibleMatches.Count;x++)
                    {
                        if (horizMatches.Count > 0 && horizMatches[0].matchValue == currentPossibleMatches[x][0].matchValue)
                        {
                            if (!horizMatches.Except(currentPossibleMatches[x]).Any())
                            {
                                horizMatches = new List<GamePiece>();
                            }
                            else if (!currentPossibleMatches[x].Except(horizMatches).Any())
                            {
                                currentPossibleMatches[x] = currentPossibleMatches[x].Union(horizMatches).ToList();
                                horizMatches = new List<GamePiece>();
                            }

                        }
                        if (vertMatches.Count > 0 && vertMatches[0].matchValue == currentPossibleMatches[x][0].matchValue)
                        {
                            if (!vertMatches.Except(currentPossibleMatches[x]).Any())
                            {
                                vertMatches = new List<GamePiece>();
                            }
                            else if(!currentPossibleMatches[x].Except(vertMatches).Any())
                            {
                                currentPossibleMatches[x] = currentPossibleMatches[x].Union(vertMatches).ToList();
                                vertMatches = new List<GamePiece>();
                            }

                        }
                    }
                }

                if (horizMatches.Count >=listLength && vertMatches.Count>=listLength)
                {
                    currentPossibleMatches.Add(horizMatches);
                    currentPossibleMatches.Add(vertMatches);
                    // possibleMatches.Add(allPieces[i, j]);

                } else if (horizMatches.Count >= listLength)
                {
                    currentPossibleMatches.Add(horizMatches);
                } else if(vertMatches.Count >= listLength)
                {
                    currentPossibleMatches.Add(vertMatches);
                }
            }
        }
        
        Debug.Log("possibleMatches: " + possibleMatches.Count);
        return currentPossibleMatches.Count;
    }



   

    class GamePieceComparer : IComparer<GamePiece> 
    {
        public int Compare(GamePiece x, GamePiece y)
        {   
            return Comparer.Default.Compare(y.xIndex, x.yIndex);
        }
    }
}
