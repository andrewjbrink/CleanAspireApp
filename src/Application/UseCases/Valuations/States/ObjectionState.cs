using CleanAspireApp.Domain.ValuationRoll;

namespace CleanAspireApp.Application.UseCases.Valuations.States;

public class ObjectionState
{
    public PropertyRecord? SelectedItem { get; set; }


    public void Set(PropertyRecord data)
        => SelectedItem = data;
}
