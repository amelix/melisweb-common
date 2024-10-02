namespace MelisWeb.Common.DataBase;

public static class Extensions
{
    public static string ToPascalCase(this string value, string separator = "", char[]? separators = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        value = value.ToLower();

        if ((separators == null))
        {
            separators = new char[] { ' ', '_', '-' };
        }
        var words = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < words.Length; i++)
        {
            words[i] = words[i].Substring(0, 1).ToUpper() + words[i].Substring(1);
        }
        return string.Join(separator, words);
    }

    public static string ToCamelCase(this string value, char[]? separators = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        value = value.ToPascalCase(separators: separators);
        return value.Substring(0, 1).ToLower() + value.Substring(1);
    }

    public static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
    }

    public static string ToKebabCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())).ToLower();
    }

    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x.ToString() : x.ToString())).ToLower();
    }

    public static string ToSingular(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        if (value.EndsWith("phases", StringComparison.InvariantCultureIgnoreCase))
        {
            return value.Substring(0, value.Length - 1);
        }
        if (value.EndsWith("ies", StringComparison.InvariantCultureIgnoreCase))
        {
            return value.Substring(0, value.Length - 3) + "y";
        }
        if (value.EndsWith("es", StringComparison.InvariantCultureIgnoreCase))
        {
            return value.Substring(0, value.Length - 2);
        }
        if (value.EndsWith("s", StringComparison.InvariantCultureIgnoreCase))
        {
            return value.Substring(0, value.Length - 1);
        }
        return value;
    }
}
