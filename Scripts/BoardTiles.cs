using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardTiles : MonoBehaviour
{
    public Board board;

    private void Awake()
    {
        board = GetComponent<Board>();
    }

    public void BreakTileAt(int x, int y)
    {
        Tile tileToBreak = board.allTiles[x, y];

        if (tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
        {

            if (board.particleManager != null)
            {
                board.particleManager.BreakTileFXAt(tileToBreak.breakableValue, x, y, 0);
            }

            tileToBreak.BreakTile();
        }
    }

 
    public void BreakTileAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                BreakTileAt(piece.xIndex, piece.yIndex);
            }
        }
    }
}
