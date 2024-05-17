namespace HelpDesk;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class Main : IExternalCommand
{
    public Result Execute(
    ExternalCommandData commandData,
    ref string message,
    ElementSet elements)
    {
        if (commandData.Application == null)
            return Result.Cancelled;

        MainWindow window = new MainWindow(commandData.Application);
            window.Show();

        return Result.Succeeded;
    }
}
