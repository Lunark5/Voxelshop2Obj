using System.Text.Json;
using System.Text.Json.Serialization;
using Assimp;

try
{
    var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

    if (!File.Exists(settingsPath))
    {
        throw new FileNotFoundException(nameof(settingsPath));
    }

    var settingsString = File.ReadAllText(settingsPath);
    var settings = JsonSerializer.Deserialize<Settings>(settingsString);

    var exportPath = string.IsNullOrEmpty(settings.ExportPath)
        ? AppDomain.CurrentDomain.BaseDirectory
        : settings.ExportPath;

    Console.WriteLine($"Экспорт будет произведен в папку: {exportPath}");

    var daeFiles = args.Where(arg => arg.EndsWith(".dae"));

    if (daeFiles.Any())
    {
        foreach (var filePath in daeFiles)
        {
            Console.WriteLine($"-> {filePath}");

            ConvertToObj(filePath, Path.Combine(exportPath, "Obj"));
        }
    }
    else
    {
        Console.WriteLine(".dae файлы не найдены");
    }

    Console.WriteLine("Успех");
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}

Console.WriteLine("Нажмите любую клавишу для выхода...");
Console.ReadKey();

static void ConvertToObj(string filePath, string exportPath)
{
    if (!File.Exists(filePath) || !filePath.EndsWith(".dae"))
    {
        throw new FileNotFoundException(nameof(filePath));
    }

    if (!Directory.Exists(exportPath))
    {
        Directory.CreateDirectory(exportPath);
    }

    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

    using var context = new AssimpContext();

    var scene = context.ImportFile(filePath, PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals);

    var scaleFactor = 0.05f;
    var angleZRad = (float)Math.PI;
    var angleXRad = -(float)Math.PI / 2f;

    var scale = Matrix4x4.FromScaling(new Vector3D(scaleFactor, scaleFactor, scaleFactor));
    var translation = Matrix4x4.FromTranslation(new Vector3D(0.05f, 0.05f, -0.05f));
    var rotationZ = Matrix4x4.FromRotationZ(angleZRad);
    var rotationX = Matrix4x4.FromRotationX(angleXRad);

    var transformMatrix = rotationZ * rotationX * scale * translation;

    scene.RootNode.Transform = transformMatrix;

    var finalPath = Path.Combine(exportPath, $"{fileNameWithoutExtension}.obj");

    if (File.Exists(finalPath))
    {
        File.Delete(finalPath);
    }

    context.ExportFile(scene, finalPath, "obj");
}

[Serializable]
class Settings
{
    [JsonPropertyName("exportPath")] public string ExportPath { get; set; }
}