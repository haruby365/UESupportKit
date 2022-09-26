// © 2022 Jong-il Hong

using Haruby.Uesk.IO;

namespace Haruby.Uesk.TreeItSupport;

internal class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("There is no args.");
            return 0;
        }

        foreach (var sourceFilePath in args)
        {
            string ext = Path.GetExtension(sourceFilePath);
            if (!".obj".Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("File extension is not OBJ. " + sourceFilePath);
                continue;
            }

            ObjParser parser = new();
            parser.Read(File.OpenRead(sourceFilePath));
            if (!parser.IsSucceed)
            {
                Console.WriteLine("Read failed. " + sourceFilePath);
                continue;
            }

            // Remove bad faces
            parser.RemoveAllInvalidFaces();

            // cm to m
            parser.ScaleAllPositions(0.01f);

            // Separate geometries to objects
            List<ObjGeometry> geometries = parser.Objects.SelectMany(o => o.Geometries).ToList();
            parser.Objects.Clear();
            foreach (var g in geometries)
            {
                string name = g.Name;
                name = name.Replace("branch", "leaf", StringComparison.OrdinalIgnoreCase);

                ObjObject obj = new() { Name = name, };
                obj.Geometries.Add(g);
                parser.Objects.Add(obj);
            }

            string sourceDir = Path.GetDirectoryName(sourceFilePath) ?? string.Empty;
            string sourceFilename = Path.GetFileNameWithoutExtension(sourceFilePath);
            string outputFilePath = Path.Combine(sourceDir, sourceFilename + "_modified.obj");
            parser.Write(File.Create(outputFilePath), false);

            Console.WriteLine(outputFilePath);
        }

        return 0;
    }
}