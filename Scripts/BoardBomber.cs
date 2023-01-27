using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]

public class BoardBomber : MonoBehaviour
{
    public Board board;

    private void Awake()
    {
        board = GetComponent<Board>();
    }


    public void ProcessBombs(Tile tileA, Tile tileB, GamePiece clickedPiece, GamePiece targetPiece, List<GamePiece> tileAPieces, List<GamePiece> tileBPieces)
    {
        Vector2 swipeDirection = new Vector2(tileB.xIndex - tileA.xIndex, tileB.yIndex - tileA.yIndex);

        board.clickedTileBomb = DropBomb(tileA.xIndex, tileA.yIndex, swipeDirection, tileAPieces);
        board.targetTileBomb = DropBomb(tileB.xIndex, tileB.yIndex, swipeDirection, tileBPieces);

        if (board.clickedTileBomb != null && targetPiece != null)
        {
            GamePiece clickedBombPiece = board.clickedTileBomb.GetComponent<GamePiece>();
            if (!board.boardQuery.IsColorBomb(clickedBombPiece))
            {
                clickedBombPiece.ChangeColor(targetPiece);
            }
        }

        if (board.targetTileBomb != null && clickedPiece != null)
        {
            GamePiece targetBombPiece = board.targetTileBomb.GetComponent<GamePiece>();

            if (!board.boardQuery.IsColorBomb(targetBombPiece))
            {
                targetBombPiece.ChangeColor(clickedPiece);
            }
        }
    }



    public List<GamePiece> ProcessColorBombs(GamePiece clickedPiece, GamePiece targetPiece,
                                             bool clearNonBlockers = false)
    {
        List<GamePiece> colorMatches = new List<GamePiece>();

        GamePiece colorBombPiece = null;
        GamePiece otherPiece = null;

        if (board.boardQuery.IsColorBomb(clickedPiece) && !board.boardQuery.IsColorBomb(targetPiece))
        {
            colorBombPiece = clickedPiece;
            otherPiece = targetPiece;
        }
        else if (!board.boardQuery.IsColorBomb(clickedPiece) && board.boardQuery.IsColorBomb(targetPiece))
        {
            colorBombPiece = targetPiece;
            otherPiece = clickedPiece;
        }
        else if (board.boardQuery.IsColorBomb(clickedPiece) && board.boardQuery.IsColorBomb(targetPiece))
        {
            foreach (GamePiece piece in board.allGamePieces)
            {
                if (!colorMatches.Contains(piece))
                {
                    colorMatches.Add(piece);
                }
            }
        }

        if (colorBombPiece != null)
        {
            colorBombPiece.matchValue = otherPiece.matchValue;

            colorMatches = board.boardQuery.FindAllMatchValue(otherPiece.matchValue);
        }

        if (!clearNonBlockers)
        {
            List<GamePiece> collectedAtBottom = board.boardQuery.FindAllCollectibles(true);

            if (collectedAtBottom.Contains(otherPiece))
            {
                return new List<GamePiece>();
            }
            else
            {
                foreach (GamePiece piece in collectedAtBottom)
                {
                    colorMatches.Remove(piece);
                }
            }
        }
        return colorMatches;
    }


    public GameObject DropBomb(int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces)
    {

        GameObject bomb = null;
        MatchValue matchValue = MatchValue.None;

        if (gamePieces != null)
        {
            matchValue = board.boardQuery.FindMatchValue(gamePieces);
        }

        if (gamePieces.Count >= 5 && matchValue != MatchValue.None)
        {
            if (board.boardQuery.IsCornerMatch(gamePieces))
            {
                GameObject adjacentBomb = board.boardQuery.FindGamePieceByMatchValue(board.adjacentBombPrefabs, matchValue);

                if (adjacentBomb != null)
                {
                    bomb = board.boardFiller.MakeBomb(adjacentBomb, x, y);
                }
            }
            else
            {

                if (board.colorBombPrefab != null)
                {
                    bomb = board.boardFiller.MakeBomb(board.colorBombPrefab, x, y);

                }
            }
        }
        else if (gamePieces.Count == 4 && matchValue != MatchValue.None)
        {
            if (Mathf.Abs(swapDirection.x) > 0.01f)
            {
                GameObject rowBomb = board.boardQuery.FindGamePieceByMatchValue(board.rowBombPrefabs, matchValue);
                if (rowBomb != null)
                {
                    bomb = board.boardFiller.MakeBomb(rowBomb, x, y);
                }
            }
            else
            {
                GameObject columnBomb = board.boardQuery.FindGamePieceByMatchValue(board.columnBombPrefabs, matchValue);
                
                if (columnBomb != null)
                {
                    bomb = board.boardFiller.MakeBomb(columnBomb, x, y);
                }
            }
        }
        return bomb;
    }

    public void InitBomb(GameObject bomb)
    {
        int x = (int)bomb.transform.position.x;
        int y = (int)bomb.transform.position.y;


        if (board.boardQuery.IsWithinBounds(x, y))
        {
            board.allGamePieces[x, y] = bomb.GetComponent<GamePiece>();
        }
    }

    public void InitAllBombs()
    {
        if (board.clickedTileBomb != null)
        {
            board.boardBomber.InitBomb(board.clickedTileBomb);
            board.clickedTileBomb = null;
        }

        if (board.targetTileBomb != null)
        {
            board.boardBomber.InitBomb(board.targetTileBomb);
            board.targetTileBomb = null;
        }
    }


}
