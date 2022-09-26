// © 2022 Jong-il Hong

namespace Haruby.Uesk.IO;

public class ObjObject
{
    public string Name { get; set; } = string.Empty;
    public List<ObjGeometry> Geometries { get; } = new();
}
