/* 
Mighty Game Jam Pack
Copyright (C) Mateusz Szymonski 2020 - All Rights Reserved
Written by Mateusz Szymonski <matt.szymonski@gmail.com>

NAME:
    Mighty Game Manager

DEPENDENCIES: 
    https://github.com/dbrizov/NaughtyAttributes

CONTENTS:
    - MainGameManager (Place where game developer write main logic of the game and what should be done when game changes its states  (Play, restart, game over, pause, open options, quit functions)
    - MightyGameManager (Game state control, some different settings, cannot be used as component! Use MainGameManager instead)
    - MightyUIManager (UI handling, screen blink effect, transition between menus, custom cursor, game jam logo displaying)
    - MightyAudioManager (Handling audio volume, playing sounds (also random from provided list))
    - MightyParticleEffectsManager (Spawning particles, destroying finished ones)
    - MightyTimersManager (Creates, updates and stores timers)
    - MightyUtilities (A bunch of helpful utility functions)

    - FloatingText (Spawning floating texts eg. damage point popup)
    - TextTyper (Prints text to UI text components letter by letter over time)
    - DuelCamera (Camera that keeps center beteen two players)
    - TransformJuicer (Animates transform of object using curves, no need to use animator and animations)
    - FlashJuicer (Animates material of object eg. on hit)
    - CameraShaker (Provides excellent camera shakes. Copyright (c) 2019 Road Turtle Games, MIT License, https://github.com/andersonaddo/EZ-Camera-Shake-Unity)
    - UltimateSmoothCamera (Camera with advanced interia effects)
    - AnimatorFunctions (Triggering various functions, effects, sounds, etc from animations)
    - CameraRTS (?)
    - CameraFollow2D (Smoothly follows target and gently twists towards moving direction)


TODO:
    - PersonalAudioManager (component holding pool of audiosources and pool of sounds and uses them to play requested sounds)
    - PersonalStateManager (component handling state and animations changes (eg. enemy: idle, pursuing, fighting))
    - Knockback (?)
    - SceneSwitcher
	- Parallax background
	- Magnet (pulling objects toward other objects, eg. coins, experience orbs, etc)
	- Raycast (with easy layers setting support https://www.youtube.com/watch?v=uDYE3RFMNzk)

SETUP:
    All these elements need to be properly set.
    There is a lot of dependencies in Unity project (UI Menus, Sound mixers, Animations, Input)
    All these need to be set in only one way. If you don't want to get cancer while setting this up just use template project (modify it of course by adding your own game mechanics)
    Modifications to the UI structure are of course allowed but can break things so make them wisely.

    Warning! Mighty Game Jam Pack systems use special input settings.
    Please close Unity and replace data in InputManager.asset in projectSettings directory with data from MightyInputManager.txt file provided with the pack or use template project.
    If you want to use your own input settings you need to replace keys/buttons/axes names in several places in code but better don't do that.
    To add another gamepad support just duplicate input settings for gamepad, change joystick index and rename 1 to 2, 3, 4, etc (use notepad to do this)
    Changes made via notepad will be visible after refresing player settings view or restarting Unity

HOW TO USE IT:
    When developing game just use GAMECONTROLLING FUNCTIONS in MightyGameManager. 
    These GAMECONTROLLING FUNCTIONS are called when pressing buttons in UI.
    You don't need to care about coding UI at all.
   

    There is GameState variable that forms the game loop so you don't need to care about it too.
    In typical cases the main gameplay mechanics should work only in "Playing" state.
 
    To hide whole UI on start for faster development use debugHideUI variable

    Options menu allows for setting the sound volume with sliders. 
    Again, this works only when sound mixers are created and properly set.

    While coding anything refer to all other managers (Audio, Particle effects, etc) via MightyGameManager
    The easiest way to do this is by "MightyGamePack.MightyGameManager.gameManager.particleEffectsManager"

    If you don't know what some weird named parameters do, just hover cursor over them, there are tooltips implemented!

    When developing a game, MightyGameManager is only file that should be modified (mostly add game-specific code)
    Other files/classes should remain untouched! Don't modify them unless you know what you are doing.


MODIFICATIONS:
    If you don't want pause menu for example, just delete it in the scene UI and remove some dependencies in code like "Pause game when click escape button".
    When changing fonts remember that they have different sizes and text can disappear, simply scale text in each text component to fix it.

    Adding own UI:
        UI prefab needs to have "_UI_x" postfix in name, where x is a single digit!
        All animations that works in this UI need to have "_UI_x" postfix in name, where x is same digit as in UI's name!
        MightyUIManager script needs to be attached to UI prefab!

ISSUES:
	- NaughtyAttributes package is rather heavy so you can expect FPS drop when playing in editor and with object with script that uses NaughtyAttributes opened in the inspector. 
      Especially [ShowIf()] and [ReorderableList] can cause that. To fix it just collapse component or switch to different GameObject in the inspector.
	- Cannot select resolution using gamepad in options menu
	- Resolutions not displaying properly in options menu




*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace MightyGamePack
{
    public interface IMainGameManager
    {
        void PlayGame();
        void GameOver(int winner);
        void PauseGame();
        void UnpauseGame();
        void OpenOptions();
        void RestartGame();
        void BackToMainMenu();
        void QuitGame(); 
    }

    public enum GameState
    {
        Playing,
        MainMenu,
        OptionsMenu,
        PauseMenu,
        GameOverMenu
    };

    //!!! MIGHTY GAME PACK BRAIN - DO NOT TOUCH !!!
    public class MightyGameManager : MonoBehaviour
    {
        [BoxGroup("Info")] public GameState startGameState = GameState.MainMenu; //Game state set at the start of the game
        [BoxGroup("Info")] [ReadOnly] public GameState gameState;
        [BoxGroup("Info")] [ReadOnly] public GameState lastGameState;
        
        [BoxGroup("Score")] [Tooltip("Enable score")] public bool countScore;
        [BoxGroup("Score")] [ShowIf("countScore")] [Tooltip("Displays in-game score counter (UI element)")] public bool inGameScoreCounter;
        [BoxGroup("Score")] [ShowIf(EConditionOperator.And, "countScore", "inGameScoreCounter")] [Tooltip("Enables popping effect when score is increasing")] public bool inGameScoreCounterPopping;
        [BoxGroup("Score")] [Tooltip("Strength of popping effect when score is increasing")] [ShowIf(EConditionOperator.And, "countScore", "inGameScoreCounter", "inGameScoreCounterPopping")] public int scoreFontSizeMax;

        [BoxGroup("Restart")] [Tooltip("Trigger restart game function during translation between main menu and game (works only when transitionMMToG in MightyUIManager is true)")] public bool restartGameMMToG;
        [BoxGroup("Restart")] [Tooltip("Trigger restart game function during translation between game over menu or pause menu and game (works only when transitionRestart in MightyUIManager is true)")] public bool restartGameGOMOrPMToG;
        [BoxGroup("Restart")] [ShowIf("restartGameMMToG")] [Tooltip("Restart game additional time to wait")] public float restartGameDelay;

        [BoxGroup("Other")] [Tooltip("Hides whole UI on start and immediately jumps to playing state for faster development")] public bool debugHideUI;

        [HideInInspector] public MightyGameManager gameManager;
        [HideInInspector] public MightyUIManager UIManager;
        [HideInInspector] public MightyAudioManager audioManager;
        [HideInInspector] public MightyParticleEffectsManager particleEffectsManager;
        [HideInInspector] public MightyTimersManager timersManager;
        private MainGameManager mainGameManager;


        //void Awake()
        public void InitializeMighty(MainGameManager mainGameManager)
        {
            this.gameManager = this;
            this.mainGameManager = mainGameManager;
            ValidateMightyGameManager();
            SetUpMightyGameManager();
        }

        //void Update()
        public void UpdateMightyGameManager()
        {
            HandleMightySpriteCustomCursor();
        }

        //-----------------------------------------------OTHER FUNCTIONS-------------------------------------------------
        
        #region OtherFunctions

        void SetUpMightyGameManager()
        {
            //gameManager = this;
            FindMightyReferences();
            gameState = lastGameState = startGameState;

            if (debugHideUI)
            {
                gameState = lastGameState = GameState.Playing;
                UIManager.enabled = false;
                UIManager.transform.gameObject.SetActive(false);
            }
        }

        void ValidateMightyGameManager()
        {
            MightyGameManager[] mightyGameManagers = FindObjectsOfType<MightyGameManager>();
            MainGameManager[] mainGameManagers = FindObjectsOfType<MainGameManager>();

            if (mainGameManagers.Length > 1)
            {
                Debug.LogError("There can be only one MainGameManager at a time");
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #endif
                return;
            }
            else
            {
                if (mainGameManagers.Length != 1)
                {
                    Debug.LogError("Mighty Game Manager cannot be used as component. Please use MainGameManager instead");
                    #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
                    #endif
                    return;
                }
            }

            mainGameManager = mainGameManagers[0];
        }

        void HandleMightySpriteCustomCursor()
        {
            if (UIManager.spriteCustomCursor)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    UIManager.SpriteCustomCursorClickPlayAnimation("Click");
                    UIManager.SpriteCustomCursorClickPlayParticleSystem();
                }
            }
        }

        public void SetGameState(GameState value)
        {
            lastGameState = gameState;
            gameState = value;
        }

        void FindMightyReferences()
        {
            MightyUIManager[] UIManagers = FindObjectsOfType<MightyUIManager>();
            if (UIManagers.Length > 1)
            {
                Debug.LogError("There can be only one MightyUIManager at a time");
            }
            else if(UIManagers.Length == 0)
            {
                Debug.LogWarning("No UI Manager detected");
            }
            else
            {
                UIManager = UIManagers[0];
                UIManager.gameManager = this;
                UIManager.mainGameManager = mainGameManager;
            }

            MightyAudioManager[] audioManagers = FindObjectsOfType<MightyAudioManager>();
            if (audioManagers.Length > 1)
            {
                Debug.LogError("There can be only one MightyAudioManager at a time");
            }
            else if (audioManagers.Length == 0)
            {
                Debug.LogWarning("No Audio Manager detected");
            }
            else
            {
                audioManager = audioManagers[0];
            }

            MightyParticleEffectsManager[] particleEffectsManagers = FindObjectsOfType<MightyParticleEffectsManager>();
            if (particleEffectsManagers.Length > 1)
            {
                Debug.LogError("There can be only one MightyParticleEffectsManager at a time");
            }
            else if (particleEffectsManagers.Length == 0)
            {
                Debug.LogWarning("No Particle Effects Manager detected");
            }
            else
            {
                particleEffectsManager = particleEffectsManagers[0];
            }

            MightyTimersManager[] timersUIManagers = FindObjectsOfType<MightyTimersManager>();
            if (timersUIManagers.Length > 1)
            {
                Debug.LogError("There can be only one MightyTimersManager at a time");
            }
            else if (timersUIManagers.Length == 0)
            {
                Debug.LogWarning("No Timer Manager detected");
            }
            else
            {
                timersManager = timersUIManagers[0];
            }
        }
        #endregion    
    }
}
