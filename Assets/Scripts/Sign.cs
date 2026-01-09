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

    public List<long> TickGaps(List<long> ticks) {
        List<long> gaps = new List<long>();
        for (int i = 1; i < ticks.Count; i++) {
            gaps.Add(ticks[i] - ticks[i-1]);
        }
        return gaps;
    }

    public List<float> LogGaps(List<long> gaps) {
        List<float> logGaps = new List<float>();
        foreach (float gap in gaps) {
            logGaps.Add(Mathf.Log(gap, 15));
        }
        return logGaps;
    }

    public int StwFluencyLevel() {
        int fluencyLevel = 0;
        int nLevels = 10;
        int levelCountThreshold = 2;
        List<float> logGaps = LogGaps(TickGaps(stwCorrectTicks));
        List<int> levels = new List<int>();
        for (int i = 0; i < nLevels; i++) {
            levels.Add(0);
        }
        foreach (float gap in logGaps) {
            for (int i = 0; i < nLevels; i++) {
                if (gap > i) {
                    levels[i]++;
                    if (levels[i] >= levelCountThreshold && fluencyLevel < i) {
                        fluencyLevel = i;
                    }
                }
            }
        }
        return fluencyLevel;
    }

    public long StwLevelUpWaitTime() {
        long waitTime = 0;
        if (stwCorrectTicks.Count > 1) {
            long gap = DateTime.Now.Ticks/10000000 - stwCorrectTicks[stwCorrectTicks.Count-1];
            float logGap = Mathf.Log(gap, 15);
            int level = StwFluencyLevel();
            waitTime = (long)Mathf.Pow(15, level + 1) - gap;
        }
        return waitTime;
    }


    // need a should learn next.
    // overwhelm stat
    // mastery stat
}
