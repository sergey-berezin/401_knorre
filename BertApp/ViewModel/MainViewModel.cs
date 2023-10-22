using Bert;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ViewModel;

public interface IUIServices
{
    string? ChooseFileToOpen();
}
class RelayCommand : ICommand
{
    private readonly Action<object?> execute;

    public RelayCommand(Action<object?> execute)
    {
        this.execute = execute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => execute.Invoke(parameter);
}
public class BertTab : INotifyPropertyChanged
{
    private BertModel model;
    private CancellationToken token;
    public string Text { get; set; }
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public string FileName { get; set; }
    public string TextName 
    { 
        get => Path.GetFileNameWithoutExtension(FileName);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public ICommand AnswerQuestionCommand { get; private set; }
    public void NotifyPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    public BertTab(string text, string fileName, BertModel bertModel, CancellationToken token)
    {
        Text = text;
        FileName = fileName;
        model = bertModel;
        AnswerQuestionCommand = new RelayCommand(AnswerQuestionTask);
        this.token = token;
    }
    public async void AnswerQuestion()
    {
        if (Question is not null)
        {
            Answer = await model.AnswerOneQuestionTask(Text, Question, token);
            NotifyPropertyChanged("Answer");
        }
    }

    public void AnswerQuestionTask(object? sender)
    {
        Task.Factory.StartNew(() =>
        {
            AnswerQuestion();
        });
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
        CancellationTokenSource ctf = new CancellationTokenSource();
        token = ctf.Token;
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
            Tabs.Add(new BertTab(text, Path.GetFileName(filePath), bertModel, token));
            NotifyPropertyChanged("Tabs");
        }
    }
}