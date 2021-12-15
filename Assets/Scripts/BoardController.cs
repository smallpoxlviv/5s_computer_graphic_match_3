using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public static BoardController instance;

    private int xSize, ySize;
    private List<Sprite> tileSprite = new List<Sprite>();
    private Tile[,] tileArray;

    private Tile oldSelectTile;
    private Vector2[] dirRay = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    private bool isFindMatch = false;
    private bool isShift = false;
    private bool isSearchEmptyTile = false;


    public void SetValue(Tile[,] tileArray, int xSize, int ySize, List<Sprite> tileSprite)
    {
        this.tileArray = tileArray;
        this.xSize = xSize;
        this.ySize = ySize;
        this.tileSprite = tileSprite;
    }

    private void Awake()
    {
        instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isSearchEmptyTile)
        {
            SearchEmptyTile();
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
            if (ray != false)
            {
                CheckSelectTile(ray.collider.gameObject.GetComponent<Tile>());
            }
        }
    }

    public void SelectTile(Tile tile)
    {
        tile.isSelected = true;
        tile.spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
        oldSelectTile = tile;
    }

    public void DeselectTile(Tile tile)
    {
        tile.isSelected = false;
        tile.spriteRenderer.color = new Color(1, 1, 1);
        oldSelectTile = null;
    }

    public void CheckSelectTile(Tile tile)
    {
        if (tile.isEmpty || isShift)
        {
            return;
        }
        if (tile.isSelected)
        {
            DeselectTile(tile);
        }
        else
        {
            // first tile
            if (!tile.isSelected && oldSelectTile == null)
            {
                SelectTile(tile);
            }
            // second tile
            else
            {
                // if second is neighbour
                if (AdjacentTiles().Contains(tile))
                {
                    SwapTwoTiles(tile);
                    FindAllMatch(tile);
                    DeselectTile(oldSelectTile);
                }
                // select new tile
                else
                {
                    DeselectTile(oldSelectTile);
                    SelectTile(tile);
                }
                
            }
        }
    }

    #region(search matches, delete sprite, move tile, change sprite in tiles)

    private List<Tile> findMatch(Tile tile, Vector2 dir)
    {
        List<Tile> cashFindTiles = new List<Tile>();
        RaycastHit2D hit = Physics2D.Raycast(tile.transform.position, dir);
        while (hit.collider != null &&
            hit.collider.gameObject.GetComponent<Tile>().spriteRenderer.sprite == tile.spriteRenderer.sprite)
        {
            cashFindTiles.Add(hit.collider.gameObject.GetComponent<Tile>());
            hit = Physics2D.Raycast(hit.collider.gameObject.transform.position, dir);
        }
        return cashFindTiles;
    }

    private void DeleteSprite(Tile tile, Vector2[] dirArray)
    {
        List<Tile> cashFindSprite = new List<Tile>();
        for (int i = 0; i < dirArray.Length; i++)
        {
            cashFindSprite.AddRange(findMatch(tile, dirArray[i]));
        }
        if (cashFindSprite.Count >= 2)
        {
            for (int i = 0; i < cashFindSprite.Count; i++)
            {
                cashFindSprite[i].spriteRenderer.sprite = null;
            }
            isFindMatch = true;
        }
    }

    private void FindAllMatch(Tile tile)
    {
        if (tile.isEmpty)
        {
            return;
        }
        DeleteSprite(tile, new Vector2[2] { Vector2.up, Vector2.down });
        DeleteSprite(tile, new Vector2[2] { Vector2.left, Vector2.right });
        if (isFindMatch)
        {
            isFindMatch = false;
            tile.spriteRenderer.sprite = null;
            isSearchEmptyTile = true;
        }
    }

    #endregion

    private void SwapTwoTiles(Tile tile)
    {
        if (oldSelectTile.spriteRenderer.sprite == tile.spriteRenderer.sprite)
        {
            return;
        }
        Sprite cashSprite = oldSelectTile.spriteRenderer.sprite;
        oldSelectTile.spriteRenderer.sprite = tile.spriteRenderer.sprite;
        tile.spriteRenderer.sprite = cashSprite;

        UI.instance.Moves(1);
    }

    public List<Tile> AdjacentTiles()
    {
        List<Tile> cashTiles = new List<Tile>();
        for (int i = 0; i < dirRay.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(oldSelectTile.transform.position, dirRay[i]);
            if (hit.collider != null)
            {
                cashTiles.Add(hit.collider.gameObject.GetComponent<Tile>());
            }
        }
        return cashTiles;
    }


    /// /////////////

    private void SearchEmptyTile()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = ySize - 1; y > -1; y--)
            {
                if (tileArray[x, y].isEmpty) 
                {
                    ShiftTileDown(x, y);
                }
            }
        }
        isSearchEmptyTile = false;

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                FindAllMatch(tileArray[x, y]);
            }
        }

    }
 
    private void ShiftTileDown(int xPos, int yPos)
    {
        isShift = true;
        for (int y = yPos; y < ySize - 1; y++)
        {
            if (!tileArray[xPos, y + 1].isEmpty)
            {
                Tile tile = tileArray[xPos, y];
                tile.spriteRenderer.sprite = tileArray[xPos, y + 1].spriteRenderer.sprite;
            }
        }
        UI.instance.Score(50); ///
        tileArray[xPos, ySize - 1].spriteRenderer.sprite = tileSprite[Random.Range(0, tileSprite.Count)];
        isShift = false;
    }


    /*    private void SearchEmptyTile()
        {
            for (int x = 0; x < xSize; x++)
            {
                for ( int y = 0; y < ySize; y++)
                {
                    if (tileArray[x,y].isEmpty)
                    {
                        ShiftTileDown(x, y);
                        break;
                    }
                    if (x == xSize-1 && y == ySize-1)
                    {
                        isSeachEmptyTile = false;
                    }
                }
            }

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    FindAllMatch(tileArray[x, y]);
                }
            }
        }


        private void ShiftTileDown(int xPos, int yPos)
        {
            isShift = true;
            List<SpriteRenderer> cashRenderer = new List<SpriteRenderer>();
            for (int y = yPos; y< ySize; y++)
            {
                Tile tile = tileArray[xPos, y];
                if (tile.isEmpty)
                {
                    cashRenderer.Add(tile.spriteRenderer);
                }
            }
            SetNewSprite(xPos, cashRenderer);
            isShift = false;
        }

        private void SetNewSprite(int xPos, List<SpriteRenderer> renderer)
        {
            for (int y=0; y< renderer.Count-1; y++)
            {
                renderer[y].sprite = renderer[y + 1].sprite;
                renderer[y + 1].sprite = GetNewSprite(xPos, ySize - 1);
            }
        }


        private Sprite GetNewSprite(int xPos, int yPos)
        {
            List<Sprite> cashSprite = new List<Sprite>();
            cashSprite.AddRange(tileSprite);

            if (xPos > 0)
            {
                cashSprite.Remove(tileArray[xPos - 1, yPos].spriteRenderer.sprite);
            }

            if (xPos < xSize -1)
            {
                cashSprite.Remove(tileArray[xPos + 1, yPos].spriteRenderer.sprite);
            }
            if (xPos>0)   /// error
            {
                cashSprite.Remove(tileArray[xPos, yPos - 1].spriteRenderer.sprite);
            }
            return cashSprite[Random.Range(0, cashSprite.Count)];
        }
    */

}
