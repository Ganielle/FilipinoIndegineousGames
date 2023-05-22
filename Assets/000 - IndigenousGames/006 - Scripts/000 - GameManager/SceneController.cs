using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MyBox;

public class SceneController : MonoBehaviour
{
    //  ===========================================

    private event EventHandler sceneChange;
    public event EventHandler onSceneChange
    {
        add
        {
            if (sceneChange == null || !sceneChange.GetInvocationList().Contains(value))
                sceneChange += value;
        }
        remove { sceneChange -= value; }
    }
    public string CurrentScene
    {
        get => currentScene;
        set
        {
            if (currentScene != "")
                previousScene = currentScene;

            currentScene = value;
            sceneChange?.Invoke(this, EventArgs.Empty);
        }
    }
    public string LastScene
    {
        get => previousScene;
        private set => previousScene = value;
    }

    public bool ActionPass
    {
        get => actionPass;
        set => actionPass = value;
    }

    public List<IEnumerator> GetActionLoadingList
    {
        get => actionLoading;
    }
    public void AddActionLoadinList(IEnumerator action)
    {
        actionLoading.Add(action);
    }

    private event EventHandler MultiplayerSceneChange;
    public event EventHandler OnMultuplayerSceneChange
    {
        add
        {
            if (MultiplayerSceneChange == null || !MultiplayerSceneChange.GetInvocationList().Contains(value))
                MultiplayerSceneChange += value;
        }
        remove { MultiplayerSceneChange -= value; }
    }
    public bool MultiplayerScene
    {
        get => multiplayerScene;
        set
        {
            multiplayerScene = value;
            MultiplayerSceneChange?.Invoke(this, EventArgs.Empty);
        }
    }

    //  ===========================================

    [SerializeField] private List<GameObject> loadingObjs;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private CanvasGroup loadingCG;

    [Header("LEANTWEEN ANIMATION")]
    [SerializeField] private LeanTweenType easeType;
    [SerializeField] private float speed;
    [SerializeField] private float loadingBarSpeed;

    [Header("DEBUGGER")]
    [ReadOnly] [SerializeField] private string currentScene;
    [ReadOnly] [SerializeField] private string previousScene;
    [ReadOnly] [SerializeField] private bool multiplayerScene;
    [ReadOnly] [SerializeField] private bool actionPass;
    [ReadOnly] [SerializeField] private float totalSceneProgress;
    [ReadOnly] [SerializeField] private bool splashOver;
    [ReadOnly][SerializeField] private float currentAspectRatio;
    [ReadOnly][SerializeField] private float orthographicSize;

    //  ============================================

    private List<IEnumerator> actionLoading = new List<IEnumerator>();
    AsyncOperation scenesLoading = new AsyncOperation();

    //  ============================================

    private void Awake()
    {
        //StartCoroutine(DisplaySplash());
        //ScaleLoading();
        onSceneChange += SceneChange;
        OnMultuplayerSceneChange += MultiplayerChangeScene;
    }

    private void OnDisable()
    {
        onSceneChange -= SceneChange;
        OnMultuplayerSceneChange -= MultiplayerChangeScene;
    }

    private void MultiplayerChangeScene(object sender, EventArgs e)
    {
        StartCoroutine(MultiplayerLoading());
    }

    private void SceneChange(object sender, EventArgs e)
    {
        StartCoroutine(Loading());
    }

    //private void ScaleLoading()
    //{
    //    currentAspectRatio = (float)Screen.width / Screen.height;

    //    // calculate the adjusted orthographic size based on the aspect ratio difference
    //    orthographicSize = mainCamera.orthographicSize;

    //    if (currentAspectRatio < targetAspectRatio)
    //    {
    //        orthographicSize *= currentAspectRatio / targetAspectRatio;
    //        loadingBGRT.sizeDelta = new Vector3(loadingBGRT.sizeDelta.x, loadingBGRT.sizeDelta.y);
    //    }
    //    else
    //    {
    //        orthographicSize *= targetAspectRatio / currentAspectRatio;
    //        loadingBGRT.sizeDelta = new Vector3(loadingBGRT.localScale.x, loadingBGRT.localScale.y + 80);
    //    }
    //}

    public IEnumerator MultiplayerLoading()
    {
        if (!MultiplayerScene) yield break;
        //Time.timeScale = 0f;

        actionPass = false;

        loadingSlider.value = 0f;

        for (int a = 0; a < loadingObjs.Count; a++)
        {
            loadingObjs[a].SetActive(true);

            yield return null;
        }

        LeanTween.alphaCanvas(loadingCG, 1f, speed).setEase(easeType);

        yield return new WaitWhile(() => loadingCG.alpha != 1f);

        while (!actionPass) yield return null;

        for (int a = 0; a < GetActionLoadingList.Count; a++)
        {
            yield return StartCoroutine(GetActionLoadingList[a]);

            int index = a + 1;

            totalSceneProgress = (float)index / (1 + GetActionLoadingList.Count);

            LeanTween.value(loadingSlider.gameObject, a => loadingSlider.value = a, loadingSlider.value, totalSceneProgress, loadingBarSpeed).setEase(easeType);

            yield return new WaitWhile(() => loadingSlider.value != totalSceneProgress);

            yield return null;
        }

        totalSceneProgress = scenesLoading.progress;

        LeanTween.value(loadingSlider.gameObject, a => loadingSlider.value = a, loadingSlider.value, totalSceneProgress, loadingBarSpeed).setEase(easeType);

        yield return new WaitForSecondsRealtime(loadingBarSpeed);

        LeanTween.alphaCanvas(loadingCG, 0f, speed).setEase(easeType);

        yield return new WaitWhile(() => loadingCG.alpha != 0f);
        for (int a = 0; a < loadingObjs.Count; a++)
        {
            loadingObjs[a].SetActive(false);

            yield return null;
        }

        GetActionLoadingList.Clear();

        loadingSlider.value = 0f;

        totalSceneProgress = 0f;

        //Time.timeScale = 1f;

        MultiplayerScene = false;
    }

    public IEnumerator Loading()
    {
        /*yield return new WaitUntil(() => splashOver);
        backgroundImage.sprite = backgroundSprites[UnityEngine.Random.Range(0, backgroundSprites.Count)];
        triviaImage.sprite = triviaSprites[UnityEngine.Random.Range(0, triviaSprites.Count)];*/

        Time.timeScale = 0f;

        loadingSlider.value = 0f;

        for (int a = 0; a < loadingObjs.Count; a++)
        {
            loadingObjs[a].SetActive(true);

            yield return null;
        }

        LeanTween.alphaCanvas(loadingCG, 1f, speed).setEase(easeType);

        yield return new WaitWhile(() => loadingCG.alpha != 1f);

        scenesLoading = SceneManager.LoadSceneAsync(CurrentScene, LoadSceneMode.Single);

        scenesLoading.allowSceneActivation = false;

        while (!scenesLoading.isDone)
        {
            if (scenesLoading.progress >= 0.9f)
            {
                scenesLoading.allowSceneActivation = true;

                break;
            }

            yield return null;
        }

        while (!actionPass) yield return null;

        actionPass = false; //  THIS IS FOR RESET

        if (GetActionLoadingList.Count > 0)
        {
            for (int a = 0; a < GetActionLoadingList.Count; a++)
            {
                yield return StartCoroutine(GetActionLoadingList[a]);

                int index = a + 1;

                totalSceneProgress = (float)index / (1 + GetActionLoadingList.Count);

                LeanTween.value(loadingSlider.gameObject, a => loadingSlider.value = a, loadingSlider.value, totalSceneProgress, loadingBarSpeed).setEase(easeType);

                yield return new WaitWhile(() => loadingSlider.value != totalSceneProgress);

                yield return null;
            }

            totalSceneProgress = scenesLoading.progress;

            LeanTween.value(loadingSlider.gameObject, a => loadingSlider.value = a, loadingSlider.value, totalSceneProgress, loadingBarSpeed).setEase(easeType);
        }
        else
        {
            totalSceneProgress = 1f;

            LeanTween.value(loadingSlider.gameObject, a => loadingSlider.value = a, loadingSlider.value, totalSceneProgress, loadingBarSpeed).setEase(easeType);

            yield return new WaitWhile(() => loadingSlider.value != totalSceneProgress);
        }

        yield return new WaitForSecondsRealtime(loadingBarSpeed);

        LeanTween.alphaCanvas(loadingCG, 0f, speed).setEase(easeType);

        yield return new WaitWhile(() => loadingCG.alpha != 0f);

        for (int a = 0; a < loadingObjs.Count; a++)
        {
            loadingObjs[a].SetActive(false);

            yield return null;
        }

        GetActionLoadingList.Clear();

        loadingSlider.value = 0f;

        totalSceneProgress = 0f;

        Time.timeScale = 1f;
    }
}
