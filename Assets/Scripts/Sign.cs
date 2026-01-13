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

    public int GapToLevel(long gap) {
        int[] levels = {20, 60, 300, 3600};
        int logValue = 12*3600;
        int level = 0;
        for (int i = 0; i < levels.Length; i++) {
            if (gap > levels[i]) {
                level = i+1;
            }
        }
        if (gap > logValue) {
            level = (int)Mathf.Log(gap / (logValue), 2) + levels.Length + 1;
        }
        return level;
    }

    public long GapToWaitTime(int level, long gap) {
        int[] levels = {20, 60, 300, 3600};
        int logValue = 12*3600;
        long nextLevelGap = (long)Mathf.Pow(level - levels.Length, 2) * logValue;
        if (level < levels.Length) {
            nextLevelGap = levels[level];
        }
        return nextLevelGap - gap;
    }

    public List<int> GapLevels(List<long> gaps) {
        List<int> gapLevels = new List<int>();
        foreach (long gap in gaps) {
            gapLevels.Add(GapToLevel(gap));
        }
        return gapLevels;
    }

    public int FluencyLevel(List<long> correctTicks) {
        int fluencyLevel = 0;
        int nLevels = 10;
        int levelCountThreshold = 1;
        List<int> gapLevels = GapLevels(TickGaps(correctTicks));
        List<int> levels = new List<int>();
        for (int i = 0; i < nLevels; i++) {
            levels.Add(0);
        }
        foreach (int level in gapLevels) {
            for (int i = 0; i < nLevels; i++) {
                if (level >= i) {
                    levels[i]++;
                    if (levels[i] >= levelCountThreshold && fluencyLevel < i) {
                        fluencyLevel = i;
                    }
                }
            }
        }
        return fluencyLevel;
    }

    public long LevelUpWaitTime(List<long> correctTicks) {
        long waitTime = 0;
        if (correctTicks.Count > 0) {
            long gap = DateTime.Now.Ticks/10000000 - correctTicks[correctTicks.Count-1];
            waitTime = GapToWaitTime(FluencyLevel(correctTicks), gap);
        }
        return waitTime;
    }

    public int FluencyLevel(bool wts) {
        if (wts) return FluencyLevel(wtsCorrectTicks);
        else return FluencyLevel(stwCorrectTicks);
    }
    public long LevelUpWaitTime(bool wts) {
        if (wts) return LevelUpWaitTime(wtsCorrectTicks);
        else return LevelUpWaitTime(stwCorrectTicks);
    }



    // need a should learn next.
    // overwhelm stat
    // mastery stat
}
