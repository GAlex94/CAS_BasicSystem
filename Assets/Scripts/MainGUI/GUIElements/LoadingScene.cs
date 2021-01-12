using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Casino
{
    public class LoadingScene : MonoBehaviour
    {
        private const string MENU_SCENE_NAME = "menu";

        [SerializeField]
        private TextMeshProUGUI loadingText;

        [SerializeField]
        private Slider progressSlider;

        [SerializeField]
        private TextMeshProUGUI loadingTitle;

        [SerializeField]
        private Image loadingBack;

        private AsyncOperation asyncLoad = null;

        void Start()
        {
            Debug.Log("LoadingScene start");
            loadingText.text = "Loading " + GameManager.Instance.CurrentGame + "_" + GameManager.Instance.CurrentSimpleGameTag;

            loadingTitle.text = DataManager.Instance.CurrentSimpleGameConfig.loadingTextTag;
            loadingBack.sprite = DataManager.Instance.CurrentSimpleGameConfig.loadingBack;

            StartCoroutine(LoadGame(GameManager.Instance.CurrentGame));
        }

        IEnumerator LoadGame(CasinoGameType gameType)
        {
            yield return null;

            progressSlider.value = 0.0f;

            string sceneName = "";
            if (gameType == CasinoGameType.None)
            {
                sceneName = MENU_SCENE_NAME;
            }
            else
            {
                sceneName = DataManager.Instance.CurrentSimpleGameConfig.sceneName;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("Loading scene is empty! Stopped.");
                yield break;
            }

            asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            yield return asyncLoad.isDone;

            Debug.Log("LoadingScene: finish loading");
            progressSlider.value = 1.0f;
            asyncLoad = null;

            if (sceneName == MENU_SCENE_NAME)
                GameManager.Instance.RestoreMenu();
            else
                GameManager.Instance.ActivateSimpleGameOnScene();
        }

        void Update()
        {
            if (asyncLoad != null)
                progressSlider.value = asyncLoad.progress;
        }
    }
}
