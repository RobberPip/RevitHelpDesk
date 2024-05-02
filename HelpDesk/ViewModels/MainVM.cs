using Autodesk.Revit.UI.Selection;
using Newtonsoft.Json;
using System;

namespace HelpDesk;

public class MainVM : INotifyPropertyChanged
{
    public MainVM() { }
    public MainVM(UIApplication application)
    {
        uiapp = application;
        uidoc = uiapp.ActiveUIDocument;
        app = uiapp.Application;
        doc = uidoc.Document;
    }
    UIApplication uiapp { get; set; }
    UIDocument uidoc { get; set; }
    Autodesk.Revit.ApplicationServices.Application app { get; set; }
    Document doc { get; set; }
    private ObservableCollection<UserControl> _userControls = new ObservableCollection<UserControl>();
    public ObservableCollection<UserControl> UserControls
    {
        get { return _userControls; }
        set
        {
            _userControls = value;
            OnPropertyChanged(nameof(UserControls));
        }
    }
    private DbProvider _dbProvider = new DbProvider();
    public RequestModel Request { get; set; }
    private string _theme;
    public string Theme
    {
        get { return _theme; }
        set
        {
            _theme = value;
            OnPropertyChanged(nameof(Theme));
        }
    }
    private string _description;
    public string Description
    {
        get { return _description; }
        set
        {
            _description = value;
            OnPropertyChanged(nameof(Description));
        }
    }
    private ObservableCollection<string> _items;
    public ObservableCollection<string> Items
    {
        get { return _items; }
        set
        {
            _items = value;
            OnPropertyChanged(nameof(Items));
        }
    }
    private ObservableCollection<string> _selectedTags = new ObservableCollection<string>();
    public ObservableCollection<string> SelectedTags
    {
        get { return _selectedTags; }
        set
        {
            _selectedTags = value;
            OnPropertyChanged(nameof(SelectedTags));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public async void LoadWindow()
    {
        UserControls.Add(new RequestAddControl());
        RequestModel model = new RequestModel()
        {
            Tags = await _dbProvider.GetTagsAsync()
        };
        Items = model.Tags;
    }
    private RelayCommand _addRequest;
    public RelayCommand AddRequest
    {
        get
        {
            return _addRequest ??
              (_addRequest = new RelayCommand(async obj =>
              {
                  Request = new RequestModel()
                  {
                      Theme = this.Theme,
                      Description = this.Description,
                      Tags = this.SelectedTags,
                  };
                  int issueId = await JqlBuilder.SendPostRequest(Request);
                  await _dbProvider.AddRequestAsync(issueId);
              }));
        }
    }
    private RelayCommand _getAdvanceElements;
    public RelayCommand GetAdvanceElements
    {
        get
        {
            return _getAdvanceElements ??
              (_getAdvanceElements = new RelayCommand(obj =>
              {
                  Reference reference = uidoc.Selection.PickObject(ObjectType.Element);
                  Element element = uidoc.Document.GetElement(reference);
                  string json = JsonConvert.SerializeObject(element);
                  MessageBox.Show(json);
              }));
        }
    }
}


