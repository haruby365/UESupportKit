// © 2022 Jong-il Hong

namespace Haruby.Uesk.IO;

public class ObjGeometry
{
    public string Name { get; set; } = string.Empty;
    public List<ObjFace> Faces { get; } = new();
}
