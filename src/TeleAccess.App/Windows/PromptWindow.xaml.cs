using System.Windows;

namespace TeleAccess.App.Windows;

public partial class PromptWindow : Window
{
    public PromptWindow()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            ValueBox.Focus();
            ValueBox.SelectAll();
        };
    }

    public static string? Ask(Window owner, string prompt, string initial = "")
    {
        ArgumentNullException.ThrowIfNull(owner);

        var window = new PromptWindow { Owner = owner };
        window.Prompt.Text = prompt;
        window.ValueBox.Text = initial;

        return window.ShowDialog() == true ? window.ValueBox.Text : null;
    }

    private void OnOkClick(object sender, RoutedEventArgs e) => DialogResult = true;
}
