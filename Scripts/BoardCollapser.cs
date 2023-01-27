using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Board))]
public class BoardCollapser : MonoBehaviour
{
    public Board board;

    private void Awake()
    {
        board = GetComponent<Board>();
    }


    public List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < board.height - 1; i++)
        {
            if (board.allGamePieces[column, i] == null && board.allTiles[column, i].tileType != TileType.Obstacle)
            {
                for (int j = i + 1; j < board.height; j++)
                {
                    if (board.allGamePieces[column, j] != null)
                    {
                        board.allGamePieces[column, j].Move(column, i, collapseTime * (j - i));
                        board.allGamePieces[column, i] = board.allGamePieces[column, j];
                        board.allGamePieces[column, i].SetCoord(column, i);
g
                        if (!movingPieces.Contains(board.allGamePieces[column, i]))
                        {
                            movingPieces.Add(board.allGamePieces[column, i]);
                        }


                        board.allGamePieces[column, j] = null;

                        break;
                    }
                }
            }
        }
        return movingPieces;
    }

    public List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        List<int> columnsToCollapse = board.boardQuery.GetColumns(gamePieces);

        foreach (int column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;
    }

    public List<GamePiece> CollapseColumn(List<int> columnsToCollapse)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        foreach (int column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }
        return movingPieces;
    }



}
