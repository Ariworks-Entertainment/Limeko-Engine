namespace Limeko
{
    public static class Utils
    {
        public static class Paths
        {
            public static string EngineCore = @"Engine\Core";
            public static string EngineData = @"Engine\Data";
            public static string EngineGraphics = @"Engine\Graphics";
            public static string EnginePhysics = @"Engine\Phyics";
        }

        public static class Math
        {
            public enum MathOperation
            {
                Add,
                Subtract,
                Multiply,
                Divide
            }

            public static float Vector3Magnitude(System.Numerics.Vector3 input)
            {
                float r = input.X;
                if (input.Y > r) r = input.Y;
                if (input.Z > r) r = input.Z;
                return r;
            }

            public static Vector3 ForwardFromYawPitch(float yaw, float pitch)
            {
                Vector3 forward;

                forward.X = MathF.Cos(pitch) * MathF.Sin(yaw);
                forward.Y = MathF.Sin(pitch);
                forward.Z = MathF.Cos(pitch) * MathF.Cos(yaw);

                return Vector3.Normalize(forward);
            }

            public static System.Numerics.Vector3 Vector3Operation(System.Numerics.Vector3 a, System.Numerics.Vector3 b, MathOperation operation)
            {
                System.Numerics.Vector3 ret = a;
                switch (operation)
                {
                    case MathOperation.Add:
                        ret.X += b.X; ret.Y += b.Y; ret.Z += b.Z;
                        break;
                    case MathOperation.Subtract:
                        ret.X -= b.X; ret.Y -= b.Y; ret.Z -= b.Z;
                        break;
                    case MathOperation.Multiply:
                        ret.X *= b.X; ret.Y *= b.Y; ret.Z *= b.Z;
                        break;
                    case MathOperation.Divide:
                        ret.X /= b.X; ret.Y /= b.Y; ret.Z /= b.Z;
                        break;
                }
                return ret;
            }
        }

        public static class Images
        {
            public static ImageResult GetImageByPath(string path)
            {
                using var stream = File.OpenRead(path);
                var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                return image;
            }
        }

        public static class Misc
        {
            public static void PrintLimeko(bool spacer)
            {
                if (spacer) Console.WriteLine("");
                Console.WriteLine("                                                           .-'''-.     ");
                Console.WriteLine(".---.                                                     '   _    \\   ");
                Console.WriteLine("|   |.--. __  __   ___         __.....__          .     /   /` '.   \\  ");
                Console.WriteLine("|   ||__||  |/  `.'   `.   .-''         '.      .'|    .   |     \\  '  ");
                Console.WriteLine("|   |.--.|   .-.  .-.   ' /     .-''\"'-.  `.  .'  |    |   '      |  ' ");
                Console.WriteLine("|   ||  ||  |  |  |  |  |/     /________\\   \\<    |    \\    \\     / /  ");
                Console.WriteLine("|   ||  ||  |  |  |  |  ||                  | |   | ____`.   ` ..' /   ");
                Console.WriteLine("|   ||  ||  |  |  |  |  |\\    .-------------' |   | \\ .'   '-...-'`    ");
                Console.WriteLine("|   ||  ||  |  |  |  |  | \\    '-.____...---. |   |/  .                ");
                Console.WriteLine("|   ||__||__|  |__|  |__|  `.             .'  |    /\\  \\               ");
                Console.WriteLine("'---'                        `''-...... -'    |   |  \\  \\              ");
                Console.WriteLine("                                              '    \\  \\  \\             ");
                Console.WriteLine("                                             '------'  '---'           ");
                if (spacer) Console.WriteLine("");
            }

            public static void PrintLicenseDisclaimer()
            {
                Console.WriteLine("Limeko-Engine  Copyright (C) 2026  lunark");
                Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY.");
                Console.WriteLine("This is free software, and you are welcome to redistribute it");
                Console.WriteLine("under certain conditions. Press F9 to learn more.");
            }

            public static void OpenWebpage(string url)
            {
                ProcessStartInfo info = new ProcessStartInfo
                { FileName = url, UseShellExecute = true };
                Process.Start(info);
            }
        }

        public static class MeshLoader
        {
            public static float[] Load_OBJ(string path)
            {
                var positions = new List<Vector3>();
                var normals = new List<Vector3>();
                var vertices = new List<float>();

                foreach (var line in File.ReadLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    switch (parts[0])
                    {
                        case "v":
                            positions.Add(new Vector3(
                                float.Parse(parts[1]),
                                float.Parse(parts[2]),
                                float.Parse(parts[3])));
                            break;

                        case "vn":
                            normals.Add(Vector3.Normalize(new Vector3(
                                float.Parse(parts[1]),
                                float.Parse(parts[2]),
                                float.Parse(parts[3]))));
                            break;

                        case "f":
                            {
                                for (int i = 2; i < parts.Length - 1; i++)
                                {
                                    AddVertex(parts[1], positions, normals, vertices);
                                    AddVertex(parts[i], positions, normals, vertices);
                                    AddVertex(parts[i + 1], positions, normals, vertices);
                                }
                                break;
                            }
                    }
                }

                return vertices.ToArray();
            }

            static void AddVertex(
                string token,
                List<Vector3> positions,
                List<Vector3> normals,
                List<float> vertices)
            {
                // OBJ formats:
                // v
                // v/vt
                // v//vn
                // v/vt/vn

                var indices = token.Split('/');

                int posIndex = int.Parse(indices[0]) - 1;

                int normIndex = -1;
                if (indices.Length >= 3 && !string.IsNullOrEmpty(indices[2]))
                    normIndex = int.Parse(indices[2]) - 1;

                Vector3 pos = positions[posIndex];
                Vector3 norm = normIndex >= 0
                    ? normals[normIndex]
                    : Vector3.UnitY; // safe fallback

                // ---- POSITION ----
                vertices.Add(pos.X);
                vertices.Add(pos.Y);
                vertices.Add(pos.Z);

                // ---- NORMAL ----
                vertices.Add(norm.X);
                vertices.Add(norm.Y);
                vertices.Add(norm.Z);

                // ---- TEMP UVs (IMPORTANT FIX) ----
                vertices.Add(0f);
                vertices.Add(0f);
            }
        }
    }
}