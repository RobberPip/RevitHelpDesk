<<<<<<< HEAD
﻿using Autodesk.Revit.UI.Selection;
using Newtonsoft.Json;
using System;

namespace HelpDesk;

public class MainVM : INotifyPropertyChanged
{
    public MainVM() { }
    public MainVM(UIApplication application)
=======
﻿using System.Windows.Forms;

namespace HelpDesk;

public class MainVM : INotifyPropertyChanged, IExternalEventHandler
{
    public MainVM() { }
    private Window _window;
    public MainVM(UIApplication application, Window window)
>>>>>>> 4c2db5e (first version)
    {
        uiapp = application;
        uidoc = uiapp.ActiveUIDocument;
        app = uiapp.Application;
        doc = uidoc.Document;
<<<<<<< HEAD
    }
=======
        _window = window;

        exEvent = ExternalEvent.Create(this);
    }
    public ExternalEvent exEvent { get; set; }
>>>>>>> 4c2db5e (first version)
    UIApplication uiapp { get; set; }
    UIDocument uidoc { get; set; }
    Autodesk.Revit.ApplicationServices.Application app { get; set; }
    Document doc { get; set; }
<<<<<<< HEAD
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
=======
    private UserControl _userControl = new UserControl();
    public UserControl ActiveUserControl
    {
        get { return _userControl; }
        set
        {
            _userControl = value;
            OnPropertyChanged(nameof(ActiveUserControl));
        }
    }
    private ObservableCollection<UserControl> _uploadingFiles = new ObservableCollection<UserControl>();
    public ObservableCollection<UserControl> UploadingFiles
    {
        get { return _uploadingFiles; }
        set
        {
            _uploadingFiles = value;
            OnPropertyChanged(nameof(UploadingFiles));
        }
    }
    private ObservableCollection<UserControl> _advanceFiles = new ObservableCollection<UserControl>();
    public ObservableCollection<UserControl> AdvanceFiles
    {
        get { return _advanceFiles; }
        set
        {
            _advanceFiles = value;
            OnPropertyChanged(nameof(AdvanceFiles));
        }
    }
    private DbProvider _dbProvider = new DbProvider();
    public  RequestModel Request = new RequestModel();
>>>>>>> 4c2db5e (first version)
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
<<<<<<< HEAD
=======
    private string _heading;
    public string Heading
    {
        get { return _heading; }
        set
        {
            _heading = value;
            OnPropertyChanged(nameof(Heading));
        }
    }
>>>>>>> 4c2db5e (first version)
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
<<<<<<< HEAD
        UserControls.Add(new RequestAddControl());
        RequestModel model = new RequestModel()
        {
            Tags = await _dbProvider.GetTagsAsync()
        };
        Items = model.Tags;
    }
=======
        RequestModel requestModel = new RequestModel();
        List<RequestModel> requests = await JqlBuilder.GetNameRequests(requestModel.UID);
        foreach (RequestModel request in requests) { RequestList.Add(request); }
        if (RequestList.Count > 0) { ActiveUserControl = new RequestListControl(this); }
        else
        {
            ActiveUserControl =new RequestAddControl();
            requestModel.Tags = await _dbProvider.GetTagsAsync();
            Items = requestModel.Tags;
        }
    }
    public async void LoadRequest()
    {
        AdvanceFiles.Clear();
        Message = null;

        ActiveUserControl = null;
        ActiveUserControl = new RequestInfoControl();
        Request = await JqlBuilder.GetRequestComment(Request);
        var reversedDictionary = new Dictionary<long, AuthorModel>();
        foreach (var item in Request.Comments.Reverse())
        {
            reversedDictionary.Add(item.Key, item.Value);
        }
        AuthorsDictionary = reversedDictionary;
        Heading = $"{Request.Theme}:{Request.Status}";
    }





    public void Execute(UIApplication app)
    {
        using (Transaction tr = new Transaction(doc, "Export IFC"))
        {

            try
            {
                IList<ElementId> elementIds = new List<ElementId>();
                _window.Hide();
                IList<Reference> refList = uidoc.Selection.PickObjects(ObjectType.Element, "Select");
                var d = uidoc.Selection.GetElementIds();
                tr.Start();
                foreach (Reference r in refList)
                {
                    ElementId eid = doc.GetElement(r).Id;
                    elementIds.Add(eid);
                }
                doc.ActiveView.IsolateElementsTemporary(elementIds);
                uidoc.ShowElements(elementIds);
                IFCExportOptions options = new IFCExportOptions();
                options.FilterViewId = doc.ActiveView.Id;
                string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string fileName = Guid.NewGuid().ToString() + ".ifc";
                string filePath = Path.Combine(appDataFolderPath, fileName);
                doc.Export(appDataFolderPath, fileName, options);
                doc.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                tr.Commit();
                Request.fileBytes = File.ReadAllBytes(filePath);
                File.Delete(filePath);
                _window.Show();
            }
            catch (Exception)
            {
                tr.RollBack();

            }
        }
    }

    public string GetName()
    {
        return "Return";
    }

>>>>>>> 4c2db5e (first version)
    private RelayCommand _addRequest;
    public RelayCommand AddRequest
    {
        get
        {
            return _addRequest ??
              (_addRequest = new RelayCommand(async obj =>
              {
<<<<<<< HEAD
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
=======
                  Request = new RequestModel();
                  Request.Theme = this.Theme;
                  Request.Description = this.Description;
                  Request.Tags = this.SelectedTags;
                  Request.FileDetail = new ObservableCollection<FileDetailModel>(
                  this.UploadingFiles
                        .Cast<FileDetailControl>()
                        .Select(fileControl => new FileDetailModel
                        {
                            FileName = fileControl.FileDetail.FileName,
                            FileSize = fileControl.FileDetail.FileSize,
                            UploadProgess = fileControl.FileDetail.UploadProgess,
                            FileBytes = fileControl.FileDetail.FileBytes
                        })
                );

                  long issueId = await JqlBuilder.SendPostRequest(Request);
                  await _dbProvider.AddRequestAsync(issueId, Request);
                  ActiveUserControl = null;
                  RequestList.Clear();
                  List<RequestModel> requests = await JqlBuilder.GetNameRequests(Request.UID);
                  foreach (RequestModel request in requests) { RequestList.Add(request); }
                  ActiveUserControl = new RequestListControl(this);
                  Theme = null;
                  Description = null;
                  SelectedTags.Clear();
                  UploadingFiles.Clear();
                  Request.fileBytes = null;
              }));
        }
    }


>>>>>>> 4c2db5e (first version)
    private RelayCommand _getAdvanceElements;
    public RelayCommand GetAdvanceElements
    {
        get
        {
            return _getAdvanceElements ??
              (_getAdvanceElements = new RelayCommand(obj =>
              {
<<<<<<< HEAD
                  Reference reference = uidoc.Selection.PickObject(ObjectType.Element);
                  Element element = uidoc.Document.GetElement(reference);
                  string json = JsonConvert.SerializeObject(element);
                  MessageBox.Show(json);
=======
                  exEvent.Raise();
              }));
        }
    }
    private RelayCommand _fileAdd;
    public RelayCommand FileAdd
    {
        get
        {
            return _fileAdd ??
              (_fileAdd = new RelayCommand(obj =>
              {
                  OpenFileDialog dlg = new OpenFileDialog() { Multiselect = true };
                  dlg.FileOk += (sender, e) =>
                  {
                      foreach (string fileName in dlg.FileNames)
                      {
                          FileInfo fileInfo = new FileInfo(fileName);
                          // Проверка что файл не превышает 100мб
                          if (fileInfo.Length > 100 * 1024 * 1024)
                          {
                              MessageBox.Show($"Файл {fileName} превышает максимально допустимый размер (100 Мб).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                              e.Cancel = true;
                              return;
                          }
                      }
                  };
                  bool? responce = dlg.ShowDialog();

                  if (responce == true)
                  {
                      foreach (var file in dlg.FileNames)
                      {
                          string filename = System.IO.Path.GetFileName(file);
                          FileInfo fileInfo = new FileInfo(file);
                          var fileDetail = new FileDetailModel()
                          {
                              FileName = filename,
                              FileSize = string.Format($"{(fileInfo.Length / 1.049e+6).ToString("0.0")} {"Mb"}"),
                              FileBytes = File.ReadAllBytes(file)
                          };
                          FileDetailControl _file = new FileDetailControl() { FileDetail = fileDetail };
                          switch (ActiveUserControl)
                          {
                              case RequestInfoControl _:
                                  AdvanceFiles.Add(_file);
                                  break;
                              case RequestAddControl _:
                                  UploadingFiles.Add(_file);
                                  break;
                              default:
                                  break;
                          }
                      }
                  }
              }));
        }
    }
    private RelayCommand _fileDelete;
    public RelayCommand FileDelete
    {
        get
        {
            return _fileDelete ??
              (_fileDelete = new RelayCommand(obj =>
              {
                  FileDetailControl file = obj as FileDetailControl;

                  UploadingFiles.Remove(file);
                  AdvanceFiles.Remove(file);

              }));
        }
    }
    private RelayCommand _takeScreenshot;
    public RelayCommand TakeScreenshot
    {
        get
        {
            return _takeScreenshot ??
              (_takeScreenshot = new RelayCommand(obj =>
              {
                  _window.Hide();
                  System.Threading.Thread.Sleep(150);
                  Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
                  Bitmap bitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
                  using (Graphics graphics = Graphics.FromImage(bitmap))
                  {
                      graphics.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, screenBounds.Size);
                  }

                  MemoryStream stream = new MemoryStream();
                  bitmap.Save(stream, ImageFormat.Png);
                  byte[] screenshotBytes = stream.ToArray();
                  _window.Show();
                  FileDetailControl file = new FileDetailControl()
                  {
                      FileDetail = new FileDetailModel()
                      {
                          FileName = "RevitScreenshot.png",
                          FileSize = $"{(screenshotBytes.Length / 1.049e+6).ToString("0.0")} Mb",
                          FileBytes = screenshotBytes
                      }
                  };
                  switch (ActiveUserControl)
                  {
                      case RequestInfoControl _:
                          AdvanceFiles.Add(file);
                          break;
                      case RequestAddControl _:
                          UploadingFiles.Add(file);
                          break;
                      default:
                          break;
                  }
              }));
        }
    }
    private RelayCommand _closeRequest;
    public RelayCommand CloseRequest
    {
        get
        {
            return _closeRequest ??
              (_closeRequest = new RelayCommand(obj =>
              {
                  _window.Close();
              }));
        }
    }
    public ObservableCollection<RequestModel> RequestList { get; set; } = new ObservableCollection<RequestModel>();
    private RelayCommand _addRequestWindow;
    public RelayCommand AddRequestWindow
    {
        get
        {
            return _addRequestWindow ??
              (_addRequestWindow = new RelayCommand(async obj =>
              {
                  ActiveUserControl = null;
                  ActiveUserControl = new RequestAddControl();
                  RequestModel requestModel = new RequestModel();
                  requestModel.Tags = await _dbProvider.GetTagsAsync();
                  Items = requestModel.Tags;
              }));
        }
    }
    private RelayCommand _listRequestWindow;
    public RelayCommand ListRequestWindow
    {
        get
        {
            return _listRequestWindow ??
              (_listRequestWindow = new RelayCommand(async obj =>
              {
                  ActiveUserControl = null;
                  RequestList.Clear();
                  RequestModel requestModel = new RequestModel();
                  List<RequestModel> requests = await JqlBuilder.GetNameRequests(requestModel.UID);
                  foreach (RequestModel request in requests) { RequestList.Add(request); }
                  ActiveUserControl = new RequestListControl(this);

              }));
        }
    }
    private RelayCommand _goBack;
    public RelayCommand GoBack
    {
        get
        {
            return _goBack ??
              (_goBack = new RelayCommand(obj =>
              {
                  ActiveUserControl = null;
                  RequestList.Clear();
                  LoadWindow();

              }));
        }
    }
    private Dictionary<long, AuthorModel> _authorsDictionary;
    public Dictionary<long, AuthorModel> AuthorsDictionary
    {
        get { return _authorsDictionary; }
        set
        {
            _authorsDictionary = value;
            OnPropertyChanged(nameof(AuthorsDictionary));
        }
    }
    private string _message;
    public string Message
    {
        get { return _message; }
        set
        {
            _message = value;
            OnPropertyChanged(nameof(Message));
        }
    }
    private RelayCommand _sendInfo;
    public RelayCommand SendInfo
    {
        get
        {
            return _sendInfo ??
              (_sendInfo = new RelayCommand(async obj =>
              {
                  Request.FileDetail = new ObservableCollection<FileDetailModel>(
                  this.AdvanceFiles
                        .Cast<FileDetailControl>()
                        .Select(fileControl => new FileDetailModel
                        {
                            FileName = fileControl.FileDetail.FileName,
                            FileSize = fileControl.FileDetail.FileSize,
                            UploadProgess = fileControl.FileDetail.UploadProgess,
                            FileBytes = fileControl.FileDetail.FileBytes
                        })
                );
                  Request.CommentMessage = Message;
                  await JqlBuilder.SendPostComment(Request);
                  DbProvider db = new DbProvider();
                  await db.AddRequestAsync(-1,Request);
                  RequestModel _ = new RequestModel()
                  {
                      IssueId = Request.IssueId,
                      Theme = Request.Theme,
                      Status = Request.Status,
                  };
                  Request = _;
                  LoadRequest();

>>>>>>> 4c2db5e (first version)
              }));
        }
    }
}


