using Bert;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ViewModel;

public interface IUIServices
{
    string? ChooseFileToOpen();
}
public class RelayCommand : ICommand
{
    private readonly Action<object?> execute;
    private readonly Func<object?, bool>? canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool CanExecute(object? parameter) => canExecute == null || canExecute(parameter);

    public void Execute(object? parameter) => execute.Invoke(parameter);
}
public class BertTab : INotifyPropertyChanged
{
    private BertModel model;
    private string? question;
    private CancellationTokenSource ctf;
    private CancellationToken token;
    private MainViewModel controller;
    public string Text { get; set; }
    public string? Question
    {
        get => question;
        set
        {
            AnswerQuestionCommand.RaiseCanExecuteChanged();
            question = value;
        }
    }
    public string? Answer { get; set; }
    public string FileName { get; set; }
    public string TextName 
    { 
        get => Path.GetFileNameWithoutExtension(FileName);
    }
    public bool IsAnswering { get; set; }
    public event PropertyChangedEventHandler? PropertyChanged;
    public RelayCommand AnswerQuestionCommand { get; private set; }
    public RelayCommand CloseTabCommand { get; private set; }
    public void NotifyPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    public BertTab(string text, string fileName, BertModel bertModel, MainViewModel controller)
    {
        ctf = new CancellationTokenSource();
        token = ctf.Token;
        IsAnswering = false;
        this.controller = controller;
        Text = text;
        FileName = fileName;
        model = bertModel;
        AnswerQuestionCommand = new RelayCommand(AnswerQuestionTask, CanAnswer);
        CloseTabCommand = new RelayCommand(CloseTab);
    }
    public async void AnswerQuestionTask(object? sender)
    {
        IsAnswering = true;
        AnswerQuestionCommand.RaiseCanExecuteChanged();
        if (Question is not null)
        {
            Answer = await model.AnswerOneQuestionTask(Text, Question, token);
            NotifyPropertyChanged("Answer");
        }
        
        IsAnswering = false;
        AnswerQuestionCommand.RaiseCanExecuteChanged();
    }
    public bool CanAnswer(object? sender)
    {
        return (Question is not null) && (!IsAnswering);
    }
    public void CloseTab(object? sender)
    {
        ctf.Cancel();
        var tabToDelete = controller.Tabs.FirstOrDefault(f => f.FileName == FileName);
        if (tabToDelete != null)
        {
            controller.Tabs.Remove(tabToDelete);
        }
    }
}
public class MainViewModel : INotifyPropertyChanged
{
    private BertModel bertModel;
    private IUIServices uiServices;
    private CancellationToken token;
    public ICommand NewTabCommand { get; private set; }
    public event PropertyChangedEventHandler? PropertyChanged;
    public void NotifyPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    public ObservableCollection<BertTab> Tabs { get; set; }
    public MainViewModel(IUIServices uiServices)
    {
        this.uiServices = uiServices;
        bertModel = new BertModel(token);
        NewTabCommand = new RelayCommand(AddTab);
        Tabs = new ObservableCollection<BertTab>();
    }
    public void AddTab(object? sender)
    {
        string? filePath = uiServices.ChooseFileToOpen();
        if (filePath != null)
        {
            string text = File.ReadAllText(filePath);
            Tabs.Add(new BertTab(text, Path.GetFileName(filePath), bertModel, this));
            NotifyPropertyChanged("Tabs");
        }
    }

}