using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class showingmoney : MonoBehaviour
{
    public Text txt;

    private static readonly List<string> suffixes = new List<string>
    {
        "", "k", "m", "b", "t", "q", "Q", "s", "S", "o", "n",
        "d", "Ud", "Dd", "Td", "qd", "Qd", "sd", "Sd", "od", "Nd",
        "v", "Uv", "Dv", "Tv", "qv", "Qv", "sv", "Sv", "ov", "Nv",
        "Tg", "UTg", "DTg", "TTg", "qTg", "QTg", "sTg", "STg", "oTg", "NTg",
        "Qd", "UQd", "DQd", "TQd", "qQd", "QQd", "sQd", "SQd", "oQd", "NQd",
        "Sx", "USx", "DSx", "TSx", "qSx", "QSx", "sSx", "SSx", "oSx", "NSx",
        "Sp", "USp", "DSp", "TSp", "qSp", "QSp", "sSp", "SSp", "oSp", "NSp",
        "Oc", "UOc", "DOc", "TOc", "qOc", "QOc", "sOc", "SOc", "oOc", "NOc",
        "No", "UNo", "DNo", "TNo", "qNo", "QNo", "sNo", "SNo", "oNo", "NNo",
        "Ce", "Googol", "Googolplex"
    };

    void Start()
    {
        txt = GetComponent<Text>();
    }

    void Update()
    {
        txt.text = "€" + FormatMoney(MoneyManager.GlobalMoney);
    }

    string FormatMoney(float money)
    {
        if (money < 1000f)
            return money.ToString("0.0");

        int tier = (int)(Mathf.Log10(money) / 3f);
        if (tier >= suffixes.Count)
        {
            // Fallback to scientific notation if too high
            return money.ToString("0.###e+0");
        }

        if (money == float.PositiveInfinity)
        {
            return "∞";
        }

        float scaled = money / Mathf.Pow(1000f, tier);
        string suffix = suffixes[tier];
        return scaled.ToString("0.0") + suffix;
    }
}
