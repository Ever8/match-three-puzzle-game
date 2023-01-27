using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Board))]
public class BoardQuery : MonoBehaviour
{
    public Board board;

    private void Awake()
    {
        board = GetComponent<Board>();
    }
    public GameObject GetRandomObject(GameObject[] objectArray)
    {
        if (board == null)
            return null;

        int randomIdx = Random.Range(0, objectArray.Length);
        if (objectArray[randomIdx] == null)
        {
            Debug.LogWarning("ERROR:  BOARD.GetRandomObject at index " + randomIdx + "does not contain a valid GameObject!");
        }
        return objectArray[randomIdx];
    }
    public GameObject GetRandomGamePiece()
    {
        if (board == null)
            return null;

        return GetRandomObject(board.gamePiecePrefabs);
    }

    public GameObject GetRandomCollectible()
    {
        if (board == null)
            return null;

        return GetRandomObject(board.collectiblePrefabs);
    }

    public List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();

        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                if (!columns.Contains(piece.xIndex))
                {
                    columns.Add(piece.xIndex);
                }
            }
        }
        return columns;
    }


    public List<GamePiece> GetRowPieces(int row)
    {
        if (board == null)
            return null;

        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < board.width; i++)
        {
            if (board.allGamePieces[i, row] != null)
            {
                gamePieces.Add(board.allGamePieces[i, row]);
            }
        }
        return gamePieces;
    }

   public List<GamePiece> GetColumnPieces(int column)
    {
        if (board == null)
            return null;

        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < board.height; i++)
        {
            if (board.allGamePieces[column, i] != null)
            {
                gamePieces.Add(board.allGamePieces[column, i]);
            }
        }
        return gamePieces;
    }

    public List<GamePiece> GetAdjacentPieces(int x, int y, int offset = 1)
    {
        if (board == null)
            return null;

        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = x - offset; i <= x + offset; i++)
        {
            for (int j = y - offset; j <= y + offset; j++)
            {
                if (board.boardQuery.IsWithinBounds(i, j))
                {
                    gamePieces.Add(board.allGamePieces[i, j]);
                }

            }
        }

        return gamePieces;
    }

    public List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
    {

        if (board == null)
            return null;

        List<GamePiece> allPiecesToClear = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                List<GamePiece> piecesToClear = new List<GamePiece>();

                Bomb bomb = piece.GetComponent<Bomb>();

                if (bomb != null)
                {
                    switch (bomb.bombType)
                    {
                        case BombType.Column:
                            piecesToClear = GetColumnPieces(bomb.xIndex);
                            break;
                        case BombType.Row:
                            piecesToClear = GetRowPieces(bomb.yIndex);
                            break;
                        case BombType.Adjacent:
                            piecesToClear = GetAdjacentPieces(bomb.xIndex, bomb.yIndex, 1);
                            break;
                        case BombType.Color:

                            break;
                    }

                    allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();

                    allPiecesToClear = board.boardQuery.RemoveCollectiblesFromList(allPiecesToClear);

                }
            }
        }

        return allPiecesToClear;
    }

    public List<GamePiece> GetCollectedPieces(List<GamePiece> gamePieces)
    {

        List<GamePiece> collectedPieces = FindCollectiblesAt(0, true);


        List<GamePiece> allCollectibles = FindAllCollectibles();
        List<GamePiece> blockers = gamePieces.Intersect(allCollectibles).ToList();

        collectedPieces = collectedPieces.Union(blockers).ToList();

        board.collectibleCount -= collectedPieces.Count;

        gamePieces = gamePieces.Union(collectedPieces).ToList();
        return collectedPieces;
    }


    public List<GamePiece> RemoveCollectiblesFromList(List<GamePiece> gamePieces)
    {

        List<GamePiece> collectiblePieces = board.boardQuery.FindAllCollectibles();
        List<GamePiece> piecesToRemove = new List<GamePiece>();

        foreach (GamePiece piece in collectiblePieces)
        {
            Collectible collectibleComponent = piece.GetComponent<Collectible>();
            if (collectibleComponent != null)
            {

                if (!collectibleComponent.clearedByBomb)
                {
                    piecesToRemove.Add(piece);
                }
            }
        }
        return gamePieces.Except(piecesToRemove).ToList();
    }

    public bool IsWithinBounds(int x, int y)
    {
        if (board == null)
            return false;

        return (x >= 0 && x < board.width && y >= 0 && y < board.height);
    }

    public bool IsColorBomb(GamePiece gamePiece)
    {
        Bomb bomb = gamePiece.GetComponent<Bomb>();

        if (bomb != null)
        {
            return (bomb.bombType == BombType.Color);
        }
        return false;
    }

    public bool IsCornerMatch(List<GamePiece> gamePieces)
    {
        bool vertical = false;
        bool horizontal = false;
        int xStart = -1;
        int yStart = -1;

        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {

                if (xStart == -1 || yStart == -1)
                {
                    xStart = piece.xIndex;
                    yStart = piece.yIndex;
                    continue;
                }

                if (piece.xIndex != xStart && piece.yIndex == yStart)
                {
                    horizontal = true;
                }


                if (piece.xIndex == xStart && piece.yIndex != yStart)
                {
                    vertical = true;
                }
            }
        }

        return (horizontal && vertical);

    }

    public bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }

        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }

        return false;
    }

    public bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                if (piece.transform.position.y - (float)piece.yIndex > 0.001f)
                {
                    return false;
                }

                if (piece.transform.position.x - (float)piece.xIndex > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }


    public bool HasMatchOnFill(int x, int y, int minLength = 3)
    {

        List<GamePiece> leftMatches = board.boardMatcher.FindMatches(x, y, new Vector2(-1, 0), minLength);

        List<GamePiece> downwardMatches = board.boardMatcher.FindMatches(x, y, new Vector2(0, -1), minLength);

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }


        return (leftMatches.Count > 0 || downwardMatches.Count > 0);

    }

    public bool CanAddCollectible()
    {

        if (board == null)
            return false;

        return (Random.Range(0f, 1f) <= board.chanceForCollectible && board.collectiblePrefabs.Length > 0 && board.collectibleCount < board.maxCollectibles);
    }


    public List<GamePiece> FindCollectiblesAt(int row, bool clearedAtBottomOnly = false)
    {
        if (board == null)
            return null;

        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int i = 0; i < board.width; i++)
        {
            if (board.allGamePieces[i, row] != null)
            {
                Collectible collectibleComponent = board.allGamePieces[i, row].GetComponent<Collectible>();

                if (collectibleComponent != null)
                {

                    if (!clearedAtBottomOnly || (clearedAtBottomOnly && collectibleComponent.clearedAtBottom))
                    {
                        foundCollectibles.Add(board.allGamePieces[i, row]);
                    }
                }
            }
        }
        return foundCollectibles;
    }

    public List<GamePiece> FindAllCollectibles(bool clearedAtBottomOnly = false)
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int i = 0; i < board.height; i++)
        {
            List<GamePiece> collectibleRow = FindCollectiblesAt(i, clearedAtBottomOnly);
            foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();
        }

        return foundCollectibles;
    }

    public List<GamePiece> FindAllMatchValue(MatchValue mValue)
    {
        if (board == null)
            return null;

        List<GamePiece> foundPieces = new List<GamePiece>();

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allGamePieces[i, j] != null)
                {
                    if (board.allGamePieces[i, j].matchValue == mValue)
                    {
                        foundPieces.Add(board.allGamePieces[i, j]);
                    }
                }
            }
        }
        return foundPieces;
    }

    public MatchValue FindMatchValue(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                return piece.matchValue;
            }
        }
        return MatchValue.None;
    }

    public GameObject FindGamePieceByMatchValue(GameObject[] prefabs, MatchValue matchValue)
    {
        if (matchValue == MatchValue.None)
        {
            return null;
        }

        foreach (GameObject go in prefabs)
        {
            GamePiece piece = go.GetComponent<GamePiece>();

            if (piece != null)
            {
                if (piece.matchValue == matchValue)
                {
                    return go;
                }
            }
        }

        return null;

    }

}
