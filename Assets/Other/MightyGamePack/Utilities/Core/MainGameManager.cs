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

    public List<Object> otherCars;
    public GameObject otherCarsPrefab;
    public MightyGamePack.MightyTimer spawnOtherCarsTimer;

    public Vector4 mapBorders;

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
        //player1.GetComponent<PlayerMovement>().playerNumber = 1;
        duelCamera.player1 = player1;

        position = spawnerPlayer2.transform.position;
        GameObject player2 = Instantiate(player2Prefab, position, Quaternion.identity) as GameObject;
        player2.name = "PlayerTwo";
        //player2.GetComponent<PlayerMovement>().playerNumber = 2;
        duelCamera.player2 = player2;
    }

    public void Update()
    {
        if (gameState == GameState.Playing)
        {
            SpawnOtherCars();
        }
    }

    //------------------------------------------------ GAME STATE FUNCTIONS ---------------------------------------------------- (CODE IN THESE FUNCTIONS WILL AUTOMATICALY BE EXECUTED ON GAME STATE CHANGES)
    public void PlayGame() { Debug.Log("Play"); }

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

        }
    }

    public void PauseGame() { }

    public void UnpauseGame() { }

    public void BackToMainMenu() { }

    public void RestartGame()
    {
        for (int i = otherCars.Count - 1; i >= 0; --i)
        {
            if (otherCars[i])
            {
               // Destroy(otherCars[i].gameObject);
            }
            otherCars.RemoveAt(i);
        }
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


    void SpawnOtherCars()
    { 
        spawnOtherCarsTimer = this.timersManager.CreateTimer("SpawnOtherCarsTimer", 1f, 1f, false, false);
        //myTimer.PlayTimer();

        if (spawnOtherCarsTimer.finished)
        {
            Vector2 position = new Vector2(Random.Range(mapBorders.x, mapBorders.y), Random.Range(mapBorders.z, mapBorders.w));

            GameObject newOtherCar = Instantiate(otherCarsPrefab, new Vector3(position.x, 0, position.y), Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f)) as GameObject;
            newOtherCar.name = "OtherCar" + otherCars.Count.ToString();
            //otherCars.GetComponent<otherCar>().owner = 1;
            newOtherCar.transform.parent = GameObject.Find("OtherCars").transform;
            //otherCars.Add(otherCars.GetComponent<OtherCars>());

            particleEffectsManager.SpawnParticleEffect(newOtherCar.transform.position, Quaternion.identity, 3, 0.0f, "SpawnNewCar");




            spawnOtherCarsTimer.RestartTimer();
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
