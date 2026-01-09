using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System;

public class Practice : MonoBehaviour {

    private VisualElement root;
    private VisualElement configRoot;
    private VisualElement practiceRoot;
    private VisualElement stwRoot;
    public Button backButton;
    private List<Sign> signs;
    private VideoPlayer videoPlayer;
    public RenderTexture videoTexture;
    private VisualElement videoContainer;
    private ToggleButton wtsToggle;
    private TextField answerField;
    private Sign currentSign;
    private bool answerIncorrect = false;
    private Label waitTimeLabel;
    private long canNextLearn;

    InputSystem_Actions input;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init(VisualElement in_root, List<Sign> in_signs) {
        Test();
        root = in_root;
        signs = in_signs;
        InitConfig();
        InitPractice();
        root.Add(configRoot);
        input = new InputSystem_Actions();
    }

    void InitConfig() {
        backButton = new Button();
        backButton.text = "x";
        wtsToggle = new ToggleButton();
        wtsToggle.text = "Word to sign";
        Button goButton = new Button();
        goButton.text = "GO";
        goButton.clicked += ConfigToPractice;
        configRoot = new VisualElement();
        configRoot.Add(backButton);
        configRoot.Add(wtsToggle);
        configRoot.Add(goButton);
    }

    void InitPractice() {
        Button backButton = new Button();
        backButton.text = "x";
        backButton.clicked += BackToConfig;
        videoPlayer = GetComponent<VideoPlayer>();
        videoContainer = new VisualElement();
        videoContainer.style.width = 640;
        videoContainer.style.height = 480;
        videoContainer.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(videoTexture));
        answerField = new TextField();
        Button submitButton = new Button();
        submitButton.text = "Submit";
        submitButton.clicked += Submit;
        stwRoot = new VisualElement();
        stwRoot.Add(videoContainer);
        stwRoot.Add(answerField);
        stwRoot.Add(submitButton);
        practiceRoot = new VisualElement();
        practiceRoot.Add(backButton);
        practiceRoot.Add(stwRoot);
        waitTimeLabel = new Label();
    }

    void ConfigToPractice() {
        root.Remove(configRoot);
        root.Add(practiceRoot);
        LoadNextSign();
    }
    void BackToConfig() {
        root.Remove(practiceRoot);
        root.Add(configRoot);
    }

    Sign NextSign() { // TODO check if learning
        Sign nextSign = signs[0]; // TODO 0 signs case
        if (currentSign == nextSign) nextSign = signs[1];
        bool canLearn = false;
        long minWaitTime = signs[0].StwLevelUpWaitTime();
        foreach (Sign sign in signs) {
            long waitTime = sign.StwLevelUpWaitTime();
            if (waitTime <= 0) {
                canLearn = true;
                nextSign = sign;
            }
            if (waitTime < minWaitTime) {
                minWaitTime = waitTime;
            }
        }
        foreach (Sign sign in signs) {
            int nsctCount = nextSign.stwCorrectTicks.Count;
            int sctCount = sign.stwCorrectTicks.Count;
            if (nsctCount != 0) {
                if (sctCount == 0) {
                    nextSign = sign;
                } else if (nextSign.stwCorrectTicks[nsctCount-1] > sign.stwCorrectTicks[sctCount-1] && sign.StwLevelUpWaitTime() <= 0) {
                    nextSign = sign;
                }
            }
        }
        if (!canLearn) {
            nextSign = null;
            canNextLearn = DateTime.Now.Ticks/10000000 + minWaitTime;
        }
        return nextSign;
    }

    void LoadNextSign() {
        videoPlayer.Stop();
        currentSign = NextSign();
        if (currentSign == null) {
            practiceRoot.Remove(stwRoot);
            practiceRoot.Add(waitTimeLabel);
        } else {
            if (canNextLearn != 0) {
                practiceRoot.Remove(waitTimeLabel);
                practiceRoot.Add(stwRoot);
                canNextLearn = 0;
            }
            videoPlayer.url = currentSign.url;
            videoPlayer.Play();
            answerIncorrect = false;
            answerField.value = "";
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (DateTime.Now.Ticks/10000000 > canNextLearn && canNextLearn != 0) {
            LoadNextSign();
        } else {
            long waitTime = canNextLearn - DateTime.Now.Ticks/10000000;
            waitTimeLabel.text = $"Finished practicing for now come back in {waitTime}s";
        }
    }

    void OnSubmit(InputAction.CallbackContext callbackContext) {
        Submit();
    }
    void Submit() {
        if (wtsToggle.on) {
        } else {
            if (answerField.value == currentSign.word) {
                if (!answerIncorrect) {
                    currentSign.stwCorrectTicks.Add(DateTime.Now.Ticks/10000000);
                    Menu.SaveSigns(signs);
                }
                Debug.Log($"Correct! sign at fluency level: {currentSign.StwFluencyLevel()}");
                LoadNextSign();
            } else if (!answerIncorrect) {
                currentSign.stwIncorrectTicks.Add(DateTime.Now.Ticks/10000000);
                answerIncorrect = true;
                Menu.SaveSigns(signs);
            }
        }
    }

    public void Show() {
        videoPlayer.enabled = true;
        //input.UI.Submit.performed += OnSubmit;
    }

    public void Hide() {
        videoPlayer.Stop();
        videoPlayer.enabled = false;
        //input.UI.Submit.performed -= OnSubmit;
    }

    public void Test() {
        int testN = 0;
        void TestRan() {
            Debug.Log($"Test {testN} passed");
            testN++;
        }
        long now = DateTime.Now.Ticks/10000000;

        { // test 1
            Sign sign = new Sign();
            Assert.AreEqual(sign.StwFluencyLevel(), 0); TestRan();
            sign.stwCorrectTicks.Add(10);
            Assert.AreEqual(sign.StwFluencyLevel(), 0); TestRan();
            sign.stwCorrectTicks.Add(40);
//            sign.stwCorrectTicks.Add(60);
            sign.stwCorrectTicks.Add(120);
            Assert.AreEqual(sign.StwFluencyLevel(), 1); TestRan();
            sign.stwCorrectTicks.Add(480);
 //           sign.stwCorrectTicks.Add(880);
            sign.stwCorrectTicks.Add(1280);
            Assert.AreEqual(sign.StwFluencyLevel(), 2); TestRan();
        }
        { // test 2
            Sign sign = new Sign();
            Assert.IsTrue(sign.StwLevelUpWaitTime() <= 0); TestRan();
            sign.stwCorrectTicks.Add(now-10);
            Assert.IsTrue(sign.StwLevelUpWaitTime() <= 0); TestRan();
        }
        { // test 3
            Sign sign = new Sign();
            sign.stwCorrectTicks.Add(now-60);
            sign.stwCorrectTicks.Add(now-40);
            Assert.IsTrue(sign.StwLevelUpWaitTime() <= 0); TestRan();
            sign.stwCorrectTicks.Add(now-10);
            Assert.IsTrue(sign.StwLevelUpWaitTime() > 0); TestRan();
        }
      //List<Sign> signs = new List<Sign>;
      //for (int i = 0; i < 10; i++) {
      //    signs.word = $"{i}";
      //    signs.learning = true;
      //}
        Debug.Log("All tests pass!");
    }
}
