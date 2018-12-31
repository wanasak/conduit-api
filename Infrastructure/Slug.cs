using System.Globalization;
using System.Linq;
using System.Text;

namespace conduit_api.Infrastructure
{
    public static class Slug
    {
        public static string GenerateSlug(this string phrase)
        {
            return phrase;
        }

        public static string RemoveDiacritics(this string text)
        {
            var s = new string(text.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());

            return s.Normalize(NormalizationForm.FormC);
        }
    }
}