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
        statsLabel.text = $"Signs: {statSigns.Count}, Learning: {nLearning(statSigns)}";
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
