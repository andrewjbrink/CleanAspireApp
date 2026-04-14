namespace CleanAspireApp.Web.Services;

public class EnqNotifier
{
    public event Func<bool, Task>? OnEnqChanged;

    public async Task NotifyEnqChanged(bool value)
    {
        if (OnEnqChanged != null)
        {
            await OnEnqChanged.Invoke(value);
        }
    }

    public async Task ShowNotification()
    {

    }
}