using System.Text;

namespace FAiSEWebClient.AppCode
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string title)
        {
            string slug = title.ToLowerInvariant()
                               .Replace(" ", "-")
                               .Replace("--", "-")
                               .Normalize(System.Text.NormalizationForm.FormD);

            var sb = new StringBuilder();
            foreach (var c in slug)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) !=
                    System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }
    }
}
