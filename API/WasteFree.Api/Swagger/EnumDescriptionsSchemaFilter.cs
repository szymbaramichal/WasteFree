// Explanation: Schema filter that adds x-enumDescriptions vendor extension (and augments schema.Description) with XML doc summaries for enum members.
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WasteFree.App.Swagger;

public sealed class EnumDescriptionsSchemaFilter : ISchemaFilter
{
    private static readonly ConcurrentDictionary<Assembly, XmlDocument?> XmlCache = new();

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (type == null || !type.IsEnum || schema.Enum == null || schema.Enum.Count == 0) return;

        var xml = LoadXmlFor(type.Assembly);
        if (xml == null) return; // no xml file -> nothing to add

        var descriptions = new OpenApiArray();
        foreach (var name in Enum.GetNames(type))
        {
            var memberSummary = GetMemberSummary(xml, type, name);
            descriptions.Add(new OpenApiString(memberSummary ?? string.Empty));
        }

        if (descriptions.Any(d => !string.IsNullOrEmpty(((OpenApiString)d).Value)))
        {
            schema.Extensions["x-enumDescriptions"] = descriptions;
        }

        // Optionally append value list with descriptions to the schema description for UIs that don't read the extension.
        if (schema.Description == null) schema.Description = string.Empty;
        if (!schema.Description.Contains("Values:", StringComparison.Ordinal))
        {
            var lines = Enum.GetNames(type)
                .Select((n, i) => $"{n} = {Convert.ToInt64(Enum.Parse(type, n))} - {((OpenApiString)descriptions[i]).Value}".TrimEnd(' ', '-'));
            schema.Description = string.IsNullOrWhiteSpace(schema.Description)
                ? $"Values:\n{string.Join("\n", lines)}"
                : schema.Description + "\n\nValues:\n" + string.Join("\n", lines);
        }
    }

    private static XmlDocument? LoadXmlFor(Assembly assembly)
    {
        return XmlCache.GetOrAdd(assembly, asm =>
        {
            try
            {
                var xmlPath = Path.ChangeExtension(asm.Location, ".xml");
                if (!File.Exists(xmlPath)) return null;
                var doc = new XmlDocument();
                doc.Load(xmlPath);
                return doc;
            }
            catch
            {
                return null;
            }
        });
    }

    private static string? GetMemberSummary(XmlDocument xml, Type enumType, string name)
    {
        var memberName = $"F:{enumType.FullName}.{name}"; // enum fields documented with F: prefix
        var node = xml.SelectSingleNode($"/doc/members/member[@name='{memberName}']/summary");
        var summary = node?.InnerText?.Trim();
        if (string.IsNullOrEmpty(summary)) return null;
        // collapse whitespace
        return string.Join(' ', summary.Split(['\\', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries));
    }
}

