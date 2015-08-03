using System;
using System.IO;
using Android.App;
using Android.Content;
using ExpansionDownloader.Database;
using System.Linq;

namespace People.Droid
{
public class FileAccessHelper
{
    private static bool AreExpansionFilesDelivered(Context context)
    {
        var downloads = DownloadsDatabase.GetDownloads();
        return downloads.Any() && downloads.All(x => DoesFileExist(context, x.FileName, x.TotalBytes, false));
    }

    public static bool DoesFileExist(Context context, string fileName, long fileSize, bool deleteFileOnMismatch)
    {
        var fileForNewFile = new FileInfo(GenerateSaveFileName(context, fileName));
        if (fileForNewFile.Exists)
        {
            if (fileForNewFile.Length == fileSize)
                return true;

            if (deleteFileOnMismatch)
                fileForNewFile.Delete();
        }

        return false;
    }

    public static string GenerateSaveFileName(Context context, string fileName)
    {
        var root = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + string.Format("{0}Android{0}obb{0}", Path.DirectorySeparatorChar) + context.PackageName;
        return Path.Combine(root, fileName);
    }

	public static string GetLocalFilePath (string filename)
	{
		string path = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
		string dbPath = Path.Combine (path, filename);

		CopyDatabaseIfNotExists (dbPath);

		return dbPath;
	}

	private static void CopyDatabaseIfNotExists (string dbPath)
	{
		if (!File.Exists (dbPath)) {
			using (var br = new BinaryReader (Application.Context.Assets.Open ("people.db3"))) {
				using (var bw = new BinaryWriter (new FileStream (dbPath, FileMode.Create))) {
					byte[] buffer = new byte[2048];
					int length = 0;
					while ((length = br.Read (buffer, 0, buffer.Length)) > 0) {
						bw.Write (buffer, 0, length);
					}
				}
			}
		}
	}
}
}