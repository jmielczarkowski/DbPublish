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

    internal static bool ProcessExpansionDatabase()
    {
        var localFilePath = GetLocalFilePath();

        // Check if database file exists, so there is no point in processing data
        if(File.Exists(localFilePath))
            return true;

        var expansionFiles = ApkExpansionSupport.GetApkExpansionZipFile(Application.Context, Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionCode, 0);

        // Check if there is expansion file available
        var expansionFileEntry = expansionFiles.GetEntry(dbFileName);
        if (expansionFileEntry == null)
            return false;

        CopyZipDatabase(expansionFiles.GetEntry(dbFileName), dbFileName, localFilePath);
        return true;
    }
}
}