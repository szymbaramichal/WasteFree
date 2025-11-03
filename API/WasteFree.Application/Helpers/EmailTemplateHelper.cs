using System.Text;
using System.Text.RegularExpressions;

namespace WasteFree.Application.Helpers;

public static class EmailTemplateHelper
{
    private static readonly Regex PlaceholderRegex = new("{{(?<name>[^{}\\s]+)}}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string ApplyPlaceholders(string template, IReadOnlyDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(values);

        foreach (Match match in PlaceholderRegex.Matches(template))
        {
            var placeholder = match.Groups["name"].Value;

            if (!values.ContainsKey(placeholder))
            {
                throw new ArgumentException($"Missing placeholder '{placeholder}' in provided values.", nameof(values));
            }
        }

        var sb = new StringBuilder(template);
        foreach (var (key, value) in values)
        {
            sb.Replace("{{" + key + "}}", value);
        }
        return sb.ToString();
    }
}