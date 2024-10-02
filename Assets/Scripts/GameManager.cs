using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //public GameObject wallGraphic, snakeGraphic, fruitGraphic;
    private List<GameObject> snakeGraphicParts; //lista de partes de snake (cubitos)
    public GameObject panelPerdiste;
    private GameObject laFrutita;
    private bool gotFrutita; //indica que agarró una frutita para agrandar la cola
    public GameObject camarita;
    private bool endOfGame = false;
    private Vector3Int ultimaPosicion;

    public GameObject panelAndroid;

    private Cell[,] matrix;

    private List<Vector3Int> snakePositions;

    private int currentDirection = 0;
    public float speed = 0.2f;

    public GameObject plano;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("Execute", GetSpeed());
        
        //init world map
        matrix = new Cell[40, 40];
        for (int i = 0; i < 40; i++)
        {
            for (int j = 0; j < 40; j++)
            {
                matrix[i, j] = new Cell();
            }
        }

        //init walls
        for (int i = 0; i < 40; i++)
        {
            CreateWall(new Vector3Int(0, 0, i));
            CreateWall(new Vector3Int(40 - 1, 0, i));
        }

        for (int j = 0; j < 40; j++)
        {
            CreateWall(new Vector3Int(j, 0, 0));
            CreateWall(new Vector3Int(j, 0, 40 - 1));
        }
        int numberOfRandomWalls = RandomWallValue();
        for (int i = 0; i < numberOfRandomWalls; i++)
        {
            Vector3Int nuevaPos2 = GetFreePosition();
            CreateWall(nuevaPos2);
        }

        //init snake
        snakePositions = new List<Vector3Int>();
        snakeGraphicParts = new List<GameObject>();
        Vector3Int nuevaPos = GetFreePosition();
        AddNewSnakeGraphicPart(nuevaPos);
        UpdateSnakeMatrix(nuevaPos);

        //init fruit
        Vector3Int nuevaPos3 = GetFreePosition();
        //laFrutita = Instantiate(fruitGraphic, nuevaPos3, transform.rotation) as GameObject;
        laFrutita = GameObject.CreatePrimitive(PrimitiveType.Cube);
        laFrutita.transform.position = nuevaPos3;
        laFrutita.GetComponent<Renderer>().material.color = Color.yellow;
        PlaceFrutitaAt(nuevaPos3.x, nuevaPos3.z);

        EstirarPlano();
    }

    public void EstirarPlano()
    {
        plano.transform.localScale = new Vector3(4, 1, 4);
    }


    // Update is called once per frame
    void Update()
    {
        UpdatePlayer();

        MoveCamera();
    
    }

    private void MoveCamera()
    {
        Vector3Int snakeHead = GetHead();
        snakeHead.z -= 15;
        snakeHead.y =20;
        camarita.transform.position = Vector3.Lerp(camarita.transform.position, snakeHead, 1 * Time.deltaTime);
    }


    private void FreeSnakeTailPosition()
    {
        FreeSnakePartAt(GetTail().x,GetTail().z);
    }



    private void UpdateSnakeHeadPosition(Vector3Int pos)
    {
        UpdateHeadPosition(pos);
        PlaceSnakePartAt(GetHead().x, GetHead().z);
    }

    private void UpdateSnakeGraphicsWhileMoving()
    {
        for (int i = 0; i < snakeGraphicParts.Count; i++)
        {
            snakeGraphicParts[i].transform.position = GetSnakePartPosition(i);
        }
    }

    private void Execute()
    {
        ultimaPosicion = GetTail();
        FreeSnakeTailPosition();
        UpdateSnakePositionsWhileMoving();
        Vector3Int pos = GetHead();

        if (currentDirection==1)
        {
            pos.x += 1;
        }
        else if (currentDirection == 2)
        {
            pos.z += 1;
        }
        else if (currentDirection == 3)
        {
            pos.x -= 1;
        }
        else if (currentDirection == 4)
        {
            pos.z-= 1;
        }

        CheckCollisions();

        UpdateSnakeHeadPosition(pos);
        UpdateSnakeGraphicsWhileMoving();
        

        //si agarró frutita se añade un pedaso más a la cola de la serpiente
        if (gotFrutita)
        {
            gotFrutita = false;
            AgregarElementoGraficoAlFinalDeLaColaDeLaSerpiente();
        }

        //vuelve a llamar a ejecutar 
        if(!endOfGame)Invoke("Execute", GetSpeed());
    }

    private void AgregarElementoGraficoAlFinalDeLaColaDeLaSerpiente()
    {
        //ver que onda la ultima posicion
        UpdateTailPosition(ultimaPosicion);
        //GameObject nuevoCubito = Instantiate(snakeGraphic, ultimaPosicion, transform.rotation);
        GameObject nuevoCubito = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nuevoCubito.transform.position = ultimaPosicion;
        nuevoCubito.GetComponent<Renderer>().material.color = Color.red;

        snakeGraphicParts.Insert(0, nuevoCubito);

        PlaceSnakePartAt(ultimaPosicion.x, ultimaPosicion.z);
    }


   

    private void DestroyFruit()
    {
        int r = (int)laFrutita.transform.position.x;
        int c = (int)laFrutita.transform.position.z;
        FreeFruitAt(r, c);
        Destroy(laFrutita);
    }

    private void CreateNewFruit()
    {
        Vector3Int nuevaPos3 = GetFreePosition();
        //laFrutita = Instantiate(fruitGraphic, nuevaPos3, transform.rotation) as GameObject;
        laFrutita = GameObject.CreatePrimitive(PrimitiveType.Cube);
        laFrutita.transform.position = nuevaPos3;
        laFrutita.GetComponent<Renderer>().material.color = Color.yellow;
        PlaceFrutitaAt(nuevaPos3.x, nuevaPos3.z);
        gotFrutita = true;
    }

    public void CheckCollisions()
    {
        if (CheckFruitCollision(GetHead()))
        {
            DestroyFruit();
            CreateNewFruit();
        }
        if (CheckWallsCollision(GetHead())) //agregar colision con la serpiente
        {
            endOfGame = true;
            panelPerdiste.SetActive(true);
        }
    }

    

    

   

    private void CreateWall(Vector3Int nuevaPos)
    {
        //Instantiate(wallGraphic, nuevaPos, transform.rotation);
        GameObject pared = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pared.transform.position = nuevaPos;
        pared.GetComponent<Renderer>().material.color = Color.black;
        PlaceWallAt(nuevaPos.x, nuevaPos.z);
    }




    private void UpdateSnakeMatrix(Vector3Int nuevaPos)
    {
        AddNewPart(nuevaPos);
        PlaceSnakePartAt(nuevaPos.x, nuevaPos.z);
    }

    private void AddNewSnakeGraphicPart(Vector3Int nuevaPos)
    {
        //GameObject nuevoSnakePart = Instantiate(snakeGraphic, nuevaPos, transform.rotation) as GameObject;
        GameObject nuevoSnakePart = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nuevoSnakePart.transform.position = nuevaPos;
        nuevoSnakePart.GetComponent<Renderer>().material.color = Color.red;
        snakeGraphicParts.Add(nuevoSnakePart);
    }

    


    public void Reiniciar()
    {
        SceneManager.LoadScene(0);
    }


    public void PressedSpaceUp()
    {
        SetSpeed(0.05f);
    }

    public void releasedSpaceUp()
    {
        SetSpeed(0.2f);
    }




    public void PlaceSnakePartAt(int r, int c)
    {
        matrix[r, c].Snake = (new SnakeEntity());
        //matrixSnake[r, c] = 1;
    }

    public void PlaceWallAt(int r, int c)
    {
        matrix[r, c].Wall = (new WallEntity());
        //matrixWalls[r, c] = 1;
    }

    public void PlaceFrutitaAt(int r, int c)
    {
        matrix[r, c].Fruit = (new FruitEntity());
        //matrixFrutitas[r, c] =  1;
    }




    public bool IsWallAt(int r, int c)
    {
        return matrix[r, c].HasWall();
    }

    public bool IsFruitAt(int r, int c)
    {
        return matrix[r, c].HasFruit();
    }

    public bool IsSnakeAt(int r, int c)
    {
        return matrix[r, c].HasSnake();
    }





    public void FreeSnakePartAt(int r, int c)
    {
        matrix[r, c].Snake = null;
    }

    public void FreeFruitAt(int r, int c)
    {
        matrix[r, c].Fruit = null;
    }





    public int RandomWallValue()
    {
        return Random.Range(50, 50);
    }




    public Vector3Int GetFreePosition()
    {
        bool isAvailable = false;
        Vector3Int newFreePosition = new Vector3Int(0, 0, 0);
        while (!isAvailable)
        {
            newFreePosition = new Vector3Int(Random.Range(1, 40), 0, Random.Range(1, 40));
            isAvailable = IsFreeSurroundings(newFreePosition);
        }
        return newFreePosition;
    }

    public bool IsFreeSurroundings(Vector3Int pos)
    {
        bool isAvailable = true;
        for (int i = pos.x - 1; i < pos.x + 1 && isAvailable; i++)
        {
            for (int j = pos.z - 1; j < pos.z + 1 && isAvailable; j++)
            {
                isAvailable = !IsWallAt(i, j);
            }
        }
        return isAvailable;
    }

    public bool CheckWallsCollision(Vector3Int snakeHead)
    {
        return IsWallAt(snakeHead.x, snakeHead.z);
    }

    public bool CheckFruitCollision(Vector3Int snakeHead)
    {
        return IsFruitAt(snakeHead.x, snakeHead.z);
    }

    public bool CheckSnakeCollision(Vector3Int snakeHead)
    {
        return IsSnakeAt(snakeHead.x, snakeHead.z);
    }


    ///////////////////////////////////////////////
    ///Player input stuff
    ///////////////////////////////////////////////
    ///
    public void UpdatePlayer()
    {
        //esto quedaría super lindo
        //hacer un patrón state y tal vez un command
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (currentDirection != 3) currentDirection = 1;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            if (currentDirection != 4) currentDirection = 2;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (currentDirection != 1) currentDirection = 3;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (currentDirection != 2) currentDirection = 4;
        }
        //mientras se presiona el shift izquierdo, se incrementa la velocidad
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed = 0.05f;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed = 0.2f;
        }
    }
    public float GetSpeed()
    {
        return speed;
    }
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }


    ///////////////////////////////////////////////
    ///Snake stuff
    ///////////////////////////////////////////////

    public Vector3Int GetHead()
    {
        return snakePositions[snakePositions.Count - 1];
    }

    public Vector3Int GetTail()
    {
        return snakePositions[0];
    }

    public int GetLength()
    {
        return snakePositions.Count;
    }

    public void UpdateSnakePositionsWhileMoving()
    {
        for (int i = 0; i < snakePositions.Count - 1; i++)
        {
            snakePositions[i] = snakePositions[i + 1];
        }
    }

    public void UpdateHeadPosition(Vector3Int newPosition)
    {
        snakePositions[snakePositions.Count - 1] = newPosition;
    }

    public void UpdateTailPosition(Vector3Int newPosition)
    {
        snakePositions.Insert(0, newPosition);
    }

    public Vector3Int GetSnakePartPosition(int i)
    {
        return snakePositions[i];
    }

    public void AddNewPart(Vector3Int nuevaPos)
    {
        snakePositions.Add(nuevaPos);
    }
}
