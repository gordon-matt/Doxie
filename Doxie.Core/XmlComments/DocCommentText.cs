using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Doxie.Core.XmlComments;

/// <summary>
/// Turns XML documentation markup into readable text.
/// Compiler comments store references in attributes, which are lost when only the element text is read.
/// </summary>
public static class DocCommentText
{
    public static string RenderChild(XElement parent, string localName)
    {
        var child = parent?.Elements().FirstOrDefault(element => element.Name.LocalName == localName);
        return child == null ? string.Empty : RenderElement(child);
    }

    public static string RenderElement(XElement element) => element == null ? string.Empty : Normalize(RenderNodes(element.Nodes()));

    public static string RenderExceptions(XElement member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        var parts = member.Elements()
            .Where(element => element.Name.LocalName == "exception")
            .Select(element =>
            {
                string typeName = FormatCref(element.Attribute("cref")?.Value);
                string text = RenderElement(element);
                return string.IsNullOrEmpty(typeName) ? text : string.IsNullOrEmpty(text) ? typeName : $"{typeName}: {text}";
            })
            .Where(text => !string.IsNullOrEmpty(text));

        return string.Join("\n", parts);
    }

    public static string RenderTypeParameters(XElement member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        var parts = member.Elements()
            .Where(element => element.Name.LocalName == "typeparam")
            .Select(element =>
            {
                string name = element.Attribute("name")?.Value ?? string.Empty;
                string text = RenderElement(element);
                return string.IsNullOrEmpty(name) ? text : $"{name}: {text}";
            })
            .Where(text => !string.IsNullOrEmpty(text));

        return string.Join("\n", parts);
    }

    public static string RenderSeeAlso(XElement member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        string items = string.Join(", ", member.Elements()
            .Where(element => element.Name.LocalName == "seealso")
            .Select(RenderReference)
            .Where(text => !string.IsNullOrEmpty(text)));

        return string.IsNullOrEmpty(items) ? string.Empty : "See also: " + items;
    }

    public static bool IsInheritDoc(XElement member)
    {
        if (member == null)
        {
            return false;
        }

        var children = member.Elements().ToList();
        return children.Count > 0 && children.All(element => element.Name.LocalName == "inheritdoc");
    }

    public static string FormatCref(string cref)
    {
        if (string.IsNullOrWhiteSpace(cref))
        {
            return string.Empty;
        }

        string body = cref;
        if (body.Length > 2 && body[1] == ':')
        {
            body = body[2..];
        }

        int parenthesis = body.IndexOf('(');
        if (parenthesis >= 0)
        {
            body = body[..parenthesis];
        }

        body = Regex.Replace(body, "``(\\d+)", match => AngleBrackets(int.Parse(match.Groups[1].Value)));
        body = Regex.Replace(body, "`(\\d+)", match => AngleBrackets(int.Parse(match.Groups[1].Value)));
        return body.Replace('+', '.');
    }

    private static string AngleBrackets(int arity) =>
        arity <= 0 ? string.Empty : "<" + new string(',', Math.Max(0, arity - 1)) + ">";

    private static string RenderNodes(IEnumerable<XNode> nodes)
    {
        var builder = new StringBuilder();
        foreach (var node in nodes)
        {
            switch (node)
            {
                case XText text:
                    builder.Append(text.Value);
                    break;

                case XElement element:
                    builder.Append(RenderInline(element));
                    break;
            }
        }

        return builder.ToString();
    }

    private static string RenderInline(XElement element)
    {
        switch (element.Name.LocalName)
        {
            case "see":
            case "seealso":
                return RenderReference(element);

            case "paramref":
            case "typeparamref":
                return element.Attribute("name")?.Value ?? string.Empty;

            case "c":
                return element.Value;

            case "code":
                return "\n" + element.Value.Trim() + "\n";

            case "para":
                return "\n" + RenderNodes(element.Nodes()) + "\n";

            case "list":
                string items = string.Join("\n", element.Elements().Select(item => "- " + RenderElement(item)));
                return "\n" + items + "\n";

            default:
                return RenderNodes(element.Nodes());
        }
    }

    private static string RenderReference(XElement element)
    {
        string langword = element.Attribute("langword")?.Value;
        if (!string.IsNullOrEmpty(langword))
        {
            return langword;
        }

        string cref = FormatCref(element.Attribute("cref")?.Value);
        if (!string.IsNullOrEmpty(cref))
        {
            return cref;
        }

        string href = element.Attribute("href")?.Value;
        return string.IsNullOrEmpty(href) ? element.Value : href;
    }

    private static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        text = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = text.Split('\n').Select(line => line.TrimEnd()).ToList();
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0]))
        {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        int indent = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Length - line.TrimStart().Length)
            .DefaultIfEmpty(0)
            .Min();

        if (indent > 0)
        {
            lines = lines.Select(line => line.Length >= indent ? line[indent..] : line.TrimStart()).ToList();
        }

        return string.Join("\n", lines).Trim();
    }
}