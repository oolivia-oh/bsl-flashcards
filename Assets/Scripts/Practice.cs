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
    private VisualElement wtsRoot;
    public Button backButton;
    private Label statsLabel;
    private List<Sign> signs;
    private VideoPlayer videoPlayer;
    public RenderTexture videoTexture;
    private ToggleButton wtsToggle;
    private TextField answerField;
    private VisualElement wtsVideoContainer;
    private Label questionLabel;
    private Button checkButton;
    private Sign currentSign;
    private VisualElement correctRoot;
    private bool isChecking = false;
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
        wtsToggle.clicked += UpdateStats;
        Button goButton = new Button();
        goButton.text = "GO";
        goButton.clicked += ConfigToPractice;
        statsLabel = new Label();
        UpdateStats();
        configRoot = new VisualElement();
        configRoot.Add(backButton);
        configRoot.Add(wtsToggle);
        configRoot.Add(statsLabel);
        configRoot.Add(goButton);
    }

    void InitStw() {
        videoPlayer = GetComponent<VideoPlayer>();
        VisualElement videoContainer = new VisualElement();
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
    }

    void InitWts() {
        videoPlayer = GetComponent<VideoPlayer>();
        wtsVideoContainer = new VisualElement();
        wtsVideoContainer.style.width = 640;
        wtsVideoContainer.style.height = 480;
        wtsVideoContainer.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(videoTexture));
        wtsVideoContainer.visible = false;
        questionLabel = new Label();
        checkButton = new Button();
        checkButton.text = "Check";
        checkButton.clicked += OnCheckPressed;
        Button correctButton = new Button();
        correctButton.text = "Correct";
        correctButton.clicked += OnCorrectPressed;
        Button incorrectButton = new Button();
        incorrectButton.text = "Incorrect";
        incorrectButton.clicked += OnIncorrectPressed;
        correctRoot = new VisualElement();
        correctRoot.style.flexDirection = FlexDirection.Row;
        correctRoot.Add(correctButton);
        correctRoot.Add(incorrectButton);
        wtsRoot = new VisualElement();
        wtsRoot.Add(wtsVideoContainer);
        wtsRoot.Add(questionLabel);
        wtsRoot.Add(checkButton);
    }

    void InitPractice() {
        Button backButton = new Button();
        backButton.text = "x";
        backButton.clicked += BackToConfig;
        InitStw();
        InitWts();
        practiceRoot = new VisualElement();
        practiceRoot.Add(backButton);
        waitTimeLabel = new Label();
    }

    void ConfigToPractice() {
        root.Remove(configRoot);
        root.Add(practiceRoot);
        if (canNextLearn == 0) {
            if (wtsToggle.on) {
                practiceRoot.Add(wtsRoot);
            } else {
                practiceRoot.Add(stwRoot);
            }
        }
        LoadNextSign();
    }
    void BackToConfig() {
        if (canNextLearn == 0) {
            if (wtsToggle.on) {
                practiceRoot.Remove(wtsRoot);
            } else {
                practiceRoot.Remove(stwRoot);
            }
        }
        root.Remove(practiceRoot);
        root.Add(configRoot);
        UpdateStats();
    }

    void UpdateStats() {
        int canLearnN = 0;
        foreach (Sign sign in signs) {
            long waitTime = sign.LevelUpWaitTime(wtsToggle.on);
            if (waitTime <= 0) {
                canLearnN++;
            }
        }
        statsLabel.text = $"{canLearnN} signs to learn";
    }

    Sign NextSign() { // TODO check if learning
        Sign nextSign = null;
        long minWaitTime = signs[0].LevelUpWaitTime(wtsToggle.on);
        List<Sign> canLearnSigns = new List<Sign>();
        foreach (Sign sign in signs) {
            long waitTime = sign.LevelUpWaitTime(wtsToggle.on);
            if (waitTime <= 0) {
                canLearnSigns.Add(sign);
            }
            if (waitTime < minWaitTime) {
                minWaitTime = waitTime;
            }
        }
        if (canLearnSigns.Count > 0) {
            nextSign = canLearnSigns[UnityEngine.Random.Range(0, canLearnSigns.Count)];
        } else {
            canNextLearn = DateTime.Now.Ticks/10000000 + minWaitTime;
        }
        return nextSign;
    }

    void LoadNextSign() {
        videoPlayer.Stop();
        currentSign = NextSign();
        if (currentSign == null) {
            if (wtsToggle.on) practiceRoot.Remove(wtsRoot);
            else              practiceRoot.Remove(stwRoot);
            practiceRoot.Add(waitTimeLabel);
        } else {
            if (canNextLearn != 0) {
                practiceRoot.Remove(waitTimeLabel);
                if (wtsToggle.on) practiceRoot.Add(wtsRoot);
                else              practiceRoot.Add(stwRoot);
                canNextLearn = 0;
            }
            videoPlayer.url = currentSign.url;
            if (wtsToggle.on) {
                videoPlayer.Prepare();
                questionLabel.text = currentSign.word;
            } else {
                videoPlayer.Play();
            }
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
        if (answerField.value == currentSign.word) {
            if (!answerIncorrect) {
                currentSign.stwCorrectTicks.Add(DateTime.Now.Ticks/10000000);
                Menu.SaveSigns(signs);
            }
            Debug.Log($"Correct! sign at fluency level: {currentSign.FluencyLevel(wtsToggle.on)}");
            LoadNextSign();
        } else if (!answerIncorrect) {
            currentSign.stwIncorrectTicks.Add(DateTime.Now.Ticks/10000000);
            answerIncorrect = true;
            Menu.SaveSigns(signs);
        }
    }

    void OnCheckPressed() {
        wtsRoot.Remove(checkButton);
        wtsRoot.Add(correctRoot);
        wtsVideoContainer.visible = true;
        videoPlayer.Play();
    }

    void BackToQuestion() {
        wtsRoot.Remove(correctRoot);
        wtsRoot.Add(checkButton);
        wtsVideoContainer.visible = false;
        LoadNextSign();
    }
    void OnCorrectPressed() {
        currentSign.wtsCorrectTicks.Add(DateTime.Now.Ticks/10000000);
        Menu.SaveSigns(signs);
        BackToQuestion();
    }
    void OnIncorrectPressed() {
        currentSign.wtsIncorrectTicks.Add(DateTime.Now.Ticks/10000000);
        Menu.SaveSigns(signs);
        BackToQuestion();
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
            Assert.AreEqual(sign.FluencyLevel(false), 0); TestRan();
            sign.stwCorrectTicks.Add(10);
            Assert.AreEqual(sign.FluencyLevel(false), 0); TestRan();
            sign.stwCorrectTicks.Add(40);
//            sign.stwCorrectTicks.Add(60);
            sign.stwCorrectTicks.Add(120);
            Assert.AreEqual(sign.FluencyLevel(false), 1); TestRan();
            sign.stwCorrectTicks.Add(480);
 //           sign.stwCorrectTicks.Add(880);
            sign.stwCorrectTicks.Add(1280);
            Assert.AreEqual(sign.FluencyLevel(false), 2); TestRan();
        }
        { // test 2
            Sign sign = new Sign();
            Assert.IsTrue(sign.LevelUpWaitTime(false) <= 0); TestRan();
            sign.stwCorrectTicks.Add(now-10);
            Assert.IsTrue(sign.LevelUpWaitTime(false) <= 0); TestRan();
        }
        { // test 3
            Sign sign = new Sign();
            sign.stwCorrectTicks.Add(now-60);
            sign.stwCorrectTicks.Add(now-40);
            Assert.IsTrue(sign.LevelUpWaitTime(false) <= 0); TestRan();
            sign.stwCorrectTicks.Add(now-10);
            Assert.IsTrue(sign.LevelUpWaitTime(false) > 0); TestRan();
        }
      //List<Sign> signs = new List<Sign>;
      //for (int i = 0; i < 10; i++) {
      //    signs.word = $"{i}";
      //    signs.learning = true;
      //}
        Debug.Log("All tests pass!");
    }
}
