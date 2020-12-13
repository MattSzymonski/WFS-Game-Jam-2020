using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

using NaughtyAttributes;

namespace MightyGamePack
{
    public enum MenuStyle
    {
        Side,
        Middle
    };

    public enum InputControlDevice //All these settings have enter and escape in common
    {
        Mouse,
        WASD,
        Arrows,
        Gamepad
    };

    public class MightyUIManager : MonoBehaviour
    {
        [HideInInspector] public IMainGameManager mainGameManager; 
        [HideInInspector] public MightyGameManager gameManager;
        [ReadOnly] [Tooltip("Currently used input method. Detected automatically (Mouse, WASD, Arrows, Gamepad)")] public InputControlDevice inputControlDevice;
        [ReadOnly] [Tooltip("Currently selected object of UI")] public GameObject selectedUIObject;
        public bool disableButtonSounds;

        bool blockUIInput; //Used during transitions and playing mode
        bool firstSetResolution = true;
        int UIIndex;
        int score;
        int displayScore;
        int scoreInGameCounterFontSize; //On start
       

        [Header("Game Jam Logo")]
        [Tooltip("Enable game jam logo in main menu")] public bool gameJamLogo;
        [ShowIf("gameJamLogo")] public GameObject gameJamLogoObject;
        [ShowIf("gameJamLogo")] public Sprite gameJamLogoSprite;
        [ShowIf("gameJamLogo")] public bool gameJamLogoLink;
        [ShowIf(EConditionOperator.And, "gameJamLogo", "gameJamLogoLink")] [Tooltip("Eg. http://unity3d.com/")] public string gameJamSiteURL;

        [Header("Transition Panel Settings")]
        [Tooltip("Time of black screen transition")] public float transitionTime;
        [Tooltip("Use black screen transition between game over menu and main menu")] public bool transitionGOMToMM;
        [Tooltip("Use black screen transition between main main and game")] public bool transitionMMToG;
        [ShowIf("transitionMMToG")] [Tooltip("Trigger closing main menu animation before black screen transition between main menu and game")] public bool closeMMBeforeTransitionToG;
        [Tooltip("Use black screen transition between pause menu and main menu")] public bool transitionPMToMM;
        [Tooltip("Use black screen transition when restating the game")] public bool transitionRestart;

        [Header("Custom Cursor")]
        [HideIf("spriteCustomCursor")] [Tooltip("Just change cursor graphic, nothing more")] public bool standardCustomCursor;
        [ShowIf("standardCustomCursor")] public Texture2D cursorTexture;
        [ShowIf("standardCustomCursor")] public CursorMode cursorMode = CursorMode.Auto;
        [ShowIf("standardCustomCursor")] public Vector2 cursorHotSpot = Vector2.zero;
        [HideIf("standardCustomCursor")] [Tooltip("Use advanced cursor with animations, particles, etc")] public bool spriteCustomCursor;
        [ShowIf("spriteCustomCursor")] public GameObject spriteCursorPrefab;
        GameObject spriteCustomCursorObject;
        Animator spriteCustomCursorAnimator;
        SpriteRenderer spriteCustomCursorRenderer;
        ParticleSystem spriteCustomCursorParticleSystem;

        [Header("Other Settings")]
        [Tooltip("Animate camera in main menu")] public bool animatedCameraInMM;
        [Tooltip("Animate camera in game over menu")] public bool animatedCameraInGOM;
        [ShowIf(EConditionOperator.Or, "animatedCameraInMM", "animatedCameraInGOM")] [Tooltip("Note! Camera cannot be component of player GameObject however can be component of its children. Position of camera GameObject should be (0,0,0) and should not be animated by any other script")] public Camera menuCamera;
        [ShowIf(EConditionOperator.Or, "animatedCameraInMM", "animatedCameraInGOM")] public float cameraAnimationSpeed = 1f;
        [ShowIf(EConditionOperator.Or, "animatedCameraInMM", "animatedCameraInGOM")] [Tooltip("How far on its X local axis camera will move")] public float animatedCameraInclination = 3f;
        float animatedCameraDirectionTimer;
        Vector3 menuCameraTargetPosition;



        [Space(10)]
        [Tooltip("Enable screen blink effect when player has been hit for example")] public bool hitBlinkEffect;
        [ShowIf("hitBlinkEffect")] public float hitBlinkEffectDuration;
        [Tooltip("Enable screen color damage effect")] public bool damageEffect;


        [Header("References To Set")]
        public EventSystem eventSystem;

        public GameObject UICanvas;
        public GameObject mainMenu;
        public GameObject mainMenuBackground;
        public GameObject pauseMenu;
        public GameObject pauseBackground;
        public GameObject optionsMenu;
        public GameObject gameOverMenu;
        public GameObject transitionPanel;
        public GameObject scoreInGameCounter;
        public GameObject scoreGameOverMenuCounter;
        public GameObject hitBlinkEffectPanel;
        public GameObject damageEffectPanel;

        public GameObject gameResult;

        Animator transitionPanelAnimator;
        Animator mainMenuAnimator;
        Animator mainMenuBackgroundAnimator;
        Animator pauseMenuAnimator;
        Animator pauseBackgroundAnimator;
        Animator optionsMenuAnimator;
        Animator gameOverMenuAnimator;
        Animator scoreInGameCounterAnimator;

        Text scoreInGameCounterText;
        Text scoreGameOverMenuCounterText;

        Image hitBlinkEffectImage;
        Image damageEffectImage;

        Resolution[] resolutions;

        [Header("Font Changer")]
        public Font fontToSet;

        void Start()
        {
            inputControlDevice = InputControlDevice.Mouse;

            char UITestIndex = gameObject.name.ToCharArray()[gameObject.name.Length - 1];     
            if (char.IsDigit(UITestIndex)) { UIIndex = (int)char.GetNumericValue(UITestIndex); } else { Debug.LogError("Wrong UI GameObject/Prefab name. Should contain \"_UI_x\" postfix, where x is unique digit!"); }

            DeselectUI();

            SetUpGameJamLogo();
            SetUpUIElements();
            SetUpResolutionOptions();
            SetUpCursor();

            animatedCameraDirectionTimer = animatedCameraInclination;
            scoreInGameCounterFontSize = scoreInGameCounterText.fontSize;
        }

        void Update()
        {
            InputControl();
            DetectInputControlDevice();
            SelectButtonWhenNotMouseDevice();
            AnimateMenuCamera();
            UpdateSpriteCursor();
            UpdateScore();
            selectedUIObject = eventSystem.currentSelectedGameObject;
        }

        void FixedUpdate()
        {
            
        }


        public void SetGameResult(string value)
        {
             gameResult.GetComponent<Text>().text = value.ToString();
        }


        //-------------------------------------------------------------SETUP-------------------------------------------------------------	
        #region Setup

        void SetUpCursor()
        {
            if (standardCustomCursor)
            {
                Cursor.SetCursor(cursorTexture, cursorHotSpot, cursorMode);
            }
            else if (spriteCustomCursor)
            {
                Cursor.SetCursor(null, new Vector2(0, 0), cursorMode);
                Cursor.visible = false;
                if (spriteCursorPrefab)
                {
                    spriteCustomCursorObject = Instantiate(spriteCursorPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    spriteCustomCursorObject.transform.parent = GameObject.Find("UI").transform;
                    spriteCustomCursorAnimator = spriteCustomCursorObject.GetComponent<Animator>();
                    spriteCustomCursorRenderer = spriteCustomCursorObject.GetComponent<SpriteRenderer>();
                    spriteCustomCursorParticleSystem = spriteCustomCursorObject.GetComponent<ParticleSystem>();
                }
                else
                {
                    Debug.LogError("No spriteCursorPrefab provided! Cannot create sprite custom cursor!");
                }
            }
        }

        void SetUpGameJamLogo()
        {
            if (gameJamLogo)
            {
                gameJamLogoObject.SetActive(true);
                gameJamLogoObject.GetComponent<Image>().sprite = gameJamLogoSprite;
                if (gameJamLogoLink)
                {
                    gameJamLogoObject.GetComponent<Button>().enabled = true;
                }
                else
                {
                    gameJamLogoObject.transform.GetComponent<Button>().enabled = false;
                }
            }
            else
            {
                gameJamLogoObject.SetActive(false);
            }
        }

        void SetUpUIElements()
        {
            if (transitionPanel)
            {
                transitionPanel.SetActive(true);
                transitionPanelAnimator = transitionPanel.GetComponent<Animator>();
                transitionPanelAnimator.SetBool("Opened", false);
            }

            if (mainMenu)
            {
                mainMenu.SetActive(true);
                mainMenuAnimator = mainMenu.GetComponent<Animator>();
                mainMenuAnimator.SetBool("Opened", true);
            }

            if (mainMenuBackground)
            {
                mainMenuBackground.SetActive(true);
                mainMenuBackgroundAnimator = mainMenuBackground.GetComponent<Animator>();
                mainMenuBackgroundAnimator.SetBool("Opened", true);
            }

            if (pauseMenu)
            {
                pauseMenu.SetActive(true);
                pauseMenuAnimator = pauseMenu.GetComponent<Animator>();
                pauseMenuAnimator.SetBool("Opened", false);
            }

            if (pauseBackground)
            {
                pauseBackground.SetActive(true);
                pauseBackgroundAnimator = pauseBackground.GetComponent<Animator>();
                pauseBackgroundAnimator.SetBool("Opened", false);
            }

            if (optionsMenu)
            {
                optionsMenu.SetActive(true);
                optionsMenuAnimator = optionsMenu.GetComponent<Animator>();
                optionsMenuAnimator.SetBool("Opened", false);
            }

            if (gameOverMenu)
            {
                gameOverMenu.SetActive(true);
                gameOverMenuAnimator = gameOverMenu.GetComponent<Animator>();
                gameOverMenuAnimator.SetBool("Opened", false);
            }

            if (scoreInGameCounter && gameManager.inGameScoreCounter)
            {
                if(gameManager.countScore)
                {
                    scoreInGameCounter.SetActive(true);
                    scoreInGameCounterAnimator = scoreInGameCounter.GetComponent<Animator>();
                    scoreInGameCounterAnimator.SetBool("Opened", false);
                }

                scoreInGameCounterText = scoreInGameCounter.transform.Find("Text").GetComponent<Text>();
                scoreGameOverMenuCounterText = scoreGameOverMenuCounter.transform.Find("Text").GetComponent<Text>();
            }

            if (scoreInGameCounter && (!gameManager.countScore || !gameManager.inGameScoreCounter))
            {
                scoreInGameCounter.SetActive(false);
            }

            if (hitBlinkEffectPanel && hitBlinkEffect)
            {
                hitBlinkEffectPanel.SetActive(true);
                hitBlinkEffectImage = hitBlinkEffectPanel.GetComponent<Image>();
                Image panel = hitBlinkEffectPanel.GetComponent<Image>();
                hitBlinkEffectPanel.GetComponent<Image>().color = new Color(panel.color.r, panel.color.g, panel.color.b, 0);
            }

            if (hitBlinkEffectPanel && !hitBlinkEffect)
            {
                hitBlinkEffectPanel.SetActive(false);
            }

            if (damageEffectPanel && damageEffect)
            {
                damageEffectPanel.SetActive(true);
                damageEffectImage = damageEffectPanel.GetComponent<Image>();
                Image panel = damageEffectPanel.GetComponent<Image>();
                hitBlinkEffectPanel.GetComponent<Image>().color = new Color(panel.color.r, panel.color.g, panel.color.b, 0);
            }

            if (damageEffectPanel && !damageEffect)
            {
                damageEffectPanel.SetActive(false);
            }

            if (UICanvas != null)
            {
                UICanvas.SetActive(true);
            }

        }

        void SetUpResolutionOptions()
        {
            resolutions = Screen.resolutions; //Find avaiable resolutions in player to set them to resolution dropdown in UI
            Dropdown resolutionDropdown = optionsMenu.transform.Find("OptionsPanel").Find("OptionsGroup1").Find("Resolution").GetComponent<Dropdown>();
            resolutionDropdown.ClearOptions();

            int currentResolutionIndex = 0;
            List<string> options = new List<string>();
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);

                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
        #endregion

        //-----------------------------------------------------UI HANDLING FUNCTIONS-----------------------------------------------------
        #region UIHandlingFunctions

        public void SpriteCustomCursorClickPlayAnimation(string triggerName)
        {
            if (spriteCustomCursor)
            {
                if (spriteCustomCursorAnimator)
                {
                    spriteCustomCursorAnimator.SetTrigger(triggerName);
                }
            }
            else
            {
                Debug.LogError("Cannot play cursor animation! spriteCustomCursor is disabled in MightyUIManger!");
            }
        }

        public void SpriteCustomCursorClickPlayParticleSystem()
        {
            if (spriteCustomCursor)
            {
                if (spriteCustomCursorParticleSystem)
                {
                    spriteCustomCursorParticleSystem.Play();
                }
            }
            else
            {
                Debug.LogError("Cannot play cursor particle system! spriteCustomCursor is disabled in MightyUIManger!");
            }
        }

        public void TriggerHitBlinkEffect(Color color)
        {
            if (hitBlinkEffect && enabled)
            {
                StartCoroutine(HitBlinkEffectCoroutine(color));
            }
            else
            {
                Debug.LogError("Cannot trigger hit blink effect! Hit blink effects are disabled in MightyUIManger or debugHideUI is enabled!");
            }
        }

        IEnumerator HitBlinkEffectCoroutine(Color color)
        {
            hitBlinkEffectImage.color = color;
            yield return new WaitForSeconds(hitBlinkEffectDuration);
            hitBlinkEffectImage.color = new Color(0, 0, 0, 0);
        }

        public void SetDamageEffect(Color color, float damage)
        {
            if (damageEffect && enabled)
            {
                damageEffectImage.color = new Color(color.r, color.g, color.b, damage);
            }
            else
            {
                Debug.LogError("Cannot set damage effect! Damage effect is disabled in MightyUIManger or debugHideUI is enabled!!");
            }     
        }

        public void SetScore(int value)
        {
            if (gameManager.countScore && enabled)
            {
                score = value;

                //inGameScoreText.text = value.ToString();
                scoreGameOverMenuCounterText.text = value.ToString();
            }
        }

        void UpdateScore()
        {
            if (gameManager.countScore)
            {
                if (gameManager.inGameScoreCounterPopping)
                {
                    int fontMax = gameManager.scoreFontSizeMax;
                    int speed = 1; //[Minor problem] When we add 1 but speed value is 2 then it will jump by 1 around target value. 1 is safe value for speed 

                    if (displayScore != score)
                    {
                        if (scoreInGameCounterText.fontSize != scoreInGameCounterFontSize + fontMax) { scoreInGameCounterText.fontSize += speed; }
                        if (displayScore < score) { displayScore += speed; } else { displayScore -= speed; }
                    }
                    else if (scoreInGameCounterText.fontSize != scoreInGameCounterFontSize)
                    {
                        scoreInGameCounterText.fontSize -= speed;
                    }

                    scoreInGameCounterText.text = "" + displayScore;
                }
                else
                {
                    displayScore = score;
                    scoreInGameCounterText.text = "" + displayScore;
                }
         
            }
        }

        public void ResetScore()
        {
            score = displayScore = 0;
        }
        #endregion

        //-------------------------------------------------------------MISC--------------------------------------------------------------	
        #region Misc

        //Click the button then update editor view by dragging some windows
        [Button]
        public void ChangeFont()
        {
            foreach (Text text in UICanvas.GetComponentsInChildren<Text>()) 
            {
                text.font = fontToSet;
            }
        }

        [Button]
        public void IncreaseFontSize()
        {
            foreach (Text text in UICanvas.GetComponentsInChildren<Text>())
            {
                text.fontSize = text.fontSize + 1;
            }
        }

        [Button]
        public void DecreaseFontSize()
        {
            foreach (Text text in UICanvas.GetComponentsInChildren<Text>())
            {
                text.fontSize = text.fontSize - 1;
            }
        }

        void UpdateSpriteCursor()
        {
            if (spriteCustomCursor)
            {
                spriteCustomCursorObject.transform.localPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
                spriteCustomCursorObject.transform.forward = Camera.main.transform.forward;
            }    
        }

        void AnimateMenuCamera()
        {
            if ((animatedCameraInMM && gameManager.gameState == GameState.MainMenu) || (animatedCameraInGOM && gameManager.gameState == GameState.GameOverMenu) || (gameManager.gameState == GameState.OptionsMenu && gameManager.lastGameState == GameState.MainMenu))
            {
                if (!blockUIInput)
                {
                    animatedCameraDirectionTimer += cameraAnimationSpeed * Time.unscaledDeltaTime;
                }

                menuCameraTargetPosition = new Vector3(Mathf.PingPong(animatedCameraDirectionTimer, animatedCameraInclination * 2) - animatedCameraInclination, menuCamera.transform.localPosition.y, menuCamera.transform.localPosition.z);
                Vector3 smoothedPosition = Vector3.Lerp(menuCamera.transform.localPosition, menuCameraTargetPosition, 0.08f);
                menuCamera.transform.localPosition = smoothedPosition;
            }
        }

        AnimationClip GetAnimationFromAnimator(Animator animator, string name)
        {
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            for (int i = 0; i < ac.animationClips.Length; i++)
            {
                if (ac.animationClips[i].name == name)
                {
                    return ac.animationClips[i];
                }
            }
            return null;
        }

        public void OpenGameJamSite()
        {
            if (gameJamSiteURL != null)
            {
                Application.OpenURL(gameJamSiteURL);
            }
        }

        public void SetResolution(int resolutionIndex)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            if (!firstSetResolution)
            {
                gameManager.audioManager.PlaySound("UI_Button_Click");
                firstSetResolution = false;
            } 
        }

        public void SetFullscreen(bool isFullscreen)
        {
#if UNITY_EDITOR
            Debug.Log("Cannot set fullscreen in editor");
#else
        Screen.fullScreen = isFullscreen;   
#endif
        }

        public void UIPlaySound(string soundName)
        {
            if (!disableButtonSounds)
            {
                gameManager.audioManager.PlaySound(soundName);
            }
        }

        public void SelectButtonNoSound(Button button)
        {
            if (disableButtonSounds)
            {
                button.Select();
            }
            else
            {
                disableButtonSounds = true;
                button.Select();
                disableButtonSounds = false;
            }   
        }

        void ToggleUIInputBlock(bool state)
        {
            if (state) { DeselectUI(); }
            blockUIInput = state;
            eventSystem.sendNavigationEvents = !state;
        }

        public void DeselectUI() //Make sure that no button or any other UI element is selected (Prevent having selected "play" or "restart" button while in playing mode)
        {
            eventSystem.SetSelectedGameObject(null);
        }

        void SelectButtonWhenNotMouseDevice() //Selects appropriate "start" buttons for nonmouse input when switching menus
        {
            if (!blockUIInput)
            {
                if (inputControlDevice != InputControlDevice.Mouse && eventSystem.currentSelectedGameObject == null)
                {
                    if (gameManager.gameState == GameState.MainMenu)
                    {
                        mainMenu.transform.Find("MainPanel").Find("ButtonsGroup1").Find("PlayButton").GetComponent<Button>().Select();
                        return;
                    }
                    if (gameManager.gameState == GameState.PauseMenu)
                    {
                        pauseMenu.transform.Find("PausePanel").Find("ButtonsGroup2").Find("ResumeButton").GetComponent<Button>().Select();
                        return;
                    }
                    if (gameManager.gameState == GameState.OptionsMenu)
                    {
                        optionsMenu.transform.Find("OptionsPanel").Find("ButtonsGroup1").Find("BackButton").GetComponent<Button>().Select();
                        return;
                    }
                    if (gameManager.gameState == GameState.GameOverMenu)
                    {
                        gameOverMenu.transform.Find("ButtonsGroup1").Find("RestartButton").GetComponent<Button>().Select();
                        return;
                    }
                }
            }
        }

        void DetectInputControlDevice()
        {
            if (gameManager.gameState != GameState.Playing)
            {
                if (inputControlDevice != InputControlDevice.Mouse)
                {
                    if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 || Input.GetButton("LMB") || Input.GetButton("RMB") || Input.GetAxis("Mouse ScrollWheel") != 0)
                    {
                        inputControlDevice = InputControlDevice.Mouse;
                        return;
                    }
                }

                if (inputControlDevice != InputControlDevice.Gamepad)
                {
                    if (Input.GetAxis("ControllerAny Left Stick Horizontal UI Sensitivity") != 0 || Input.GetAxis("ControllerAny Left Stick Vertical UI Sensitivity") != 0
                        || Input.GetButton("ControllerAny A") || Input.GetButton("ControllerAny B") || Input.GetButton("ControllerAny X") || Input.GetButton("ControllerAny Y") || Input.GetButton("ControllerAny Start"))
                    {
                        inputControlDevice = InputControlDevice.Gamepad;
                        return;
                    }
                }

                if (inputControlDevice != InputControlDevice.WASD)
                {
                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                    {
                        inputControlDevice = InputControlDevice.WASD;
                        return;
                    }
                }

                if (inputControlDevice != InputControlDevice.Arrows)
                {
                    if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
                    {
                        inputControlDevice = InputControlDevice.Arrows;
                        return;
                    }
                }
            }
        }

        void InputControl()
        {
            if ((!blockUIInput || gameManager.gameState == GameState.Playing) && gameManager.gameState != GameState.MainMenu)
            {
                if (Input.GetButtonDown("Escape"))
                {
                    UIPlaySound("UI_Button_Click");
                    if (gameManager.gameState == GameState.Playing)
                    {
                        PauseGame();
                        return;
                    }
                    if (gameManager.gameState == GameState.PauseMenu)
                    {
                        UnpauseGame();
                        return;
                    }
                    if (gameManager.gameState == GameState.OptionsMenu)
                    {
                        CloseOptions();
                        return;
                    }
                }

                if (Input.GetButtonDown("ControllerAny Start"))
                {
                    UIPlaySound("UI_Button_Click");
                    if (gameManager.gameState == GameState.Playing)
                    {
                        PauseGame();
                        return;
                    }
                    if (gameManager.gameState == GameState.PauseMenu)
                    {
                        UnpauseGame();
                        return;
                    }
                }

                if (Input.GetButtonDown("ControllerAny B"))
                {
                    UIPlaySound("UI_Button_Click");
                    if (gameManager.gameState == GameState.PauseMenu)
                    {
                        UnpauseGame();
                        return;
                    }
                    if (gameManager.gameState == GameState.OptionsMenu)
                    {
                        CloseOptions();
                        return;
                    }
                }
            }
        }

        string GetAnimationNameFromUIIndex(string animationName)
        {
            return animationName + "_UI_" + UIIndex;
        }

        #endregion

        //---------------------------------------------------------UI TRANSITIONS--------------------------------------------------------
        #region UI Transitions
        IEnumerator QuitGameCoroutine()
        {
#if UNITY_EDITOR
            yield return new WaitForSeconds(0.1f);
#else
        ToggleUIInputBlock(true);
        transitionPanelAnimator.SetBool("Opened", true);
        yield return new WaitForSeconds(GetAnimationFromAnimator(transitionPanelAnimator, "TransitionPanelOpen").length + 0.5f);
#endif
            mainGameManager.QuitGame();
        }

        public void QuitGame()
        {
            StartCoroutine(QuitGameCoroutine());
        }

        IEnumerator PlayGameCoroutine()
        {
            ToggleUIInputBlock(true);
            if (transitionMMToG)
            {
                if (closeMMBeforeTransitionToG)
                {
                    mainMenuBackgroundAnimator.SetBool("Opened", false);
                    mainMenuAnimator.SetBool("Opened", false);
                   // yield return new WaitForSeconds(GetAnimationFromAnimator(mainMenuAnimator, GetAnimationNameFromUIIndex("MainMenuClose")).length);
                    transitionPanelAnimator.SetBool("Opened", true);
                    yield return new WaitForSeconds(GetAnimationFromAnimator(transitionPanelAnimator, GetAnimationNameFromUIIndex("TransitionPanelOpen")).length);
                    if (gameManager.restartGameMMToG)
                    {
                        mainGameManager.RestartGame();
                        yield return new WaitForSeconds(gameManager.restartGameDelay);
                    }
                    
                    yield return new WaitForSeconds(transitionTime);
                    if (animatedCameraInMM || animatedCameraInGOM)
                    {
                        animatedCameraDirectionTimer = animatedCameraInclination;
                        menuCameraTargetPosition = Vector3.zero;
                        yield return new WaitForSeconds(0.2f);
                    }
                    transitionPanelAnimator.SetBool("Opened", false);
                    yield return new WaitForSeconds(GetAnimationFromAnimator(transitionPanelAnimator, GetAnimationNameFromUIIndex("TransitionPanelClose")).length);
                }
                else
                {
                    transitionPanelAnimator.SetBool("Opened", true);
                    yield return new WaitForSeconds(GetAnimationFromAnimator(transitionPanelAnimator, GetAnimationNameFromUIIndex("TransitionPanelOpen")).length);
                    mainMenuBackgroundAnimator.SetBool("Opened", false);
                    mainMenuAnimator.SetBool("Opened", false);
                    if (gameManager.restartGameMMToG)
                    {
                        mainGameManager.RestartGame();
                        yield return new WaitForSeconds(gameManager.restartGameDelay);
                    }          
                    yield return new WaitForSeconds(transitionTime);
                    if (animatedCameraInMM || animatedCameraInGOM)
                    {
                        animatedCameraDirectionTimer = animatedCameraInclination;
                        menuCameraTargetPosition = Vector3.zero;
                        yield return new WaitForSeconds(0.2f);
                    }
                    transitionPanelAnimator.SetBool("Opened", false);
                    yield return new WaitForSeconds(GetAnimationFromAnimator(transitionPanelAnimator, GetAnimationNameFromUIIndex("TransitionPanelClose")).length);
                }
            }
            else
            {          
                mainMenuBackgroundAnimator.SetBool("Opened", false);
                mainMenuAnimator.SetBool("Opened", false);
                if (animatedCameraInMM || animatedCameraInGOM)
                {
                    animatedCameraDirectionTimer = animatedCameraInclination;
                    menuCameraTargetPosition = Vector3.zero;
                }
                yield return new WaitForSeconds(GetAnimationFromAnimator(mainMenuAnimator, GetAnimationNameFromUIIndex("MainMenuClose")).length);
            }

            if (gameManager.countScore && gameManager.inGameScoreCounter)
            {
                scoreInGameCounterAnimator.SetBool("Opened", true);
            }

            gameManager.SetGameState(GameState.Playing);
            mainGameManager.PlayGame();
        }

        public void PlayGame()
        {
            StartCoroutine(PlayGameCoroutine());
        }

        IEnumerator PauseGameCoroutine()
        {
            ToggleUIInputBlock(true);
            gameManager.SetGameState(GameState.PauseMenu);

            if (gameManager.countScore && gameManager.inGameScoreCounter)
            {
                scoreInGameCounterAnimator.SetBool("Opened", false);
            }

            pauseBackgroundAnimator.SetBool("Opened", true);
            pauseMenuAnimator.SetBool("Opened", true);
            if (inputControlDevice != InputControlDevice.Mouse)
            {
                SelectButtonNoSound(pauseMenu.transform.Find("PausePanel").Find("ButtonsGroup2").Find("ResumeButton").GetComponent<Button>());
            }
            yield return new WaitForSeconds(GetAnimationFromAnimator(pauseMenuAnimator, GetAnimationNameFromUIIndex("PauseMenuOpen")).length);




            mainGameManager.PauseGame();
            ToggleUIInputBlock(false);
        }

        public void PauseGame()
        {
            StartCoroutine(PauseGameCoroutine());
        }

        IEnumerator UnpauseGameCoroutine()
        {
            ToggleUIInputBlock(true);
            pauseMenuAnimator.SetBool("Opened", false);
            pauseBackgroundAnimator.SetBool("Opened", false);
            if (inputControlDevice != InputControlDevice.Mouse)
            {
                yield return new WaitForSeconds(GetAnimationFromAnimator(pauseBackgroundAnimator, GetAnimationNameFromUIIndex("PauseBackgroundClose")).length);
            }

            if (gameManager.countScore && gameManager.inGameScoreCounter)
            {
                scoreInGameCounterAnimator.SetBool("Opened", true);
            }


            gameManager.SetGameState(GameState.Playing);
            mainGameManager.UnpauseGame();
        }

        public void UnpauseGame()
        {
            StartCoroutine(UnpauseGameCoroutine());
        }

        IEnumerator OpenOptionsCoroutine()
        {
            ToggleUIInputBlock(true);
            if (gameManager.gameState == GameState.PauseMenu)
            {
                gameManager.SetGameState(GameState.OptionsMenu);
                pauseMenuAnimator.SetBool("Opened", false);
                yield return new WaitForSeconds(GetAnimationFromAnimator(pauseMenuAnimator, GetAnimationNameFromUIIndex("PauseMenuClose")).length);
            }

            if (gameManager.gameState == GameState.MainMenu)
            {
                gameManager.SetGameState(GameState.OptionsMenu);
                mainMenuAnimator.SetBool("Opened", false);
                yield return new WaitForSeconds(GetAnimationFromAnimator(mainMenuAnimator, GetAnimationNameFromUIIndex("MainMenuClose")).length);
            }

            optionsMenuAnimator.SetBool("Opened", true);
            if (inputControlDevice != InputControlDevice.Mouse)
            {
                SelectButtonNoSound(optionsMenu.transform.Find("OptionsPanel").Find("ButtonsGroup1").Find("BackButton").GetComponent<Button>());
            }
            yield return new WaitForSeconds(GetAnimationFromAnimator(optionsMenuAnimator, GetAnimationNameFromUIIndex("OptionsMenuClose")).length);

            mainGameManager.OpenOptions();
            ToggleUIInputBlock(false);
        }

        public void OpenOptions()
        {
            StartCoroutine(OpenOptionsCoroutine());
        }

        IEnumerator CloseOptionsCoroutine()
        {
            ToggleUIInputBlock(true);
            if (gameManager.lastGameState == GameState.PauseMenu)
            {
                optionsMenuAnimator.SetBool("Opened", false);
                yield return new WaitForSeconds(GetAnimationFromAnimator(optionsMenuAnimator, GetAnimationNameFromUIIndex("OptionsMenuClose")).length);
                pauseMenuAnimator.SetBool("Opened", true);
                if (inputControlDevice != InputControlDevice.Mouse)
                {
                    SelectButtonNoSound(pauseMenu.transform.Find("PausePanel").Find("ButtonsGroup1").Find("OptionsButton").GetComponent<Button>());
                }
                gameManager.SetGameState(GameState.PauseMenu);
                yield return new WaitForSeconds(GetAnimationFromAnimator(pauseMenuAnimator, GetAnimationNameFromUIIndex("PauseMenuOpen")).length);
            }

            if (gameManager.lastGameState == GameState.MainMenu)
            {
                optionsMenuAnimator.SetBool("Opened", false);
                yield return new WaitForSeconds(GetAnimationFromAnimator(optionsMenuAnimator, GetAnimationNameFromUIIndex("OptionsMenuClose")).length);
                mainMenuAnimator.SetBool("Opened", true);
                if (inputControlDevice != InputControlDevice.Mouse)
                {
                    SelectButtonNoSound(mainMenu.transform.Find("MainPanel").Find("ButtonsGroup1").Find("OptionsButton").GetComponent<Button>());
                }
                gameManager.SetGameState(GameState.MainMenu);
                yield return new WaitForSeconds(GetAnimationFromAnimator(mainMenuAnimator, GetAnimationNameFromUIIndex("MainMenuOpen")).length);
            }
            ToggleUIInputBlock(false);
        }

        public void CloseOptions()
        {
            StartCoroutine(CloseOptionsCoroutine());
        }

        IEnumerator GoToMainMenuCoroutine()
        {
            ToggleUIInputBlock(true);
            if (gameManager.gameState == GameState.PauseMenu)
            {
                pauseMenuAnimator.SetBool("Opened", false);
                pauseBackgroundAnimator.SetBool("Opened", false);
                //yield return new WaitForSeconds(GetAnimationFromAnimator(pauseBackgroundAnimator, GetAnimationNameFromUIIndex("PauseBackgroundClose")).length);
                if (transitionPMToMM)
                {
                    transitionPanelAnimator.SetBool("Opened", true);
                    yield return new WaitForSeconds(GetAnimationFromAnimator(transitionPanelAnimator, GetAnimationNameFromUIIndex("TransitionPanelOpen")).length);

                    yield return new WaitForSeconds(transitionTime);

                    transitionPanelAnimator.SetBool("Opened", false);
                    yield return new WaitForSeconds(GetAnimationFromAnimator(transitionPanelAnimator, GetAnimationNameFromUIIndex("TransitionPanelClose")).length);
                }
                else
                {
                    yield return new WaitForSeconds(GetAnimationFromAnimator(pauseBackgroundAnimator, GetAnimationNameFromUIIndex("PauseBackgroundClose")).length);
                    //yield return new WaitForSeconds(GetAnimationFromAnimator(pauseMenuAnimator, GetAnimationNameFromUIIndex("PauseMenuClose")).length);
                }

                mainMenuBackgroundAnimator.SetBool("Opened", true);
                yield return new WaitForSeconds(GetAnimationFromAnimator(mainMenuBackgroundAnimator, GetAnimationNameFromUIIndex("MainMenuBackgroundOpen")).length);
                mainMenuAnimator.SetBool("Opened", true);
                gameManager.SetGameState(GameState.MainMenu);
                yield return new WaitForSeconds(GetAnimationFromAnimator(mainMenuAnimator, GetAnimationNameFromUIIndex("MainMenuOpen")).length);
                mainGameManager.BackToMainMenu();
            }

            if (gameManager.gameState == GameState.GameOverMenu)
            {
                gameOverMenuAnimator.SetBool("Opened", false);

                if (transitionGOMToMM)
                {
                    transitionPanelAnimator.SetBool("Opened", true);
                    yield return new WaitForSeconds(GetAnimationFromAnimator(transitionPanelAnimator, GetAnimationNameFromUIIndex("TransitionPanelOpen")).length);

                    yield return new WaitForSeconds(transitionTime);

                    transitionPanelAnimator.SetBool("Opened", false);
                    yield return new WaitForSeconds(GetAnimationFromAnimator(transitionPanelAnimator, GetAnimationNameFromUIIndex("TransitionPanelClose")).length);
                }
                else
                {
                    yield return new WaitForSeconds(GetAnimationFromAnimator(gameOverMenuAnimator, GetAnimationNameFromUIIndex("GameOverMenuClose")).length);
                }

                mainMenuBackgroundAnimator.SetBool("Opened", true);
                yield return new WaitForSeconds(GetAnimationFromAnimator(mainMenuBackgroundAnimator, GetAnimationNameFromUIIndex("MainMenuBackgroundOpen")).length);
                mainMenuAnimator.SetBool("Opened", true);
                gameManager.SetGameState(GameState.MainMenu);
                yield return new WaitForSeconds(GetAnimationFromAnimator(mainMenuAnimator, GetAnimationNameFromUIIndex("MainMenuOpen")).length);
                mainGameManager.BackToMainMenu();
            }
            ToggleUIInputBlock(false);
        }

        public void GoToMainMenu()
        {
            StartCoroutine(GoToMainMenuCoroutine());
        }

        IEnumerator GameOverCoroutine()
        {
            ToggleUIInputBlock(true);
            gameManager.SetGameState(GameState.GameOverMenu);
            if (gameManager.countScore && gameManager.inGameScoreCounter)
            {
                scoreInGameCounterAnimator.SetBool("Opened", false);
            }
            gameOverMenuAnimator.SetBool("Opened", true);
            yield return new WaitForSeconds(GetAnimationFromAnimator(gameOverMenuAnimator, GetAnimationNameFromUIIndex("GameOverMenuOpen")).length);
            ToggleUIInputBlock(false);
        }

        public void GameOver()
        {
            StartCoroutine(GameOverCoroutine());
        }

        IEnumerator RestartGameCoroutine()
        {
            ToggleUIInputBlock(true);
            if (gameManager.gameState == GameState.GameOverMenu)
            {
                gameOverMenuAnimator.SetBool("Opened", false);

                if (transitionRestart)
                {
                    transitionPanelAnimator.SetBool("Opened", true);
                    yield return new WaitForSeconds(GetAnimationFromAnimator(transitionPanelAnimator, GetAnimationNameFromUIIndex("TransitionPanelOpen")).length);
                    if (gameManager.restartGameGOMOrPMToG)
                    {
                        mainGameManager.RestartGame();
                        yield return new WaitForSeconds(gameManager.restartGameDelay);
                    }

                    yield return new WaitForSeconds(transitionTime);

                    if (animatedCameraInMM || animatedCameraInGOM)
                    {
                        animatedCameraDirectionTimer = animatedCameraInclination;
                        menuCameraTargetPosition = Vector3.zero;
                        yield return new WaitForSeconds(0.2f);
                    }  
                    transitionPanelAnimator.SetBool("Opened", false);
                    yield return new WaitForSeconds(GetAnimationFromAnimator(transitionPanelAnimator, GetAnimationNameFromUIIndex("TransitionPanelClose")).length);
                }
                else
                {
                    if (animatedCameraInMM || animatedCameraInGOM)
                    {
                        animatedCameraDirectionTimer = animatedCameraInclination;
                        menuCameraTargetPosition = Vector3.zero;
                    }
                    yield return new WaitForSeconds(GetAnimationFromAnimator(gameOverMenuAnimator, GetAnimationNameFromUIIndex("GameOverMenuClose")).length);
                }
            }

            if (gameManager.gameState == GameState.PauseMenu)
            {
                pauseMenuAnimator.SetBool("Opened", false);
                //yield return new WaitForSeconds(GetAnimationFromAnimator(pauseMenuAnimator, GetAnimationNameFromUIIndex("PauseMenuClose").length);
                pauseBackgroundAnimator.SetBool("Opened", false);

                if (transitionRestart)
                {
                    transitionPanelAnimator.SetBool("Opened", true);
                    yield return new WaitForSeconds(GetAnimationFromAnimator(transitionPanelAnimator, GetAnimationNameFromUIIndex("TransitionPanelOpen")).length);

                    if (gameManager.restartGameGOMOrPMToG)
                    {
                        mainGameManager.RestartGame();
                        yield return new WaitForSeconds(gameManager.restartGameDelay);
                    }

                    yield return new WaitForSeconds(transitionTime);

                    transitionPanelAnimator.SetBool("Opened", false);
                    yield return new WaitForSeconds(GetAnimationFromAnimator(transitionPanelAnimator, GetAnimationNameFromUIIndex("TransitionPanelClose")).length);
                }
                else
                {
                    yield return new WaitForSeconds(GetAnimationFromAnimator(pauseBackgroundAnimator, GetAnimationNameFromUIIndex("PauseBackgroundClose")).length);
                }
            }
            if (gameManager.countScore && gameManager.inGameScoreCounter)
            {
                scoreInGameCounterAnimator.SetBool("Opened", true);
            }
            gameManager.SetGameState(GameState.Playing);
            ToggleUIInputBlock(false);
        }

        public void RestartGame()
        {
            StartCoroutine(RestartGameCoroutine());
        }

        #endregion
    }
}
