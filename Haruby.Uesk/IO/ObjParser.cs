// © 2022 Jong-il Hong

using System.Diagnostics.CodeAnalysis;

namespace Haruby.Uesk.IO;

public class ObjParser
{
    [MemberNotNullWhen(true, new string[]
    {
        nameof(Positions),
        nameof(Normals),
        nameof(TexCoords),
        nameof(Objects),
    })]
    public bool IsSucceed { get; private set; }
    public List<Vector3>? Positions { get; private set; }
    public List<Vector3>? Normals { get; private set; }
    public List<Vector2>? TexCoords { get; private set; }
    public List<ObjObject>? Objects { get; private set; }

    static Vector3 ParseVector3(string str)
    {
        string[] tokens = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 3)
        {
            throw new FormatException("Vector3 has not 3 tokens.");
        }
        Vector3 v;
        v.X = float.Parse(tokens[0]);
        v.Y = float.Parse(tokens[1]);
        v.Z = float.Parse(tokens[2]);
        return v;
    }
    static Vector2 ParseVector2(string str)
    {
        string[] tokens = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 2)
        {
            throw new FormatException("Vector2 has not 2 tokens.");
        }
        Vector2 v;
        v.X = float.Parse(tokens[0]);
        v.Y = float.Parse(tokens[1]);
        return v;
    }
    static ObjFace ParseFace(string str)
    {
        string[] tokens = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 3)
        {
            throw new FormatException("Face has not 3 tokens.");
        }

        static int GetNumber(string token)
        {
            string[] tokens = token.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 1)
            {
                throw new FormatException("Face number has not any token.");
            }
            int num = int.Parse(tokens[0]);
            for (int i = 1; i < tokens.Length; i++)
            {
                int n = int.Parse(tokens[i]);
                if (num != n)
                {
                    throw new NotSupportedException("Face multi-number not supported.");
                }
            }
            return num;
        }

        ObjFace face;
        face.Index1 = GetNumber(tokens[0]);
        face.Index2 = GetNumber(tokens[1]);
        face.Index3 = GetNumber(tokens[2]);
        return face;
    }

    public void Read(Stream stream)
    {
        IsSucceed = false;

        using StreamReader reader = new(stream, leaveOpen: true);

        string? line = reader.ReadLine();
        List<Vector3> positions = new();
        List<Vector3> normals = new();
        List<Vector2> texCoords = new();
        List<ObjObject> objects = new();
        ObjObject? currentObject = null;
        ObjGeometry? currentGeometry = null;
        while (line is not null)
        {
            if (line.StartsWith("v "))
            {
                positions.Add(ParseVector3(line[2..]));
            }
            else if (line.StartsWith("vn "))
            {
                normals.Add(ParseVector3(line[3..]));
            }
            else if (line.StartsWith("vt "))
            {
                texCoords.Add(ParseVector2(line[3..]));
            }
            else if (line.StartsWith("o "))
            {
                if (currentObject is not null)
                {
                    objects.Add(currentObject);
                }
                currentObject = new() { Name = line[2..], };
            }
            else if (line.StartsWith("g "))
            {
                if (currentGeometry is not null)
                {
                    if (currentObject is null)
                    {
                        throw new FormatException("There is no object to add geometry.");
                    }
                    currentObject.Geometries.Add(currentGeometry);
                }
                currentGeometry = new() { Name = line[2..], };
            }
            else if (line.StartsWith("f "))
            {
                if (currentGeometry is null)
                {
                    throw new FormatException("There is no geometry but encountered face.");
                }
                currentGeometry.Faces.Add(ParseFace(line[2..]));
            }
            line = reader.ReadLine();
        }
        if (currentGeometry is not null)
        {
            if (currentObject is null)
            {
                throw new FormatException("There is no object to add geometry.");
            }
            currentObject.Geometries.Add(currentGeometry);
        }
        if (currentObject is not null)
        {
            objects.Add(currentObject);
        }

        Positions = positions;
        Normals = normals;
        TexCoords = texCoords;
        Objects = objects;
        IsSucceed = true;
    }

    public bool IsInvalidFace(ObjFace face)
    {
        if (!IsSucceed)
        {
            return true;
        }
        return !(0 < face.Index1 && face.Index1 <= Positions.Count && face.Index1 <= Normals.Count && face.Index1 <= TexCoords.Count) ||
            !(0 < face.Index2 && face.Index2 <= Positions.Count && face.Index2 <= Normals.Count && face.Index2 <= TexCoords.Count) ||
            !(0 < face.Index3 && face.Index3 <= Positions.Count && face.Index3 <= Normals.Count && face.Index3 <= TexCoords.Count);
    }

    public void RemoveAllInvalidFaces()
    {
        if (!IsSucceed)
        {
            return;
        }
        foreach (var o in Objects)
        {
            foreach (var g in o.Geometries)
            {
                g.Faces.RemoveAll(IsInvalidFace);
            }
        }
    }

    public void ScaleAllPositions(float scale)
    {
        if (!IsSucceed)
        {
            return;
        }
        for (int i = 0; i < Positions.Count; i++)
        {
            Vector3 p = Positions[i];
            p.X *= scale;
            p.Y *= scale;
            p.Z *= scale;
            Positions[i] = p;
        }
    }

    public void Write(Stream stream, bool includeNormal)
    {
        if (!IsSucceed)
        {
            return;
        }

        using StreamWriter writer = new(stream, leaveOpen: true);
        foreach (var p in Positions)
        {
            writer.WriteLine($"v {p.X} {p.Y} {p.Z}");
        }
        if (includeNormal)
        {
            foreach (var n in Normals)
            {
                writer.WriteLine($"vn {n.X} {n.Y} {n.Z}");
            }
        }
        foreach (var t in TexCoords)
        {
            writer.WriteLine($"vt {t.X} {t.Y}");
        }
        foreach (var o in Objects)
        {
            writer.WriteLine($"o {o.Name}");
            foreach (var g in o.Geometries)
            {
                writer.WriteLine($"g {g.Name}");
                foreach (var f in g.Faces)
                {
                    if (includeNormal)
                    {
                        writer.WriteLine($"f {f.Index1}/{f.Index1}/{f.Index1} {f.Index2}/{f.Index2}/{f.Index2} {f.Index3}/{f.Index3}/{f.Index3}");
                    }
                    else
                    {
                        writer.WriteLine($"f {f.Index1}/{f.Index1} {f.Index2}/{f.Index2} {f.Index3}/{f.Index3}");
                    }
                }
            }
        }
    }
}
