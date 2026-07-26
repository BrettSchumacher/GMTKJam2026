using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class MoneyDisplayer : MonoBehaviour
{

    public TextMeshProUGUI textObj;

    public int currentMoney = 1000;
    public int currencyCounterAnimationFrameRate = 30;
    public float currencyCounterAnimationMaxDuration = 2;

    Coroutine moneyCoroutine;

    private void Start()
    {
        currentMoney = (int) TradingSystem.Instance.GetCurrentTradableItem().Value;
        SetDisplayText();
    }

    public void ChangeValue(int newValue)
    {
        if (moneyCoroutine != null)
        {
            StopCoroutine(moneyCoroutine);
        }

        moneyCoroutine = StartCoroutine(IncomeValueChanger(newValue));
    }
    public static string FormatNumber(float num)
    {
        // Ensure number has max 3 significant digits (no rounding up can happen)
        if (num > 0)
        {
            long i = (long)Mathf.Pow(10, (int)Mathf.Max(0, Mathf.Log10(num) - 2));
            num = num / i * i;
        }

        //if (num >= 1000000000000000)
        //    return num.ToString("E", CultureInfo.InvariantCulture);
        //if (num >= 1000000000000)
        //    return (num / 1000000000000D).ToString("0.##") + "T";
        //if (num >= 1000000000)
        //    return (num / 1000000000D).ToString("0.##") + "B";
        //if (num >= 1000000)
        //    return (num / 1000000D).ToString("0.##") + "M";
        //if (num >= 1000)
        //    return (num / 1000D).ToString("0.##") + "K";

        return String.Format("{0:C}", num);
    }

    //TODO: Fix this
    private IEnumerator IncomeValueChanger(int newValue)
    {
        WaitForSeconds Wait = new WaitForSeconds(1f / currencyCounterAnimationFrameRate);
        int difference = currentMoney - newValue;
        float numSteps = currencyCounterAnimationFrameRate * currencyCounterAnimationMaxDuration;
        int stepAmount;

        if (difference < 0)
        {
            stepAmount = Mathf.FloorToInt(difference / (float)numSteps);
        }
        else
        {
            stepAmount = Mathf.CeilToInt(difference / (float)numSteps);
        }

        stepAmount = Mathf.Abs(stepAmount); //WARNING

        if (difference < 0)
        {
            while (currentMoney < newValue)
            {
                currentMoney -= stepAmount;
                if (currentMoney >= newValue)
                {
                    currentMoney = newValue;
                    break;
                }

                SetDisplayText();
                yield return Wait;
            }
        }
        else
        {
            while (currentMoney > newValue)
            {
                currentMoney -= stepAmount;
                if (currentMoney <= newValue)
                {
                    currentMoney = newValue;
                    break;
                }

                SetDisplayText();
                yield return null;
            }
        }

        SetDisplayText();
        moneyCoroutine = null;
    }

    private void SetDisplayText()
    {
        textObj.text = "Net worth: " + FormatNumber(currentMoney);
    }
}
