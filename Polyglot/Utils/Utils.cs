using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyglot
{
    public static class Utils
    {
        public static string GetAppRevision()
        {
            var revFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\rev.conf";
            return File.Exists(revFilePath) ? File.ReadAllText(revFilePath).Trim() : "None";
        }

        public static string GetAppVersionFormat()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return $"{fvi.FileMajorPart}.{fvi.FileMinorPart}.{fvi.FileBuildPart}";
        }

        public static string GetAppCaptionRevosionFormat()
        {
            return $"{GetAppVersionFormat()} Rev. {GetAppRevision()}";
        }
    }
}