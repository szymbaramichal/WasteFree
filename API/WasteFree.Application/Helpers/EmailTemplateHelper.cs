using System.Text;

namespace WasteFree.Business.Helpers;

public static class EmailTemplateHelper
{
    public static string ApplyPlaceholders(string template, Dictionary<string, string> values)
    {
        var sb = new StringBuilder(template);
        foreach (var (key, value) in values)
        {
            sb.Replace("{{" + key + "}}", value);
        }
        return sb.ToString();
    }
}