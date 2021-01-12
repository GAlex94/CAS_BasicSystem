using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Casino
{
    public class GameManager : Singleton<GameManager>
    {
        private bool isDebug = true;
        private bool isFakeShop = true;
        private CasinoGameType debugGame = CasinoGameType.None;
        private string debugSimpleGameTag = "";


        private CasinoGameType currentGameType = CasinoGameType.None;
        private string currentSimpleGameTag = "";

        public CasinoGameType CurrentGame => currentGameType;
        public string CurrentSimpleGameTag => currentSimpleGameTag;
        public bool IsDebug => isDebug;
        public bool IsFakeShop => isFakeShop;

        void Awake()
        {
            Debug.Log("GameManager awake");
            DontDestroyOnLoad(this.gameObject);
        }

        void Start()
        {
            Debug.Log("Game manager start!");

            StartCoroutine(DefferGameStart());
        }

        IEnumerator DefferGameStart()
        {
            yield return new WaitForEndOfFrame();

            Debug.Log("Launch casino");
            if (this.debugGame == CasinoGameType.None)
            {
                GUIController.Instance.ShowScreen<LobbyScreen>();
                GUIController.Instance.ShowScreen<TopBarScreen>();
            }
            else
            {
                currentGameType = debugGame;
                currentSimpleGameTag = debugSimpleGameTag;

                LobbyGamesConfig newMainGame = DataManager.Instance.CasinoConfig.FindGame(debugGame);
                DataManager.Instance.LoadGameTypeConfig(newMainGame);
                DataManager.Instance.LoadSimpleGameConfig(debugSimpleGameTag);

                IGame mainGameObject = FindObjectsOfType<MonoBehaviour>().OfType<IGame>().FirstOrDefault();
                if (mainGameObject != null)
                    mainGameObject.StartGame();
                else
                {
                    Debug.LogError("IGame object not found in scene! Game didn't launch..");
                }
            }
        }

        public void Init(bool isDebug, bool isFakeShop, CasinoGameType debugGame, string debugSimpleGameTag)
        {
            this.debugGame = debugGame;
            this.debugSimpleGameTag = debugSimpleGameTag;
            this.isDebug = isDebug;
            this.isFakeShop = isFakeShop;

            if (!Debug.isDebugBuild)
            {
                this.debugGame = CasinoGameType.None;
                this.debugSimpleGameTag = "";
                this.isDebug = false;
                this.isFakeShop = false;
            }
        }

        #region Activators from loading scene
        public void RestoreMenu()
        {
            StartCoroutine(DefferRestoreMenu());
        }

        IEnumerator DefferRestoreMenu()
        {
            yield return null;

            Debug.Log("Menu restored");
            GUIController.Instance.ShowScreen<LobbyScreen>();
            GUIController.Instance.ShowScreen<TopBarScreen>();
        }

        public void ActivateSimpleGameOnScene()
        {
            StartCoroutine(DefferActivateGameOnSceneGame());
        }

        IEnumerator DefferActivateGameOnSceneGame()
        {
            yield return null;

            Debug.Log("Activate simple game...");

            IGame mainGameObject = FindObjectsOfType<MonoBehaviour>().OfType<IGame>().FirstOrDefault();
            if (mainGameObject != null)
                mainGameObject.StartGame();
            else
            {
                Debug.LogError("IGame object not found in scene! Game didn't launch..");
            }
        }
        #endregion

        public void ActivateMainGame(CasinoGameType gameType)
        {
            LobbyGamesConfig newMainGame = DataManager.Instance.CasinoConfig.FindGame(gameType);
            if (newMainGame != null)
            {
                currentGameType = gameType;
                Debug.Log("Activate main game: " + currentGameType);

                DataManager.Instance.LoadGameTypeConfig(newMainGame);

                GUIScreen gameLobbyScreen = GUIController.Instance.FoundScreen(newMainGame.gameLobby.GetType());
                if (gameLobbyScreen != null)
                    GUIController.Instance.ShowScreen(gameLobbyScreen);
            }
        }

        public void ClearCurrentMainGame()
        {
            Debug.Log("Exit from main game. Old game: " + currentGameType);
            currentGameType = CasinoGameType.None;
            currentSimpleGameTag = "";
        }

        public void LoadSimpleGame(string gameTag)
        {
            Debug.Log("Start load simple game: " + gameTag + " in main game: " + currentGameType);
            currentSimpleGameTag = gameTag;
            DataManager.Instance.LoadSimpleGameConfig(gameTag);

            //todo: need correct animations for hiding lobby and showing loading scene

            SceneManager.LoadScene("loading");
        }

        public void LoadMenu()
        {
            Debug.Log("Start load menu");

            //todo: need correct animations for hiding cur screen and showing loading scene

            ClearCurrentMainGame();

            SceneManager.LoadScene("loading");
        }
    }
}
