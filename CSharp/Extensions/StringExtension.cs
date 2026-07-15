using System.Text;
using System.Text.RegularExpressions;

public static partial class StringExtension
{
	/// <summary>
	/// Sanitize a string
	/// </summary>
	/// <param name="raw"></param>
	/// <returns></returns>
	public static string Sanitize(this string raw)
	{
		StringBuilder builder = new StringBuilder(raw)
		.Replace("[", " ")
		.Replace("]", " ")
		;

		return builder.ToString();
	}
}