using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Casino
{
    public interface IGuiListener
    {

    }

    public class GUIScreen : MonoBehaviour
    {
        [SerializeField]
        private ScreenLayer guiLayer;

        [SerializeField]
        private int offsetZ = 0;

        [SerializeField]
        private List<Button> _ignoreForPlayClickSoundButtons = new List<Button>();
        public List<Button> IgnoreForPlayClickSoundButtons
        {
            get { return _ignoreForPlayClickSoundButtons; }
        }

        [SerializeField]
        private EffectType appearEffect = EffectType.None;

        [SerializeField]
        private EffectType fadeEffect = EffectType.None;

        private bool showed = false;

        private enum EffectType
        {
            Scale, Alpha, None
        }

        protected IGuiListener gameListener;

        public bool IsShowed
        {
            get { return showed; }
        }

        public ScreenLayer ScreenLayer
        {
            get { return guiLayer; }
        }

        public int OffsetZ
        {
            get => offsetZ;
            set => offsetZ = value;
        }

        public void Show()
        {
            showed = true;

            ApplyEffect(true);

            StartCoroutine(OnShowNextFrame());

            Analytics.CustomEvent("OPEN_SCREEN", new Dictionary<string, object>
            {
                { "SCREEN_NAME", gameObject.name},
            });

        }

        IEnumerator OnShowNextFrame()
        {
            yield return null;
            OnShow();
        }

        public void Hide()
        {
            showed = false;
            ApplyEffect(false);
            OnHide();

            Analytics.CustomEvent("CLOSE_SCREEN", new Dictionary<string, object>
            {
                { "SCREEN_NAME", gameObject.name},
            });
        }

        public void SetGuiListener(IGuiListener gameListener)
        {
            this.gameListener = gameListener;
            UpdateListener();
        }

        public void ClearListener()
        {
            this.gameListener = null;
            UpdateListener();
        }

        protected virtual void UpdateListener()
        {

        }

        protected virtual void OnShow()
        {

        }

        protected virtual void OnHide()
        {

        }

        private void ApplyEffect(bool isAppear)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();

            if (isAppear)
            {
                gameObject.SetActive(true);
                switch (appearEffect)
                {
                    case EffectType.Scale:
                        {
                            transform.localScale = Vector3.zero;
                            LeanTween.scale(rectTransform, Vector3.one, .2f).setEase(LeanTweenType.easeOutExpo).setOnComplete(() => rectTransform.localScale = Vector3.one);
                            break;
                        }
                    case EffectType.Alpha:
                        {
                            LeanTween.alpha(rectTransform, 0, 0);
                            LeanTween.alpha(rectTransform, 1, 0.2f);
                            break;
                        }
                }
            }
            else
            {
                switch (fadeEffect)
                {
                    case EffectType.Scale:
                        {
                        //    Debug.Log("0");
                            LeanTween.scale(rectTransform, Vector3.zero, .2f).setEase(LeanTweenType.easeOutExpo).setOnComplete(
                                () =>
                                {
                                    this.gameObject.SetActive(false);
                                  //  Debug.Log("A");
                                });
                            break;
                        }
                    case EffectType.Alpha:
                        {
                            LeanTween.alpha(rectTransform, 0, .2f).setOnComplete(() =>
                            {
                                this.gameObject.SetActive(false);
                             //   Debug.Log("B");
                            });
                            break;
                        }
                    case EffectType.None:
                        {
                            gameObject.SetActive(false);
                            break;
                        }
                }
            }
        }
    }
}
