using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Ray Settings")]
    [SerializeField] private LayerMask boardLayer;
    [SerializeField] private LayerMask animalLayer;
    [SerializeField] private float maxRayDistance = 100f;

    [Header("Input Settings")]
    [SerializeField] private float doubleTapTime = 0.2f;
    [SerializeField] private float dragThreshold = 0.1f;

    private Camera mainCamera;
    private Vector2 lastTapPosition;
    private float lastTapTime;
    private bool isDragging;
    private Vector3 dragStartPosition;

    // Events
    public event Action<Vector2Int> OnCellClicked;
    public event Action<Animal> OnAnimalClicked;
    public event Action<Vector2> OnDragStart;
    public event Action<Vector2> OnDragUpdate;
    public event Action<Vector2> OnDragEnd;
    public event Action OnDoubleTap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }
    }

    private void Update()
    {
        // Don't process input if over UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        HandleMouseInput();
        HandleTouchInput();
    }

    private void HandleMouseInput()
    {
        if (!Application.isMobilePlatform)
        {
            // Left mouse button
            if (Input.GetMouseButtonDown(0))
            {
                HandleInputDown(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                HandleInputHold(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                HandleInputUp(Input.mousePosition);
            }

            // Right mouse button (cancel/context menu)
            if (Input.GetMouseButtonDown(1))
            {
                HandleRightClick();
            }
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleInputDown(touch.position);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    HandleInputHold(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    HandleInputUp(touch.position);
                    break;
            }
        }
    }

    private void HandleInputDown(Vector2 screenPosition)
    {
        // Check for double tap
        if (Time.time - lastTapTime < doubleTapTime &&
            Vector2.Distance(screenPosition, lastTapPosition) < dragThreshold)
        {
            OnDoubleTap?.Invoke();
            lastTapTime = 0; // Reset to prevent triple tap
            return;
        }

        lastTapPosition = screenPosition;
        lastTapTime = Time.time;
        dragStartPosition = screenPosition;
        isDragging = false;

        // Raycast to check what was clicked
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRayDistance, animalLayer))
        {
            // Animal was clicked
            Animal animal = hit.collider.GetComponent<Animal>();
            if (animal != null)
            {
                OnAnimalClicked?.Invoke(animal);
            }
        }
        else if (Physics.Raycast(ray, out hit, maxRayDistance, boardLayer))
        {
            // Board cell was clicked
            Vector2Int cellPosition = GetCellPosition(hit.point);
            OnCellClicked?.Invoke(cellPosition);
        }
    }

    private void HandleInputHold(Vector2 screenPosition)
    {
        if (!isDragging)
        {
            if (Vector2.Distance(screenPosition, dragStartPosition) > dragThreshold)
            {
                isDragging = true;
                OnDragStart?.Invoke(screenPosition);
            }
        }
        else
        {
            OnDragUpdate?.Invoke(screenPosition);
        }
    }

    private void HandleInputUp(Vector2 screenPosition)
    {
        if (isDragging)
        {
            OnDragEnd?.Invoke(screenPosition);
            isDragging = false;
        }
    }

    private void HandleRightClick()
    {
        // Handle right-click actions (e.g., cancel selection, show context menu)
        GameManager.Instance.CancelSelection();
    }

    private Vector2Int GetCellPosition(Vector3 worldPosition)
    {
        // Convert world position to grid position
        float cellSize = GameConfig.Instance.cellSize;
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.z / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GetWorldPosition(Vector2Int cellPosition)
    {
        float cellSize = GameConfig.Instance.cellSize;
        return new Vector3(cellPosition.x * cellSize, 0, cellPosition.y * cellSize);
    }

    public Ray GetMouseRay()
    {
        return mainCamera.ScreenPointToRay(Input.mousePosition);
    }

    public Vector2 GetMousePosition()
    {
        return Input.mousePosition;
    }

    public bool IsOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void SetEnabled(bool enabled)
    {
        enabled = enabled;
        if (!enabled)
        {
            // Reset all input states
            isDragging = false;
            lastTapTime = 0;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
