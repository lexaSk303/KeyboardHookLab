namespace KeyboardHookLab;
public sealed class MainForm : Form
{
    private readonly TextBox _logTextBox;

    public MainForm()
    {
        Text = "Keyboard Events";
        Width = 700;
        Height = 450;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        _logTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical
        };

        Controls.Add(_logTextBox);

        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        _logTextBox.AppendText(
            $"Нажата клавиша: {e.KeyCode}{Environment.NewLine}");
    }
}