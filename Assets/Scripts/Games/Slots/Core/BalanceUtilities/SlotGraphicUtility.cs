#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Casino;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions.Comparers;
using Random = System.Random;

public class SlotGraphicUtility : EditorWindow
{
    private AnimationCurve curveX = new AnimationCurve();
    private string defaultBetStr = "100";
    private string userBetStr = "100";
    private string startBankStr = "10000";
    private string spinCountStr = "100";
    private string periodRTPStr = "100";
    private string graphCountStr = "1";
    private bool printLogs = false;
    private bool logToFile = true;

    float[] statusesCount = new float[4];
    float[] rewardMiddle = new float[4];

    private const string FILE_NAME = "..\\spins_data.csv";

    [MenuItem("SlotUtility/SpinsGraphCreator")]
    static void Init()
    {
        SlotGraphicUtility window = (SlotGraphicUtility)EditorWindow.GetWindow(typeof(SlotGraphicUtility));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Default bet:   ");
        defaultBetStr = GUILayout.TextArea(defaultBetStr, 10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("User bet:   ");
        userBetStr = GUILayout.TextArea(userBetStr, 10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Start bank:    ");
        startBankStr = GUILayout.TextArea(startBankStr, 10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Spin count:    ");
        spinCountStr = GUILayout.TextArea(spinCountStr, 10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("RTP on period: ");
        periodRTPStr = GUILayout.TextArea(periodRTPStr, 10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Graph count: ");
        graphCountStr = GUILayout.TextArea(graphCountStr, 10);
        GUILayout.EndHorizontal();

        printLogs = GUILayout.Toggle(printLogs, "Print logs");
        logToFile = GUILayout.Toggle(logToFile, "Save logs to file");

        curveX = EditorGUILayout.CurveField("Animation on X", curveX);

        if (GUILayout.Button("Generate Curve") && EditorApplication.isPlaying)
            FillCurve();

        if (GUILayout.Button("Generate random"))
            FillSimpleRandom();

        GUILayout.Label("Status Lose=" + statusesCount[0] + "; loseBets=" + rewardMiddle[0]);
        GUILayout.Label("Status False=" + statusesCount[1] + "; loseBets=" + rewardMiddle[1]);
        GUILayout.Label("Status Lucky=" + statusesCount[2] + "; rewardBets=" + rewardMiddle[2]);
        GUILayout.Label("Status Big=" + statusesCount[3] + "; rewardBets=" + rewardMiddle[3]);
    }

    void FillSimpleRandom()
    {
        while (curveX.keys.Length > 0)
        {
            curveX.RemoveKey(0);
        }

        List<float[]> curves = new List<float[]>();

        int graphCount = Convert.ToInt32(graphCountStr);
        int spinCount = Convert.ToInt32(spinCountStr);

        for (int j = 0; j < graphCount; j++)
        {
            float startSum = 0.0f;
            float[] curCurve = new float[spinCount];
            curCurve[0] = startSum;

            for (int i = 0; i < spinCount - 1; i++)
            {
                startSum += UnityEngine.Random.Range(-50.0f, 50.0f);
                curCurve[i + 1] = startSum;
            }

            curves.Add(curCurve);
        }


        for (int i = 0; i < spinCount; i++)
        {
            float curMoney = 0.0f;
            for (int j = 0; j < graphCount; j++)
            {
                curMoney += curves[j][i];
            }

            curMoney /= (float)graphCount;

            curveX.AddKey(i, curMoney);
        }
    }

    void FillCurve()
    {
        int startMoney = Convert.ToInt32(startBankStr);
        int spinCount = Convert.ToInt32(spinCountStr);
        int graphCount = Convert.ToInt32(graphCountStr);
        float rtp = Convert.ToSingle(periodRTPStr);
        float defaultBet = Convert.ToSingle(defaultBetStr);
        float userBet = Convert.ToSingle(userBetStr);

        while (curveX.keys.Length > 0)
        {
            curveX.RemoveKey(0);
        }

        for (int i = 0; i < statusesCount.Length; i++)
        {
            statusesCount[i] = 0.0f;
            rewardMiddle[i] = 0.0f;
        }

        StreamWriter fileLog = null;
        if (logToFile)
        {
            if (File.Exists(FILE_NAME))
                File.Delete(FILE_NAME);
            fileLog = File.AppendText(FILE_NAME);
            fileLog.WriteLine(SpinInfo.PrintCSVHeaders());
        }

        List<float[]> curves = new List<float[]>();

        int startSpinId = 0;

        for (int j = 0; j < graphCount; j++)
        {
            float[] curCurve = new float[spinCount];

            PeriodData newData = new PeriodData();
            newData.curRTP = rtp;
            newData.startSpinId = DataManager.Instance.SlotsData.lastSpinId;
            newData.playerBankOnStart = startMoney;
            newData.finished = false;
            newData.isFirstPeriod = true;
            newData.isPositiveTrend = rtp >= 100.0f;

            DataManager.Instance.SetMoney(startMoney);
            DataManager.Instance.SetNewPeriod(newData);

            for (int i = 0; i < spinCount; i++)
            {
                SlotsGame.Instance.CurrentBet = Convert.ToInt32(userBet);

                SpinInfo spinInfo = SlotsGame.Instance.Generator.GenerateSpin();

                if (startSpinId == 0)
                    startSpinId = spinInfo.id;

                statusesCount[(int) spinInfo.finalStatus - 1] += 1.0f;
                DataManager.Instance.SpendMoney(SlotsGame.Instance.CurrentBet);

                if (spinInfo.finalStatus != SpinStatus.Lose)
                {
                    DataManager.Instance.AddMoney(spinInfo.moneyReward);
                }

                if (printLogs)
                {
                    Debug.Log("New spin: " + spinInfo.ToString());
                }

                if (logToFile && fileLog != null)
                {
                    fileLog.WriteLine(spinInfo.ToCSV());
                }

                if (spinInfo.finalStatus == SpinStatus.Lose)
                {
                    rewardMiddle[0] -= 1.0f;
                }
                else if (spinInfo.finalStatus == SpinStatus.False)
                {
                    rewardMiddle[1] -= 1.0f - spinInfo.finalRewardPercent;
                }
                else if (spinInfo.finalStatus == SpinStatus.Lucky)
                {
                    rewardMiddle[2] += spinInfo.finalRewardPercent;
                }
                else if (spinInfo.finalStatus == SpinStatus.Big)
                {
                    rewardMiddle[3] += spinInfo.finalRewardPercent;
                }

                curCurve[i] = DataManager.Instance.Money;
            }

            curves.Add(curCurve);
        }

        for (int i = 0; i < rewardMiddle.Length; i++)
        {
            rewardMiddle[i] /= statusesCount[i];
        }

        for (int i = 0; i < statusesCount.Length; i++)
        {
            statusesCount[i] /= (float)graphCount;
        }

        /*
        var slotConfig = SlotsGame.Instance.SlotMachineConfig;
        float loseBets = statusesCount[0] + statusesCount[1] * (100.0f - slotConfig.rewardLimits.MinFalseReward) * 0.5f / 100.0f;
        float rewardBets = statusesCount[2] * (slotConfig.rewardLimits.MaxLuckyReward - 100.0f) * 0.5f / 100.0f + statusesCount[3] * 
                           (slotConfig.rewardLimits.MaxBigReward + slotConfig.rewardLimits.MinBigReward) * 0.5f / 100.0f;

        Debug.Log("loseBets=" + loseBets + "; rewardBets=" + rewardBets);*/
        if (logToFile && fileLog != null)
        {
            fileLog.Close();
        }

        for (int i = 0; i < spinCount; i++)
        {
            float curMoney = 0.0f;
            for (int j = 0; j < graphCount; j++)
            {
                curMoney += curves[j][i];
            }

            curMoney /= (float)graphCount;

            curveX.AddKey(i + startSpinId, curMoney);
        }
    }
}
#endif