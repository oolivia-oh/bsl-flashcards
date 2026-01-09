using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToggleButton : Button {
    private bool _on;
    private Label label;
    private Box box;

    public bool on {
        get { return _on; }
        set {
            _on = value;
            if (_on) box.visible = true;
            else     box.visible = false;
        }
    }
    public ToggleButton(bool in_on=false) : base() {
        clicked += Toggle;
        style.flexDirection = FlexDirection.Row;
        label = new Label();
        box = new Box();
        Add(label);
        Add(box);
        box.style.width = 30; // FIXME
        on = in_on;
    }

    public void Toggle() {
        if (on) on = false;
        else    on = true;
    }

    public override string text {
        get { return label.text; }
        set { label.text = value; }
    }
}
