using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BotLauncher : EditorWindow
{
    private TextField _botPrefixTextField;
    private SliderInt _botAmountSlider;
    private Button _findBotPathButton;
    private TextField _botLogConfigTextField;
    private string _botPath;

    private readonly List<Process> _botProcesses = new List<Process>();

    private const string _botPathPrefs = "BotsLauncher-BotPath";
    private const string _botPrefixPrefs = "BotsLauncher-BotPrefix";
    private const string _botAmountPrefs = "BotsLauncher-BotsAmount";
    private const string _botLogConfigPrefs = "BotsLauncher-BotsLogConfig";

    private const string _botDefaultLogConfig = 
            "{\n" +
            "    \"WhiteList\": [],\n" +
            "    \"BlackList\": [],\n" +
            "    \"HideStacktrace\": false,\n" +
            "    \"GameLogsPath\": \"./gba-bot-%%BotId%%.logs\"\n" +
            "}";

    private static string _botIdMarker = "%%BotId%%";

    [MenuItem("Realcast/Build/Bots Launcher")]
    private static void Init()
    {
        BotLauncher tool = (BotLauncher)GetWindow(typeof(BotLauncher));
        tool.Show();
    }

    private void CreateGUI()
    {
        _botPrefixTextField = new TextField();
        _botPrefixTextField.label = "Bot Name Prefix";
        _botAmountSlider = new SliderInt(
            start: 1,
            end: 10,
            direction: SliderDirection.Horizontal);
        _botAmountSlider.label = "Bot Amount";
        _botAmountSlider.showInputField = true;

        _findBotPathButton = new Button(FindBotPath)
        {
            text = "Change Bot Path",
        };

        Button launchBotsButton = new Button(LaunchBots)
        {
            text = "Launch Bots"
        };

        Button killBotsButton = new Button(KillBots)
        {
            text = "Kill bots"
        };

        _botLogConfigTextField = new TextField("Log Configuration");
        _botLogConfigTextField.multiline = true;
        _botLogConfigTextField.maxLength = 500;
        _botLogConfigTextField.value = _botDefaultLogConfig;
        _botLogConfigTextField.RegisterValueChangedCallback(OnBotLogConfigTextFieldChanged);

        rootVisualElement.Add(_botPrefixTextField);
        rootVisualElement.Add(_botAmountSlider);
        rootVisualElement.Add(_botLogConfigTextField);
        rootVisualElement.Add(_findBotPathButton);
        rootVisualElement.Add(launchBotsButton);
        rootVisualElement.Add(killBotsButton);

        LoadPreferences();
    }

    private void OnBotLogConfigTextFieldChanged(ChangeEvent<string> newConfig)
    {
        EditorPrefs.SetString(_botLogConfigPrefs, newConfig.newValue);
    }

    private void LoadPreferences()
    {
        _botPath = EditorPrefs.GetString(_botPathPrefs);
        _botPrefixTextField.value = EditorPrefs.GetString(_botPrefixPrefs);
        _botAmountSlider.value = EditorPrefs.GetInt(_botAmountPrefs);
        _botLogConfigTextField.value = EditorPrefs.GetString(_botLogConfigPrefs);
        if (string.IsNullOrEmpty(_botLogConfigTextField.value))
            _botLogConfigTextField.value = _botDefaultLogConfig;

        _findBotPathButton.tooltip = _botPath;
    }

    private void SavePreferences()
    {
        EditorPrefs.SetString(_botPathPrefs, _botPath);
        EditorPrefs.SetString(_botPrefixPrefs, _botPrefixTextField.value);
        EditorPrefs.SetInt(_botAmountPrefs, _botAmountSlider.value);
    }

    private void FindBotPath()
    {
        _botPath = EditorUtility.OpenFilePanel("Find Bot Executable", "", ".exe");
    }

    private void LaunchBots()
    {
        SavePreferences();

        int alreadyLaunchedBots = _botProcesses.Count;

        string compressedLogConfig = _botLogConfigTextField.value.Replace("\n", "").Replace(" ", "");

        for (int i = 0; i < _botAmountSlider.value; ++i)
            _botProcesses.Add(RunBot(_botPath, _botPrefixTextField.value + (alreadyLaunchedBots + i + 1), compressedLogConfig));
    }

    private void KillBots()
    {
        for (int i = _botProcesses.Count - 1; i >= 0; --i)
        {
            if (!_botProcesses[i].HasExited)
                _botProcesses[i].Kill();

            _botProcesses.RemoveAt(i);
        }
    }

    private static Process RunBot(string botPath, string botId, string logConfig)
    {
        string botFolder = Path.GetDirectoryName(botPath);

        Process process = new Process();
        process.StartInfo.FileName = botPath;
        process.StartInfo.WorkingDirectory = botFolder;
        process.StartInfo.Arguments = $"-botId {botId} -logConfig={logConfig.Replace(_botIdMarker, botId)}";

        process.Start();

        return process;
    }
}
