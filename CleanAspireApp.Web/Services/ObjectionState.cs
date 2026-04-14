using CleanAspireApp.Domain.ValuationRoll;

namespace CleanAspireApp.Web.Services;

public class ObjectionState
{
    public PropertyRecord? SelectedItem { get; set; }


    public void Set(PropertyRecord data)
        => SelectedItem = data;
}
