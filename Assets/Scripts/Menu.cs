using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Menu : MonoBehaviour {

    private VisualElement documentRoot;
    private VisualElement menuRoot;
    private Button signsButton;
    private Button practiceButton;
    private SignsMenu signsMenu;
    private VisualElement signsMenuRoot;
    public Practice practice;
    private VisualElement practiceRoot;
    private List<Sign> signs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        signs = new List<Sign>();
        LoadSigns(ref signs);
        // load signs
        documentRoot = GetComponent<UIDocument>().rootVisualElement;
        menuRoot = new VisualElement();
        signsMenuRoot = new VisualElement();
        practiceRoot = new VisualElement();
        signsButton = new Button();
        signsButton.text = "Signs";
        signsButton.RegisterCallback<PointerUpEvent>(OnSignsButtonPressed);
        practiceButton = new Button();
        practiceButton.text = "Practice";
        practiceButton.RegisterCallback<PointerUpEvent>(OnPracticeButtonPressed);
        menuRoot.Add(signsButton);
        menuRoot.Add(practiceButton);
        signsMenu = new SignsMenu(signsMenuRoot, signs);
        signsMenu.backButton.RegisterCallback<PointerUpEvent>(OnSignsMenuBack);
        practice.Init(practiceRoot, signs);
        practice.backButton.RegisterCallback<PointerUpEvent>(OnPracticeBack);
        // TODO set all submenus absolute position 0
        documentRoot.Add(menuRoot);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static string GetSavePath(string filename) {
        return Application.persistentDataPath + "/" + filename + ".json";
    }

    public static void SaveSigns(List<Sign> signs) {
        string json = JsonConvert.SerializeObject(signs, Formatting.Indented);
        System.IO.File.WriteAllText(GetSavePath("signs"), json);
    }

    public static void LoadSigns(ref List<Sign> signs) {
        string path = GetSavePath("signs");
        if (System.IO.File.Exists(path)) {
            string json = System.IO.File.ReadAllText(path);
            signs = JsonConvert.DeserializeObject<List<Sign>>(json);
        }
    }


    void OnSignsButtonPressed(PointerUpEvent evt) {
        documentRoot.Remove(menuRoot);
        documentRoot.Add(signsMenuRoot);
        signsMenu.Show();
    }
    void OnSignsMenuBack(PointerUpEvent evt) {
        signsMenu.Hide();
        documentRoot.Remove(signsMenuRoot);
        documentRoot.Add(menuRoot);
    }

    void OnPracticeButtonPressed(PointerUpEvent evt) {
        documentRoot.Remove(menuRoot);
        documentRoot.Add(practiceRoot);
        practice.Show();
    }
    void OnPracticeBack(PointerUpEvent evt) {
        practice.Hide();
        documentRoot.Remove(practiceRoot);
        documentRoot.Add(menuRoot);
    }
}
