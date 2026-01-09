using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System;

public class Practice : MonoBehaviour {

    private VisualElement root;
    private VisualElement configRoot;
    private VisualElement practiceRoot;
    public Button backButton;
    private List<Sign> signs;
    private VideoPlayer videoPlayer;
    public RenderTexture videoTexture;
    private VisualElement videoContainer;
    private ToggleButton wtsToggle;
    private TextField answerField;
    private Sign currentSign;
    private bool answerIncorrect = false;

    InputSystem_Actions input;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init(VisualElement in_root, List<Sign> in_signs) {
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
        practiceRoot = new VisualElement();
        practiceRoot.Add(backButton);
        practiceRoot.Add(videoContainer);
        practiceRoot.Add(answerField);
        practiceRoot.Add(submitButton);
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
        foreach (Sign sign in signs) {
            int nsctCount = nextSign.stwCorrectTicks.Count;
            int sctCount = sign.stwCorrectTicks.Count;
            if (nsctCount != 0) {
                if (sctCount == 0) {
                    nextSign = sign;
                } else if (nextSign.stwCorrectTicks[nsctCount-1] > sign.stwCorrectTicks[sctCount-1]) {
                    nextSign = sign;
                }
            }
        }
        return nextSign;
    }

    void LoadNextSign() {
        videoPlayer.Stop();
        currentSign = NextSign();
        videoPlayer.url = currentSign.url;
        videoPlayer.Play();
        answerIncorrect = false;
        answerField.value = "";
    }

    // Update is called once per frame
    void OnSubmit(InputAction.CallbackContext callbackContext) {
        Submit();
    }
    void Submit() {
        if (wtsToggle.on) {
        } else {
            if (answerField.value == currentSign.word) {
                if (!answerIncorrect) {
                    currentSign.stwCorrectTicks.Add(DateTime.Now.Ticks);
                    Menu.SaveSigns(signs);
                }
                LoadNextSign();
            } else if (!answerIncorrect) {
                currentSign.stwIncorrectTicks.Add(DateTime.Now.Ticks);
                answerIncorrect = true;
                Menu.SaveSigns(signs);
            }
        }
    }

    public void Show() {
        videoPlayer.enabled = true;
        videoPlayer.url = "https://media.signbsl.com/videos/bsl/signstation/smell.mp4";
        videoPlayer.Play();
        //input.UI.Submit.performed += OnSubmit;
    }

    public void Hide() {
        videoPlayer.Stop();
        videoPlayer.enabled = false;
        //input.UI.Submit.performed -= OnSubmit;
    }
}
