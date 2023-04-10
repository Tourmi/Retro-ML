namespace Retro_ML.Utils;
internal static class FileUtils
{
    public static void BackupThenDeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Copy(path, path + ".backup", true);
            File.Delete(path);
        }
    }
}
