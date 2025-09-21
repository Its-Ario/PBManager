using System.Windows;
using System.Windows.Controls;

namespace PBManager.UI.Helpers;

public static class PasswordBoxPlaceholder
{
    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.RegisterAttached(
            "PlaceholderText",
            typeof(string),
            typeof(PasswordBoxPlaceholder),
            new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

    public static string GetPlaceholderText(DependencyObject d) => (string)d.GetValue(PlaceholderTextProperty);
    public static void SetPlaceholderText(DependencyObject d, string value) => d.SetValue(PlaceholderTextProperty, value);

    private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox passwordBox)
        {
            passwordBox.PasswordChanged -= OnPasswordChanged;
            passwordBox.PasswordChanged += OnPasswordChanged;
        }
    }

    private static void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox && passwordBox.Template.FindName("placeholderText", passwordBox) is TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}