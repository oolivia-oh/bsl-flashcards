using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Sign {
    public string word; // same as filename
    public string url;
    public bool learning = false;
    public List<string> synonyms;
    public List<long> stwCorrectTicks; // stw sign to word
    public List<long> stwIncorrectTicks;
    public List<long> wtsCorrectTicks; // wts word to sign
    public List<long> wtsIncorrectTicks;

    public Sign() {
        synonyms = new List<string>();
        stwCorrectTicks = new List<long>();
        stwIncorrectTicks = new List<long>();
        wtsCorrectTicks = new List<long>();
        wtsIncorrectTicks = new List<long>();
    }


    // need a should learn next.
    // mastery stat
}
