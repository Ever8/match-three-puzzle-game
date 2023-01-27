using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardInput : MonoBehaviour
{
    public Board board;

    private void Awake()
    {
        board = GetComponent<Board>();
    }

    public void ClickTile(Tile tile)
    {
        if (board == null)
            return;

        if (board.clickedTile == null)
        {
            board.clickedTile = tile;
        }
    }

    public void DragToTile(Tile tile)
    {
        if (board == null)
            return;
        if (board.clickedTile != null && board.boardQuery.IsNextTo(tile, board.clickedTile))
        {
            board.targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (board == null)
            return;

        if (board.clickedTile != null && board.targetTile != null)
        {
            board.SwitchTiles(board.clickedTile, board.targetTile);
        }

        board.clickedTile = null;
        board.targetTile = null;
    }


}
