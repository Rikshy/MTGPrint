using System.Reflection;
using System.IO;

namespace MTGPrint.Helper
{
    public class Constants
    {
        public static readonly string EXE_PATH = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
    }

    public enum Method
    {
        Text = 0,
        Url
    }
}
