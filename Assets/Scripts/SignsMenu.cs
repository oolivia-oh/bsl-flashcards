using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class SignsMenu {

    private VisualElement root;
    public Button backButton;
    private TextField searchField;
    private VisualElement expandedSignContainer;
    private Label expandedSignLabel;
    private Label expandedInfoLabel;
    private Label statsLabel;
    private ToggleButton learningToggle;
    private Button expandedButton;
    private Sign expandedSign;
    private TextField urlField;
    private VisualElement resultsRoot;
    private List<Sign> signs;

    public SignsMenu(VisualElement in_root, List<Sign> in_signs) {
        root = in_root;
        signs = in_signs;
        searchField = new TextField();
        searchField.RegisterValueChangedCallback(OnSearchChanged);
        statsLabel = new Label();
        UpdateStats(signs);
        InitExpandedSignContainer();
        resultsRoot = new VisualElement();
        backButton = new Button();
        backButton.text = "Back";
        root.Add(searchField);
        root.Add(statsLabel);
        root.Add(resultsRoot);
        root.Add(backButton);
    }

    void InitExpandedSignContainer() {
        expandedSignLabel = new Label();
        expandedInfoLabel = new Label();
        urlField = new TextField();
        Label urlLabel = new Label();
        urlLabel.text = "URL";
        VisualElement urlContainer = new VisualElement();
        urlContainer.style.flexDirection = FlexDirection.Row;
        urlContainer.Add(urlLabel);
        urlContainer.Add(urlField);
        learningToggle = new ToggleButton();
        learningToggle.text = "Learning";
        learningToggle.clicked += OnLearningToggle;
        Button deleteButton = new Button();
        deleteButton.text = "Delete";
        deleteButton.clicked += DeleteExpanded;
        expandedSignContainer = new VisualElement();
        expandedSignContainer.Add(expandedSignLabel);
        expandedSignContainer.Add(expandedInfoLabel);
        expandedSignContainer.Add(urlContainer);
        expandedSignContainer.Add(learningToggle);
        expandedSignContainer.Add(deleteButton);
    }
    
    int nLearning(List<Sign> signs) {
        int learning = 0;
        foreach (Sign sign in signs) {
            if (sign.learning) learning++;
        }
        return learning;
    }

    void UpdateStats(List<Sign> statSigns) {
        int wtsFluencyLevel0 = 0;
        int stwFluencyLevel0 = 0;
        int wtsFluencyLevel1_2 = 0;
        int stwFluencyLevel1_2 = 0;
        int wtsFluencyLevel3_4 = 0;
        int stwFluencyLevel3_4 = 0;
        int wtsFluencyLevel5Plus = 0;
        int stwFluencyLevel5Plus = 0;
        foreach (Sign sign in statSigns) {
            switch (sign.FluencyLevel(true)) {
                case 0:         wtsFluencyLevel0++; break;
                case 1: case 2: wtsFluencyLevel1_2++; break;
                case 3: case 4: wtsFluencyLevel3_4++; break;
                default:        wtsFluencyLevel5Plus++; break;
            }
            switch (sign.FluencyLevel(false)) {
                case 0:         stwFluencyLevel0++; break;
                case 1: case 2: stwFluencyLevel1_2++; break;
                case 3: case 4: stwFluencyLevel3_4++; break;
                default:        stwFluencyLevel5Plus++; break;
            }
        }
        statsLabel.text = $"Signs: {statSigns.Count}, Learning: {nLearning(statSigns)}, Fluency level: WTS 0={wtsFluencyLevel0}, 1-2={wtsFluencyLevel1_2}, 3-4={wtsFluencyLevel3_4}, 5+={wtsFluencyLevel5Plus}; STW 0={stwFluencyLevel0}, 1-2={stwFluencyLevel1_2}, 3-4={stwFluencyLevel3_4}, 5+={stwFluencyLevel5Plus}";
    }

    // Update is called once per frame
    void Update() {
        
    }

    void ClearSearchResults() {
        resultsRoot.Clear();
    }

    List<Sign> SearchSigns(string search) {
        List<Sign> searchResults = new List<Sign>();
        bool exactMatch = false;
        foreach (Sign sign in signs) {
            if (sign.word == search) {
                exactMatch = true;
            }
            if (sign.word.Contains(search)) {
                searchResults.Add(sign);
            }
        }
        UpdateStats(searchResults);
        if (!exactMatch) {
            Sign newSign = new Sign();
            newSign.word = search;
            searchResults.Add(newSign);
        }
        return searchResults;
    }

    void RenderSearchResults(List<Sign> searchResults) {
        foreach (Sign sign in searchResults) {
            Button button = new Button();
            button.text = sign.word;
            button.RegisterCallback<PointerUpEvent>((PointerUpEvent evt) => OnSearchResultPressed(evt, button, sign));
            resultsRoot.Add(button);
        }
    }


    void OnSearchResultPressed(PointerUpEvent evt, Button button, Sign sign) {
        if (expandedButton != null) {
            expandedButton.text = expandedSign.word;
            expandedButton.Clear();
        }
        expandedSignLabel.text = sign.word;
        expandedSignLabel.text = $"Fluency level: WTS={sign.FluencyLevel(true)} STW={sign.FluencyLevel(false)}";
        button.text = "";
        urlField.value = sign.url;
        urlField.RegisterValueChangedCallback(OnUrlChanged);
        learningToggle.on = sign.learning;
        button.Add(expandedSignContainer);
        expandedButton = button;
        expandedSign = sign;
    }

    void OnSearchChanged(ChangeEvent<string> evt) {
        ClearSearchResults();
        if (evt.newValue != "") {
            List<Sign> searchResults = SearchSigns(evt.newValue);
            RenderSearchResults(searchResults);
        } else {
            UpdateStats(signs);
        }
    }

    void OnUrlChanged(ChangeEvent<string> evt) {
        expandedSign.url = evt.newValue;
        if (!signs.Contains(expandedSign)) {
            signs.Add(expandedSign);
            SearchSigns(searchField.value);
        }
        Menu.SaveSigns(signs);
    }

    void DeleteExpanded() { // TODO add "are you sure"
        expandedSign.url = "";
        urlField.value = "";
        learningToggle.on = false;
        signs.Remove(expandedSign);
        if (expandedSign.word != searchField.value) {
            resultsRoot.Remove(expandedButton);
        }
        SearchSigns(searchField.value);
        Menu.SaveSigns(signs);
    }

    void OnLearningToggle() {
        expandedSign.learning = learningToggle.on;
        Menu.SaveSigns(signs);
    }

    public void Show() {
    }

    public void Hide() {
    }
}
