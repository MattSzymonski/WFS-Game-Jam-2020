using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MightyGamePack;
using NaughtyAttributes;


public class MainGameManager : MightyGameManager, IMainGameManager
{
    [HideInInspector] public static MainGameManager mainGameManager;

    GameObject player1;
    GameObject player2;
    public bool player1Dead = false;
    public bool player2Dead = false;

    public DuelCamera duelCamera;
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    public GameObject spawnerPlayer1;
    public GameObject spawnerPlayer2;
    public float spawnerRadius = 1;

    public List<Creep> creeps;
    public GameObject creepPrefab;
    public MightyGamePack.MightyTimer spawnCreepsTimer;

    public Vector4 mapBorders;



    public int creepMaxCount = 30;
    public float minDistanceFromPlayer = 10f;
    private List<Transform> playerPositions;
    public float creepSpawnInterval = 1f;
    public int initialCreepsNumber = 10;

    public void Awake()
    {
        InitializeMighty(this); // Initialize mighty pack
        mainGameManager = this;
    }

    public void Start()
    {
        Vector3 position;

        position = spawnerPlayer1.transform.position;
        GameObject player1 = Instantiate(player1Prefab, position, Quaternion.identity) as GameObject;
        player1.name = "PlayerOne";
        player1.GetComponent<Player>().controllerNumber = 0;
        duelCamera.player1 = player1;
        player1.GetComponent<Player>().mgm = this;
        this.player1 = player1;

        position = spawnerPlayer2.transform.position;
        GameObject player2 = Instantiate(player2Prefab, position, Quaternion.identity) as GameObject;
        player2.name = "PlayerTwo";
        player2.GetComponent<Player>().controllerNumber = 1;
        duelCamera.player2 = player2;
        player2.GetComponent<Player>().mgm = this;
        this.player2 = player2;

        spawnCreepsTimer = this.timersManager.CreateTimer("SpawnCreepsTimer", creepSpawnInterval, 1f, false, false);
    }

    public void Update()
    {
        if (gameState == GameState.Playing)
        {
            SpawnCreeps();
        }
    }

    //------------------------------------------------ GAME STATE FUNCTIONS ---------------------------------------------------- (CODE IN THESE FUNCTIONS WILL AUTOMATICALY BE EXECUTED ON GAME STATE CHANGES)
    public void PlayGame()
    {
        Debug.Log("Play");
        SpawnCreepsInitial();
        audioManager.PlaySound("GameStart");
    }

    public void GameOver(int winner)
    {
       

        if (!debugHideUI && gameState == GameState.Playing)
        {
            UIManager.GameOver();

            if (winner == 1)
            {
                UIManager.SetGameResult("RED CREW WINS!");
            }
            if (winner == 2)
            {
                UIManager.SetGameResult("BLUE CREW WINS!");
            }
            if (winner == 2)
            {
                UIManager.SetGameResult("SPLIT!");
            }
        }
    }

    public void PauseGame() { }

    public void UnpauseGame() { }

    public void BackToMainMenu() { }

    public void RestartGame()
    {


        GameObject ccc = GameObject.Find("Creeps");

        int ttt = GameObject.Find("Creeps").transform.childCount;
        for (int i = ttt - 1; i >= 0; --i)
        {
            Destroy(ccc.transform.GetChild(i));
        }


        //for (int i = creeps.Count - 1; i >= 0; --i)
        //{
        //    if (creeps[i] != null)
        //    {
        //        Destroy(creeps[i].gameObject);
        //    }
        //    creeps.RemoveAt(i);
        //}

        creeps.Clear();

        player1Dead = false;
        player2Dead = false;

        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.name == "PlayerOne")
            {
                Vector2 point = Random.insideUnitCircle * 1;
                player.transform.position = spawnerPlayer1.transform.position + MightyGamePack.MightyUtilites.Vec2ToVec3(point);
                player.transform.rotation = new Quaternion(spawnerPlayer1.transform.rotation.x, spawnerPlayer1.transform.rotation.y, spawnerPlayer1.transform.rotation.z, spawnerPlayer1.transform.rotation.w);
            }
            else if (player.name == "PlayerTwo")
            {
                Vector2 point = Random.insideUnitCircle * 1;
                player.transform.position = spawnerPlayer2.transform.position + MightyGamePack.MightyUtilites.Vec2ToVec3(point);
                player.transform.rotation = new Quaternion(spawnerPlayer2.transform.rotation.x, spawnerPlayer2.transform.rotation.y, spawnerPlayer2.transform.rotation.z, spawnerPlayer2.transform.rotation.w);
            }
        }

        UIManager.SetGameResult("");
    }

    public void OpenOptions() { }

    public void QuitGame()
    {
        #if UNITY_EDITOR
                Debug.Log("Cannot quit game in editor");
        #else
                Application.Quit();      
        #endif
    }

    //--------------------------------------------------- OTHER FUNCTIONS ------------------------------------------------------


    void SpawnCreepsInitial()
    {
        for (int i = 0; i < initialCreepsNumber; i++)
        {
            bool isOk = false;
            Vector2 position = Vector2.zero;
            while (!isOk)
            {
                position = new Vector2(Random.Range(mapBorders.x, mapBorders.y), Random.Range(mapBorders.z, mapBorders.w));
                if (Vector3.Distance(new Vector3(position.x, 0f, position.y), spawnerPlayer1.transform.position) > minDistanceFromPlayer && Vector3.Distance(new Vector3(position.x, 0f, position.y), spawnerPlayer2.transform.position) > minDistanceFromPlayer)
                    isOk = true;
            }
            GameObject newCreep = Instantiate(creepPrefab, new Vector3(position.x, 0.0f, position.y), Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f)) as GameObject;
            particleEffectsManager.SpawnParticleEffect(newCreep.transform.position, Quaternion.identity, 3, 0.0f, "SpawnNewCar");
            newCreep.transform.parent = GameObject.Find("Creeps").transform;
            newCreep.name = "Creep_" + creeps.Count.ToString();
            creeps.Add(newCreep.GetComponent<Creep>());
        }
    }


    void SpawnCreeps()
    { 
       
        //myTimer.PlayTimer();

        if (spawnCreepsTimer.finished)
        {
            if (creeps.Count < creepMaxCount)
            {
                bool isOk = false;
                Vector2 position = Vector2.zero;
                while (!isOk)
                {
                    position = new Vector2(Random.Range(mapBorders.x, mapBorders.y), Random.Range(mapBorders.z, mapBorders.w));
                    if (Vector3.Distance(new Vector3(position.x, 0f, position.y), player1.transform.position) > minDistanceFromPlayer && Vector3.Distance(new Vector3(position.x, 0f, position.y), player2.transform.position) > minDistanceFromPlayer)
                        isOk = true;
                }
                GameObject newCreep = Instantiate(creepPrefab, new Vector3(position.x, 0.0f, position.y), Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f)) as GameObject;
                particleEffectsManager.SpawnParticleEffect(newCreep.transform.position, Quaternion.identity, 3, 0.0f, "SpawnNewCar");
                newCreep.transform.parent = GameObject.Find("Creeps").transform;
                newCreep.name = "Creep_" + creeps.Count.ToString();
                creeps.Add(newCreep.GetComponent<Creep>());

                
                spawnCreepsTimer.RestartTimer();
            }
          
        } 
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float width = Mathf.Abs(mapBorders.x) + Mathf.Abs(mapBorders.y);
        float height = Mathf.Abs(mapBorders.z) + Mathf.Abs(mapBorders.w);
        Gizmos.DrawWireCube(new Vector3((mapBorders.x + mapBorders.y)/2, 0, (mapBorders.z + mapBorders.w) / 2), new Vector3(width,3,height));
    }


}
