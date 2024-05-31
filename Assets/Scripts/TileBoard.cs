using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    public Tile tilePrefab;
    public TileState[] tileStates;

    private TileGrid grid;
    private List<Tile> tiles;
    private float minSwipeDistance = 50f; 
    private Vector2 startTouchPosition, endTouchPosition;

    private bool waiting;
    private int tileSpawnCount = 0;

    public GameManager gameManager;
    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);

    }

    public void ClearBoard()
    {
        foreach (var cell in grid.cells)
        {
            cell.tile = null;
        }
        List<Tile> tilesToDestroy = new List<Tile>(tiles);
        tiles.Clear();

        foreach (var tile in tilesToDestroy)
        {
            Destroy(tile.gameObject);
        }

        tileSpawnCount = 0;
    }

    public void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        int tileNumber;

        
        if (tileSpawnCount < 2)
        {
            tileNumber = 2; 
        }
        else
        {
            int randomValue = Random.Range(0, 10); 
            tileNumber = (randomValue < 7) ? 2 : 4;
        }

        tile.SetState(tileStates[0], tileNumber);
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);

        tileSpawnCount++;
    }

    private void Update()
    { if (!waiting)
        {
            DetectSwipe();
        }
        
    }

    private void DetectSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position;
                HandleSwipe();
            }
        }
    }

    private void HandleSwipe()
    {
        float distance = Vector2.Distance(startTouchPosition, endTouchPosition);

        if (distance >= minSwipeDistance)
        {
            Vector2 swipeDirection = endTouchPosition - startTouchPosition;
            Vector2 swipeDirectionNormalized = swipeDirection.normalized;

            if (Mathf.Abs(swipeDirectionNormalized.x) > Mathf.Abs(swipeDirectionNormalized.y))
            {
                // Vuốt ngang
                if (swipeDirectionNormalized.x > 0)
                {
                    MoveTiles(Vector2Int.right, grid.width - 2, -1, 0, 1);
                }
                else
                {
                    MoveTiles(Vector2Int.left, 1, 1, 0, 1);
                    
                }
            }
            else
            {
                // Vuốt dọc
                if (swipeDirectionNormalized.y > 0)
                {
                    Debug.Log("xuong");
                    MoveTiles(Vector2Int.up, 0, 1, 1, 1);

                }
                else
                {
                    Debug.Log("Len");
                    MoveTiles(Vector2Int.down, 0, 1, grid.height - 2, -1);
                    
                }
            }
        }
    }
    private void MoveTiles(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool changed = false;
        for(int x = startX; x >= 0 && x < grid.width; x += incrementX)
        {
            for(int y = startY; y >= 0 && y < grid.height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);

                if (cell.occupied)
                {
                    changed |= MoveTile(cell.tile, direction);
                }
            }
        }

        if (changed)
        {
            StartCoroutine(WaitForChanges());
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {

        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        while (adjacent != null)
        {
            if (adjacent.occupied)
            {
                if(CanMerge(tile,adjacent.tile))
                {
                    Merge(tile,adjacent.tile);
                    return true;
                }
                break;
            }
            newCell = adjacent;
            adjacent = grid.GetAdjacentCell(adjacent,direction);
        }
        if(newCell != null)
        {
            tile.MoveTo(newCell);
            return true;
           
        }

        return false;
    }

    private bool CanMerge(Tile a, Tile b)
    {
        return a.number == b.number && !b.locked;
    }

    private void Merge(Tile a, Tile b)
    {
        tiles.Remove(a);
        a.Merge(b.cell);

        int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        int number = b.number * 2;

        b.SetState(tileStates[index], number);
        b.transform.DOScale(Vector3.one * 1.2f, 0.1f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            b.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBounce);
        });
        gameManager.IncreaseScore(number);
        WaitForChanges();
    }

    private int IndexOf(TileState state)
    {
        for(int i = 0; i < tileStates.Length; i++) 
        {
            if(state == tileStates[i]){
                return i;
            }
        }
        return -1;
    }

    private IEnumerator WaitForChanges()
    {
        waiting = true;
        yield return new WaitForSeconds(0.1f);
        waiting = false;

        foreach(var tile in tiles)
        {
            tile.locked = false;
        }

        if(tiles.Count != grid.size)
        {
            CreateTile();
        }

        if (CheckForGameOver())
        {
            gameManager.GameOver();
        }
        
    }

    private bool CheckForGameOver()
    {
        if(tiles.Count != grid.size)
        {
            return false;
        }

        foreach(var tile in tiles)
        {
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);

            if (up != null && CanMerge(tile, up.tile))
            {
                return false;
            }

            if (down != null && CanMerge(tile, down.tile))
            {
                return false;
            }

            if (left != null && CanMerge(tile, left.tile))
            {
                return false;
            }

            if (right != null && CanMerge(tile, right.tile))
            {
                return false;
            }
        }
        return true;
    }
}
