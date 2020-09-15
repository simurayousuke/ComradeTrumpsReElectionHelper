using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ComradeTrumpsReElectionHelper
{

    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInProcess(GAME_PROCESS)]
    public class Plugin : BaseUnityPlugin
    {
        private const string GUID = "cn.zhuangcloud.ctre.helper";
        private const string NAME = "Comrade Trump's Re-election Helper";
        private const string VERSION = "1.0";
        private const string GAME_PROCESS = "Comrade Trump's Re-election.exe";
        private const string TITLE = NAME + " v" + VERSION;

        private static Texture2D WindowBackground;
        private static readonly float WINDOW_WIDTH = Mathf.Min(Screen.width, 400);
        private static readonly float WINDOW_HEIGHT = Mathf.Min(Screen.height, WINDOW_WIDTH * 0.618f);
        private Rect windowRect = new Rect(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT);
        private bool SHOW = false;

        private static List<Twitter> TWITTERS = null;
        private static List<Response> RESPONSES = null;

        void Start()
        {
            WindowBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            WindowBackground.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 1));
            WindowBackground.Apply();
            new Harmony(GUID).PatchAll();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Home))
                SHOW = !SHOW;
            if (Input.GetKeyDown(KeyCode.F1))
                Singleton<DataManager>.Instance.PlayerData.var["Support"] += 100;
            else if (Input.GetKeyDown(KeyCode.F2))
                Singleton<DataManager>.Instance.PlayerData.var["Support"] -= 100;
            else if (Input.GetKeyDown(KeyCode.F3))
                Singleton<DataManager>.Instance.PlayerData.var["Fund"] += 1000000;
            else if (Input.GetKeyDown(KeyCode.F4))
                Singleton<DataManager>.Instance.PlayerData.var["Fund"] -= 1000000;
            else if (Input.GetKeyDown(KeyCode.F5))
                Singleton<DataManager>.Instance.PlayerData.var["Chaos"] += 100;
            else if (Input.GetKeyDown(KeyCode.F6))
                Singleton<DataManager>.Instance.PlayerData.var["Chaos"] -= 100;
        }

        void OnGUI()
        {
            if (SHOW)
            {
                GUI.Box(windowRect, GUIContent.none, new GUIStyle { normal = new GUIStyleState { background = WindowBackground } });
                windowRect = GUI.Window(0, windowRect, WindowFunc, TITLE);
            }
        }

        void DrawTwitter(Twitter twitter)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(twitter.News);
            GUILayout.FlexibleSpace();
            GUILayout.Label((twitter.minSupport / 100f).ToString() + " ~ " + (twitter.maxSupport / 100f).ToString());
            GUILayout.EndHorizontal();
        }

        void DrawResponse(Response response)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(response.Topic);
            GUILayout.FlexibleSpace();
            GUILayout.Label((response.Support / 100f).ToString() + " / " + response.Fund.ToString() + " / " + (response.Chaos / 100f).ToString());
            GUILayout.EndHorizontal();
        }

        void DrawStatus(string[] list)
        {
            foreach (string key in list)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(key + ":");
                GUILayout.FlexibleSpace();
                if (key == "Fund")
                    GUILayout.Label((Singleton<DataManager>.Instance.PlayerData.var[key]).ToString());
                else
                    GUILayout.Label((Singleton<DataManager>.Instance.PlayerData.var[key] / 100f).ToString());
                GUILayout.EndHorizontal();
            }
        }

        void WindowFunc(int windowID)
        {
            GUILayout.BeginVertical();
            if (RESPONSES != null)
                foreach (Response response in RESPONSES)
                    DrawResponse(response);
            else if (TWITTERS != null)
                foreach (Twitter twitter in TWITTERS)
                    DrawTwitter(twitter);
            else
            {
                GUILayout.Label("Standby");
                DrawStatus(new string[] { "Support", "Fund", "Chaos" });
            }
            GUILayout.EndVertical();
        }


        [HarmonyPatch(typeof(TwitterPanel), "ShowButtons")]
        class TwitterPatch
        {
            static void Prefix(List<Twitter> list)
            {
                RESPONSES = null;
                TWITTERS = list;
            }
        }

        [HarmonyPatch(typeof(TwitterPanel), "PushTwitter")]
        class TwitterClearPatch
        {
            static void Postfix()
            {
                TWITTERS = null;
            }
        }

        [HarmonyPatch(typeof(QuestionPanel), "Set")]
        class QuestionPatch
        {
            static void Prefix(Question question)
            {
                TWITTERS = null;
                RESPONSES = question.responses;
            }
        }

        [HarmonyPatch(typeof(ResultPanel), "Show")]
        class QuestionClearPatch
        {
            static void Postfix()
            {
                RESPONSES = null;
            }
        }

    }
}
