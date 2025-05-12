using UnityEngine;
using System.Collections.Generic;

public class GameBoard : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int boardWidth = 8;
    [SerializeField] private int boardHeight = 8;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float cellSize = 1f;

    [Header("Visual Settings")]
    [SerializeField] private Material normalCellMaterial;
    [SerializeField] private Material highlightedCellMaterial;
    [SerializeField] private Material selectedCellMaterial;
    [SerializeField] private Material waterCellMaterial;

    private Cell[,] cells;
    private Animal selectedAnimal;
    private List<Vector2Int> highlightedCells;

    private void Start()
    {
        InitializeBoard();
        highlightedCells = new List<Vector2Int>();
    }

    private void InitializeBoard()
    {
        cells = new Cell[boardWidth, boardHeight];
        
        // Create board cells
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                CreateCell(x, y);
            }
        }

        // Set up special cells (water areas, etc.)
        SetupSpecialCells();
    }

    private void CreateCell(int x, int y)
    {
        Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);
        GameObject cellObj = Instantiate(cellPrefab, position, Quaternion.identity, transform);
        cellObj.name = $"Cell_{x}_{y}";

        Cell cell = cellObj.AddComponent<Cell>();
        cell.Initialize(new Vector2Int(x, y), normalCellMaterial);
        cells[x, y] = cell;
    }

    private void SetupSpecialCells()
    {
        // Define water areas
        Vector2Int[] waterCells = new Vector2Int[]
        {
            new Vector2Int(3, 3), new Vector2Int(3, 4),
            new Vector2Int(4, 3), new Vector2Int(4, 4)
        };

        foreach (Vector2Int pos in waterCells)
        {
            if (IsValidPosition(pos))
            {
                cells[pos.x, pos.y].SetType(CellType.Water);
                cells[pos.x, pos.y].SetMaterial(waterCellMaterial);
            }
        }
    }

    public void SelectAnimal(Animal animal)
    {
        ClearHighlights();
        selectedAnimal = animal;

        if (animal != null)
        {
            Vector2Int pos = GetAnimalPosition(animal);
            HighlightValidMoves(pos);
            cells[pos.x, pos.y].SetMaterial(selectedCellMaterial);
        }
    }

    private void HighlightValidMoves(Vector2Int pos)
    {
        List<Vector2Int> validMoves = GetValidMoves(pos);
        foreach (Vector2Int move in validMoves)
        {
            cells[move.x, move.y].SetMaterial(highlightedCellMaterial);
            highlightedCells.Add(move);
        }
    }

    private List<Vector2Int> GetValidMoves(Vector2Int pos)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        Animal animal = cells[pos.x, pos.y].occupiedBy;

        if (animal == null) return moves;

        // Get moves based on animal type and position
        switch (animal.type)
        {
            case AnimalType.Tiger:
                AddTigerMoves(pos, moves);
                break;
            case AnimalType.Mouse:
                AddMouseMoves(pos, moves);
                break;
            // Add cases for other animals
            default:
                AddDefaultMoves(pos, moves, animal.moveRange);
                break;
        }

        return moves;
    }

    private void AddTigerMoves(Vector2Int pos, List<Vector2Int> moves)
    {
        // Tigers can jump over water
        int[] directions = new int[] { -1, 1 };
        foreach (int dx in directions)
        {
            foreach (int dy in directions)
            {
                Vector2Int jumpPos = new Vector2Int(pos.x + dx * 2, pos.y + dy * 2);
                if (IsValidPosition(jumpPos) && CanMoveTo(jumpPos))
                {
                    moves.Add(jumpPos);
                }
            }
        }
    }

    private void AddMouseMoves(Vector2Int pos, List<Vector2Int> moves)
    {
        // Mice can move through water
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Vector2Int newPos = new Vector2Int(pos.x + dx, pos.y + dy);
                if (IsValidPosition(newPos))
                {
                    moves.Add(newPos);
                }
            }
        }
    }

    private void AddDefaultMoves(Vector2Int pos, List<Vector2Int> moves, int range)
    {
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Vector2Int newPos = new Vector2Int(pos.x + dx, pos.y + dy);
                if (IsValidPosition(newPos) && CanMoveTo(newPos))
                {
                    moves.Add(newPos);
                }
            }
        }
    }

    public bool TryMoveAnimal(Vector2Int from, Vector2Int to)
    {
        if (!IsValidPosition(from) || !IsValidPosition(to)) return false;

        Cell fromCell = cells[from.x, from.y];
        Cell toCell = cells[to.x, to.y];

        if (fromCell.occupiedBy == null) return false;
        if (!CanMoveTo(to)) return false;

        // Handle combat if destination is occupied
        if (toCell.occupiedBy != null)
        {
            HandleCombat(fromCell.occupiedBy, toCell.occupiedBy);
            return true;
        }

        // Move animal
        MoveAnimal(fromCell.occupiedBy, to);
        return true;
    }

    private void HandleCombat(Animal attacker, Animal defender)
    {
        // Implement combat logic
        Debug.Log($"Combat between {attacker.animalName} and {defender.animalName}");
    }

    private void MoveAnimal(Animal animal, Vector2Int to)
    {
        Vector2Int from = GetAnimalPosition(animal);
        cells[from.x, from.y].occupiedBy = null;
        cells[to.x, to.y].occupiedBy = animal;

        // Update animal position
        Vector3 worldPos = new Vector3(to.x * cellSize, 0, to.y * cellSize);
        animal.transform.position = worldPos;
    }

    private Vector2Int GetAnimalPosition(Animal animal)
    {
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if (cells[x, y].occupiedBy == animal)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < boardWidth && 
               pos.y >= 0 && pos.y < boardHeight;
    }

    private bool CanMoveTo(Vector2Int pos)
    {
        Cell cell = cells[pos.x, pos.y];
        return cell.isWalkable && 
               (cell.occupiedBy == null || cell.occupiedBy.gameObject.CompareTag("Enemy"));
    }

    private void ClearHighlights()
    {
        foreach (Vector2Int pos in highlightedCells)
        {
            cells[pos.x, pos.y].SetMaterial(normalCellMaterial);
        }
        highlightedCells.Clear();
    }
}

public class Cell : MonoBehaviour
{
    public Vector2Int position;
    public Animal occupiedBy;
    public bool isWalkable = true;
    public CellType type = CellType.Normal;

    private MeshRenderer meshRenderer;

    public void Initialize(Vector2Int pos, Material defaultMaterial)
    {
        position = pos;
        meshRenderer = GetComponent<MeshRenderer>();
        SetMaterial(defaultMaterial);
    }

    public void SetMaterial(Material material)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material = material;
        }
    }

    public void SetType(CellType newType)
    {
        type = newType;
        isWalkable = type != CellType.Obstacle;
    }
}

public enum CellType
{
    Normal,
    Water,
    Obstacle
}
