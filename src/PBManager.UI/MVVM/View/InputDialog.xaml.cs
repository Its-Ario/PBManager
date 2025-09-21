using System.Windows;

namespace PBManager.UI.MVVM.View;

public partial class InputDialog : Window
{
    public InputDialog(string title, string question, string defaultAnswer = "")
    {
        InitializeComponent();
        Title = title;
        lblQuestion.Text = question;
        txtAnswer.Text = defaultAnswer;
        txtAnswer.Focus();
    }

    public string Answer => txtAnswer.Text;

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = true;
    }
}