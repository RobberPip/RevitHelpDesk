namespace HelpDesk;
public class MainVM : INotifyPropertyChanged, IExternalEventHandler
{
    #region AuxiliaryFields
    private Window _window;
    private string _theme;
    private bool _apply = false;
    private string _message;
    private string _heading;
    private string _description;
    private bool _isEnabledSend = false;
    private UserControl _userControl = new UserControl();
    private RelayCommand _addRequest;
    private RelayCommand _getAdvanceElements;
    private RelayCommand _fileAdd;
    private RelayCommand _fileDelete;
    private RelayCommand _takeScreenshot;
    private RelayCommand _closeRequest;
    private RelayCommand _listRequestWindow;
    private RelayCommand _addRequestWindow;
    private RelayCommand _goBack;
    private RelayCommand _sendInfo;
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion
    #region Fields
    public string Theme
    {
        get { return _theme; }
        set
        {
            _theme = value;
            OnPropertyChanged(nameof(Theme));
        }
    }
    public bool Apply
    {
        get { return _apply; }
        set
        {
            _apply = value;
            OnPropertyChanged(nameof(Apply));
        }
    }
    public string Heading
    {
        get { return _heading; }
        set
        {
            _heading = value;
            OnPropertyChanged(nameof(Heading));
        }
    }
    public string Description
    {
        get { return _description; }
        set
        {
            _description = value;
            OnPropertyChanged(nameof(Description));
        }
    }
    public string Message
    {
        get { return _message; }
        set
        {
            _message = value;
            OnPropertyChanged(nameof(Message));
        }
    }
    public bool IsEnabledSend
    {
        get { return _isEnabledSend; }
        set
        {
            _isEnabledSend = value;
            OnPropertyChanged(nameof(IsEnabledSend));
        }
    }
    public UserControl ActiveUserControl
    {
        get { return _userControl; }
        set
        {
            _userControl = value;
            OnPropertyChanged(nameof(ActiveUserControl));
        }
    }
    private DbProvider _dbProvider = new DbProvider();
    public RequestModel Request = new RequestModel();
    public Document doc { get; set; }
    public ExternalEvent exEvent { get; set; }
    UIApplication uiapp { get; set; }
    UIDocument uidoc { get; set; }
    Autodesk.Revit.ApplicationServices.Application app { get; set; }
    public ObservableCollection<UserControl> UploadingFiles { get; set; } = new ObservableCollection<UserControl>();
    public ObservableCollection<UserControl> AdvanceFiles { get; set; } = new ObservableCollection<UserControl>();
    public ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>();
    public ObservableCollection<string> SelectedTags { get; set; } = new ObservableCollection<string>();
    public ObservableCollection<RequestModel> RequestList { get; set; } = new ObservableCollection<RequestModel>();
    public Dictionary<long, AuthorModel> AuthorsDictionary { get; set; } = new Dictionary<long, AuthorModel>();

    #endregion
    #region Commands
    public RelayCommand AddRequest
    {
        get
        {
            return _addRequest ??
              (_addRequest = new RelayCommand(async obj =>
              {
                  Apply = false;
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
                  string contractor = await GetContractor(Request);
                  Request.Contractor = (contractor != default) ? contractor : Request.Contractor;
                  Request.IssueId = await JqlBuilder.SendPostRequest(Request);
                  await _dbProvider.AddRequestAsync(Request);
                  await SaveFiles(Request);
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
    public RelayCommand GetAdvanceElements
    {
        get
        {
            return _getAdvanceElements ??
              (_getAdvanceElements = new RelayCommand(obj =>
              {
                  exEvent.Raise();
              }));
        }
    }
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
    public RelayCommand AddRequestWindow
    {
        get
        {
            return _addRequestWindow ??
              (_addRequestWindow = new RelayCommand(obj =>
              {
                  ActiveUserControl = null;
                  ActiveUserControl = new RequestAddControl(this);
                  RequestModel requestModel = new RequestModel();
                  requestModel.Tags = _dbProvider.GetTags();
                  Items = requestModel.Tags;
              }));
        }
    }
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
    public RelayCommand GoBack
    {
        get
        {
            return _goBack ??
              (_goBack = new RelayCommand(async obj =>
              {
                  ActiveUserControl = null;
                  RequestList.Clear();
                  await LoadWindow();

              }));
        }
    }
    public RelayCommand SendInfo
    {
        get
        {
            return _sendInfo ??
              (_sendInfo = new RelayCommand(async obj =>
              {
                  IsEnabledSend = false;
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
                  long tempIssueId = Request.IssueId;
                  Request.IssueId = -1;
                  await db.AddRequestAsync(Request);
                  await SaveFiles(Request);
                  Request.IssueId = tempIssueId;
                  RequestModel _ = new RequestModel()
                  {
                      IssueId = Request.IssueId,
                      Theme = Request.Theme,
                      Status = Request.Status,
                  };
                  Request = _;
                  await LoadRequest();
                  IsEnabledSend = true;
              }));
        }
    }
    #endregion
    public MainVM(UIApplication application, Window window)
    {
        uiapp = application;
        uidoc = uiapp.ActiveUIDocument;
        app = uiapp.Application;
        doc = uidoc.Document;
        _window = window;
        exEvent = ExternalEvent.Create(this);
    }
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public async Task LoadWindow()
    {
        RequestModel requestModel = new RequestModel();
        List<RequestModel> requests = await JqlBuilder.GetNameRequests(requestModel.UID);
        foreach (RequestModel request in requests) { RequestList.Add(request); }
        if (RequestList.Count > 0) { ActiveUserControl = new RequestListControl(this); }
        else
        {
            ActiveUserControl = new RequestAddControl(this);
            requestModel.Tags = _dbProvider.GetTags();
            Items = requestModel.Tags;
        }
    }
    public async Task LoadRequest()
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
            tr.Start();
            foreach (Reference item in refList)
            {
                ElementId eid = doc.GetElement(item).Id;
                elementIds.Add(eid);
            }
            doc.ActiveView.IsolateElementsTemporary(elementIds);
            uidoc.ShowElements(elementIds);
            IFCExportOptions options = new IFCExportOptions();
            options.FilterViewId = doc.ActiveView.Id;
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(appDataFolderPath, fileName+".ifc");
            doc.Export(appDataFolderPath, fileName+".ifc", options);
            doc.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
            
            string outputElemntIds = string.Join(",", elementIds);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                {
                    streamWriter.Write("Элементы: " +outputElemntIds);
                    streamWriter.Flush();
                    long fileSize = memoryStream.Length;
                    byte[] bytes = memoryStream.ToArray();
                    string fileInfoName = "Выбранные элементы IFC " + fileName +".txt";
                    BuildFile(fileInfoName, fileSize, bytes);
                }
            }

            FileInfo fileInfo = new FileInfo(filePath);
            BuildFile(fileName+".ifc",fileInfo.Length,File.ReadAllBytes(filePath));
            tr.Commit();
            File.Delete(filePath);
            _window.Show();
        }
        catch (Exception)
        {
            tr.RollBack();
        }
    }
}

    private void BuildFile(string fileName,long size,byte[] bytes)
    {
        var fileDetail = new FileDetailModel()
        {
            FileName = fileName,
            FileSize = string.Format($"{(size / 1.049e+6).ToString("0.0")} {"Mb"}"),
            FileBytes = bytes
        };
        FileDetailControl _file = new FileDetailControl() { FileDetail = fileDetail };
        UploadingFiles.Add(_file);
    }
    public string GetName()
    {
        return "Return";
    }
    public async Task<string> GetContractor(RequestModel request)
    {
        var contractors = await _dbProvider.GetContractorsAsync();
        Func<KeyValuePair<int, (string, List<string>)>, double> matchPercentageFunc = p =>
        {
            return (double)p.Value.Item2.Intersect(request.Tags).Count() / request.Tags.Count * 100;
        };
        double maxMatchPercentage = contractors.Max(matchPercentageFunc);
        var maxMatchContractors = contractors.Where(p => matchPercentageFunc(p) == maxMatchPercentage && maxMatchPercentage > 50);
        var tasks = maxMatchContractors.Select(async contractor =>
        {
            var (tasksCount, success) = await JqlBuilder.GetActiveTasks(contractor.Value.Item1);
            if (success)
            {
                return (ContractorName: contractor.Value.Item1, TasksCount: tasksCount);
            }
            return default;
        }).Where(task => task != default).ToArray();

        await Task.WhenAll(tasks);

        var minTasksContractor = tasks
            .Where(task => task.Result != default) // Фильтруем успешные результаты
            .Select(task => task.Result)
            .OrderBy(taskInfo => taskInfo.TasksCount)
            .FirstOrDefault();

        if (minTasksContractor != default)
        {
            return minTasksContractor.ContractorName;
        }
        return default;
    }
    public async Task SaveFiles(RequestModel requestModel)
    {
        string assemblyPath = Assembly.GetExecutingAssembly().Location;
        string assemblyDirectory = Path.GetDirectoryName(assemblyPath);
        string requestsFilesDirectory = Path.Combine(assemblyDirectory, "RequestsFiles");
        string requestDirectory = Path.Combine(requestsFilesDirectory, requestModel.IssueId.ToString());
        new[] { requestsFilesDirectory, requestDirectory }
            .Where(dir => !Directory.Exists(dir))
            .ToList()
            .ForEach(dir => Directory.CreateDirectory(dir));
        foreach (FileDetailModel item in requestModel.FileDetail)
        {
            string filePath = Path.Combine(requestDirectory, item.FileName);
            File.WriteAllBytes(filePath, item.FileBytes);
        }
    }
}