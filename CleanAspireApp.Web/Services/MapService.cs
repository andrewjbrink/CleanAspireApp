using Esri.ArcGISRuntime.Mapping;

namespace CleanAspireApp.Web.Services;

public class MapService
{
    public Map Map { get; set; } = default!;

    public Viewpoint Viewpoint { get; set; } = default!;
}
