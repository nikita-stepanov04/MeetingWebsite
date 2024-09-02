using System.Text;
using System.Text.RegularExpressions;

namespace MeetingWebsite.Infrastracture.Models
{
    public static class StringExtentions
    {
        public static string? InjectEnvironmentVariables(this string? confString)
        {
            if (confString != null)
            {
                Regex regex = new(@"\$\{(?<env>[A-Za-z_][A-Za-z0-9_]*)\}");
                MatchCollection matches = regex.Matches(confString);
                StringBuilder builder = new(confString);
                foreach (Match match in matches)
                {
                    string? envvar = Environment.GetEnvironmentVariable(match.Groups["env"].Value);
                    if (envvar != null)
                    { 
                        builder.Replace(match.Value, envvar);
                    }
                }
                return builder.ToString();
            }
            return null;
        }
    }
}
