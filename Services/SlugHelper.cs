using System.Text.RegularExpressions;

namespace ZE.Services;

public static class SlugHelper
{
    public static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var slug = value.Trim().ToLowerInvariant();

        // Normalize accented characters where possible.
        slug = slug.Normalize(System.Text.NormalizationForm.FormD);
        slug = Regex.Replace(slug, @"\p{Mn}+", string.Empty);

        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-").Trim('-');

        return slug;
    }
}
