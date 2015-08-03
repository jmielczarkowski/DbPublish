using System;
using System.IO;
using Android.App;
using Android.Content;
using ExpansionDownloader.Database;
using System.Linq;
using System.IO.Compression.Zip;
using Android.Util;

namespace People.Droid
{
public class FileAccessHelper
{
    public static readonly string dbFileName = "people.db3";

    private static bool AreExpansionFilesDelivered()
    {
        var downloads = DownloadsDatabase.GetDownloads();
        return downloads.Any() && downloads.All(x => DoesFileExist(x.FileName, x.TotalBytes, false));
    }

    private static bool DoesFileExist(string fileName, long fileSize, bool deleteFileOnMismatch)
    {
        var fileForNewFile = new FileInfo(GenerateSaveFileName(fileName));
        if (fileForNewFile.Exists)
        {
            if (fileForNewFile.Length == fileSize)
                return true;

            if (deleteFileOnMismatch)
                fileForNewFile.Delete();
        }

        return false;
    }

    private static string GenerateSaveFileName(string fileName)
    {
        var root = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + string.Format("{0}Android{0}obb{0}", Path.DirectorySeparatorChar) + Application.Context.PackageName;
        return Path.Combine(root, fileName);
    }

	public static string GetLocalFilePath ()
	{
		string path = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
        return Path.Combine(path, dbFileName);
	}

    private static void CopyZipDatabase(ZipFileEntry entry, string name, string path)
	{
        using (var zip = new ZipFile(entry.ZipFileName))
        {
            using (var stream = zip.ReadFile(entry))
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    using (BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.Create)))
                    {
                        byte[] buffer = new byte[2048];
                        int length = 0;
                        while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, length);
                        }
                    }
                }
            }
        }
    }

    internal static bool ProcessExpansionFiles()
    {
        var localFilePath = GetLocalFilePath();

        if(File.Exists(localFilePath))
            return true;

        var hasExpansionFiles = FileAccessHelper.AreExpansionFilesDelivered();
        if (hasExpansionFiles)
        {
            var expansionFiles = ApkExpansionSupport.GetApkExpansionZipFile(Application.Context, Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionCode, 0);
            CopyZipDatabase(expansionFiles.GetEntry(dbFileName), dbFileName, localFilePath);
            return true;
        }
        else
            return false;
    }
}
}