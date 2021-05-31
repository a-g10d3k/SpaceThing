using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public bool Dead { get { return _dead; } }
    private bool _dead = false;
    public int Team = 0;
    public readonly List<IShipSystem> ShipSystems = new List<IShipSystem>();  
    public GameObject AimAssistTarget;
    public bool Controllable = false;
    public GameObject UIShipSignature;
    public GameObject UIMoveCommand;
    public GameObject UISelectUnit;
    private string _shipsDirectory = "Assets/Resources/ShipGrids/";
    public string ShipFile;
    public GameObject[,] Grid;
    public float GridPixelScale;
    public GameObject Pixel;
    public float Thrust;
    public int CpuUsage;
    public int MaxCpu;
    public float PowerGeneration;
    public float PowerUsage;
    public float MaxSpeed;
    private Vector3 _targetPos;
    public readonly List<Gun> Guns = new List<Gun>();
    public float Mass;
    public bool Stop { get { return _stop; } set { _stop = value; if (!value) { OnStopCommand(); } } }
    private bool _stop;
    void Start()
    {
        InitializeShipGrid();
        InitializeUISignature();
        InitializeUI();
        MapInfo.main.Ships.Add(this);
    }

    void InitializeShipGrid()//This spawns in the individual pixels of the ship.
    {
        int[,] grid = LoadGrid(ShipFile);
        if (grid == null) { return; }
        Grid = new GameObject[grid.GetLength(0), grid.GetLength(1)];
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                if(grid[x,y] == 0) { continue; }//dont spawn pixels of type 0
                var pixel = Instantiate(Pixel);
                pixel.GetComponent<ShipPixel>().Ship = this;
                pixel.transform.SetParent(gameObject.transform);
                pixel.transform.localScale = new Vector3(GridPixelScale, GridPixelScale, GridPixelScale);
                float xPos = (0.5f - grid.GetLength(0) / 2 + x) * GridPixelScale;
                float yPos = (0.5f - grid.GetLength(1) / 2 + y) * GridPixelScale;
                pixel.transform.localPosition = new Vector2(xPos, yPos);
                pixel.GetComponent<ShipPixel>().SetPixelType(grid[x, y]);
                Mass += pixel.GetComponent<ShipPixel>().Mass;
                pixel.GetComponent<ShipPixel>().GridPosition = new Vector2Int(x, y);
                Grid[x, y] = pixel;
            }
        }
    }

    void InitializeUISignature()//This spawns in the ship signature ui gameobject
    {
        if (Controllable) { return; }
        UIShipSignature = Instantiate(Resources.Load("Prefabs/UI-ship-signature")) as GameObject;
        switch (Team)
        {
            case 0:
                UIShipSignature.GetComponent<TextMesh>().color = Color.green;
                break;
            case 1:
                UIShipSignature.GetComponent<TextMesh>().color = Color.red;
                break;
        }
        UIShipSignature.GetComponent<TextMesh>().text = ShipFile;
    }

    void InitializeUI()
    {
        if (!Controllable) { return; }
        UIMoveCommand = Instantiate(Resources.Load("Prefabs/UI-move-command")) as GameObject;
        UISelectUnit = Instantiate(Resources.Load("Prefabs/UI-select-unit")) as GameObject;
    }

    int[,] LoadGrid(string src)//This reads a ship file and returns a 2D array of the ship pixel types.
    {
        src = _shipsDirectory + src + ".txt";
        var reader = new System.IO.StreamReader(src, true);
        if(!int.TryParse(reader.ReadLine(),out int xGridDimension) || !int.TryParse(reader.ReadLine(), out int yGridDimension))
        {
            Debug.Log("Error parsing grid dimensions. Path:\"" + src + "\"");
            return null;
        }
        int[,] grid = new int[xGridDimension, yGridDimension];
        string[] cellTypeStrings = reader.ReadLine().Split(' ');
        int i = 0;
        for (int y = 0; y < yGridDimension; y++)
        {
            for (int x = 0; x < xGridDimension; x++)
            {
                try
                {
                    grid[x, y] = int.Parse(cellTypeStrings[i]);
                    i++;
                }
                catch (System.Exception ex)
                {
                    Debug.Log("Error loading cells from grid \"" + src + "\": " + ex.Message);
                    return null;
                }
            }
        }
        return grid;
    }

    private void OnStopCommand()
    {
        if (UIMoveCommand != null)
        {
            UIMoveCommand.SetActive(false);
        }
    }

    void UpdateTargetPosition()//This updates the target to which the ship will fly
    {
        if (Input.GetMouseButtonDown(1) && Controllable)
        {
            _targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _targetPos.z = 0;
            if (UIMoveCommand != null)
            {
                UIMoveCommand.SetActive(true);
                UIMoveCommand.transform.position = _targetPos;
            }
        }
    }

    void UpdateUISelectUnitPosition()//disable the UI-select-unit gameobject if no arguments are passed
    {
        if (UISelectUnit == null) { return; }
        UISelectUnit.SetActive(false);
    }

    void UpdateUISelectUnitPosition(Vector3 pos)//Update the position of the UI-select-unit gameobject
    {
        if (UISelectUnit == null) { return; }
        UISelectUnit.SetActive(true);
        UISelectUnit.transform.position = pos;
    }

    void UpdateAimAssistTarget()
    {
        if (Input.GetMouseButtonDown(0) && Controllable)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] ships = Physics2D.OverlapCircleAll(mousePos, 0.5f * Camera.main.orthographicSize/5, 2048);
            if (ships.Length > 0)
            {
                AimAssistTarget = ships[0].gameObject;
            }
        }
    }

    void UpdateGunsTarget()//This updates the position at which the ship will shoot, and whether the guns should hold fire or not
    {
        if (AimAssistTarget != null)
        {
            UpdateUISelectUnitPosition(AimAssistTarget.transform.position);
            foreach (Gun gun in Guns)
            {
                Vector4 target = gun.CalculateAimAssist(AimAssistTarget);
                gun.SetTarget(target);
                if (target.w <= gun.ProjectileLife)
                {
                    gun.HoldFire = false;
                }
                else
                {
                    gun.HoldFire = true;
                }
            }
        }
        else
        {
            UpdateUISelectUnitPosition();
            foreach (Gun gun in Guns)
            {
                gun.HoldFire = true;
            }
        }
    }

    public void SetTargetPos(Vector3 pos)
    {
        Stop = false;
        _targetPos = pos;
    }

    private void Accelerate()
    {
        var rb = gameObject.GetComponent<Rigidbody2D>();
        if (Mass == 0) { return; }
        float acceleration = Thrust / Mass;
        if (Stop)
        {
            Vector2 directionVector = -rb.velocity.normalized;
            Vector2 acc = directionVector * acceleration * Time.deltaTime;
            if (rb.velocity.magnitude <= acceleration * Time.deltaTime)
            {
                rb.velocity = new Vector2(0,0);
                return;
            }
            rb.velocity += acc;
        }
        else
        {
            Vector2 directionVector = (_targetPos - transform.position).normalized;
            Vector2 acc = ((directionVector * MaxSpeed) - rb.velocity).normalized * acceleration * Time.deltaTime;
            rb.velocity += acc;
            if (rb.velocity.magnitude >= MaxSpeed)
            {
                rb.velocity = rb.velocity.normalized * (MaxSpeed + 0.001f);
                return;
            }
        }
    }

    private void Rotate()
    {
        if (!Stop)
        {
            var rb = gameObject.GetComponent<Rigidbody2D>();
            transform.up = rb.velocity;
        }
    }

    private void SetUISignaturePosition()//this positions the ship signature ui gameobject
    {
        if (UIShipSignature == null) { return; }
        Vector3 pos = transform.position;
        float yOffset = Grid.GetLength(1) * GridPixelScale;
        pos.y -= yOffset;
        pos.z = -1;
        UIShipSignature.transform.position = pos;
    }

    void Move()//This handles the ships movement
    {
        UpdateTargetPosition();
        Accelerate();
        Rotate();
        SetUISignaturePosition();
    }

    public void OnDestroyCommandModule()
    {
        Destroy(UIShipSignature);
        foreach(IShipSystem shipSystem in ShipSystems)
        {
            if (shipSystem as Object != null)
            {
                shipSystem.SetActive(false);
            }
        }
        _dead = true;
    }

    private void UpdatePower()//Shuts down ship systems if there's not enough power to power them all, and destroys the ship if there is no power generation
    {
        for (int i = 0; i < ShipSystems.Count && PowerGeneration < PowerUsage; i++)
        {
            if (!(ShipSystems[i] is PowerGenerator) && ShipSystems[i] as Object != null) //The cast to Object is required here because comparing an interface to null doesnt overload the operator correctly in unity.
            {
                ShipSystems[i].SetActive(false);
            }
        }
        if (PowerGeneration <= 0)
        {
            OnDestroyCommandModule();
        }
    }

    void Update()
    {
        Move();
        UpdateAimAssistTarget();
        UpdatePower();
    }
    private void FixedUpdate()
    {
        UpdateGunsTarget();
    }
}
