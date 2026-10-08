using System.Buffers;
using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
internal static class _003CRegexGenerator_g_003EF71E376FAD2752FBA54FD96890479470EE720A1C7871087FCE98491935D932B6C__Utilities
{
	internal static readonly TimeSpan s_defaultTimeout = ((AppContext.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") is TimeSpan timeSpan) ? timeSpan : Regex.InfiniteMatchTimeout);

	internal static readonly bool s_hasTimeout = s_defaultTimeout != Regex.InfiniteMatchTimeout;

	internal static readonly SearchValues<char> s_ascii_60FF03FEFFFF87FEFFFF07 = SearchValues.Create("-.0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz");
}
