using System.ComponentModel;
using System.Diagnostics;
using KeyboardHookLab.Interop;

namespace KeyboardHookLab;

public sealed class MainForm : Form
{
    private readonly TextBox _logTextBox;
    private readonly CheckBox _hookCheckBox;
    private readonly Label _statusLabel;
    private readonly Label _counterLabel;
    private readonly Button _clearButton;
    private readonly KeyboardHook _keyboardHook;
    private int _eventCounter;

    public MainForm()
    {
        Text = "Keyboard Hook Laboratory";
        Width = 850;
        Height = 500;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        _keyboardHook = new KeyboardHook();
        _keyboardHook.KeyPressed += OnHookKeyPressed;

        _logTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical
        };

        _hookCheckBox = new CheckBox { Text = "Включить low-level hook", AutoSize = true, Margin = new Padding(8) };

        _clearButton = new Button { Text = "Очистить журнал", AutoSize = true };

        _statusLabel = new Label { Text = "Hook не установлен", AutoSize = true, Margin = new Padding(8) };

        _counterLabel = new Label { Text = "Событий: 0", AutoSize = true, Margin = new Padding(8) };

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = false
        };
        toolbar.Controls.AddRange(new Control[] { _hookCheckBox, _clearButton, _statusLabel, _counterLabel });

        Controls.Add(_logTextBox);
        Controls.Add(toolbar);

        KeyDown += OnLocalKeyDown;
        _clearButton.Click += (_, _) => { _logTextBox.Clear(); _eventCounter = 0; UpdateCounter(); };
        _hookCheckBox.CheckedChanged += OnHookCheckedChanged;
        Activated += OnFormActivated;
        Deactivate += OnFormDeactivated;
        FormClosing += OnFormClosing;
    }

    private void OnLocalKeyDown(object? sender, KeyEventArgs e)
    {
        AppendLog("WinForms", e.KeyCode.ToString());
    }

    private void OnHookKeyPressed(Keys key)
    {
        if (IsDisposed || !IsHandleCreated) return;

        BeginInvoke(() =>
        {
            if (!IsDisposed)
            {
                AppendLog("Low-level hook", key.ToString());
            }
        });
    }

    private void AppendLog(string source, string key)
    {
        string timestamp = DateTimeOffset.Now.ToString("HH:mm:ss.fff");
        _logTextBox.AppendText($"[{timestamp}] [{source}] {key}{Environment.NewLine}");
        _eventCounter++;
        UpdateCounter();
    }

    private void UpdateCounter()
    {
        _counterLabel.Text = $"Событий: {_eventCounter}";
    }

    private void OnHookCheckedChanged(object? sender, EventArgs e)
    {
        if (_hookCheckBox.Checked && Form.ActiveForm == this)
            StartHook();
        else
            StopHook();
    }

    private void OnFormActivated(object? sender, EventArgs e)
    {
        if (_hookCheckBox.Checked) StartHook();
    }

    private void OnFormDeactivated(object? sender, EventArgs e)
    {
        StopHook();
    }

    private void StartHook()
    {
        if (_keyboardHook.IsInstalled) 
            return;

        try
        {
            _keyboardHook.Start();
            _statusLabel.Text = "Hook установлен";
            AppendLog("System", "Hook установлен");
        }
        catch (Win32Exception ex)
        {
            _statusLabel.Text = "Ошибка установки hook";
            AppendLog("Error", ex.Message);
            _hookCheckBox.Checked = false;
        }
    }

    private void StopHook()
    {
        if (!_keyboardHook.IsInstalled) 
            return;

        try
        {
            _keyboardHook.Stop();
            _statusLabel.Text = "Hook не установлен";
            AppendLog("System", "Hook снят");
        }
        catch (Win32Exception ex)
        {
            _statusLabel.Text = "Ошибка снятия hook";
            AppendLog("Error", ex.Message);
        }
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        _hookCheckBox.Enabled = false;
        StopHook();
        try { _keyboardHook.Dispose(); }// Задание 4: блокируем кнопку при закрытии
        catch (Win32Exception ex) { Debug.WriteLine($"Ошибка освобождения: {ex}"); }
    }
}