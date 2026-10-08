using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
internal sealed class _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__GuidPattern_10 : Regex
{
	private sealed class RunnerFactory : RegexRunnerFactory
	{
		private sealed class Runner : RegexRunner
		{
			protected override void Scan(ReadOnlySpan<char> inputSpan)
			{
				while (TryFindNextPossibleStartingPosition(inputSpan) && !TryMatchAtCurrentPosition(inputSpan) && runtextpos != inputSpan.Length)
				{
					runtextpos++;
					if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
				}
			}

			private bool TryFindNextPossibleStartingPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				if (num <= inputSpan.Length - 36)
				{
					ReadOnlySpan<char> readOnlySpan = inputSpan.Slice(num);
					int num2;
					for (num2 = 0; num2 < readOnlySpan.Length - 35; num2++)
					{
						int num3 = readOnlySpan.Slice(num2 + 8).IndexOf('-');
						if (num3 < 0)
						{
							break;
						}
						num2 += num3;
						if ((uint)(num2 + 18) >= (uint)readOnlySpan.Length)
						{
							break;
						}
						if (readOnlySpan[num2 + 13] == '-' && readOnlySpan[num2 + 18] == '-')
						{
							runtextpos = num + num2;
							return true;
						}
					}
				}
				runtextpos = inputSpan.Length;
				return false;
			}

			private bool TryMatchAtCurrentPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				int start = num;
				ReadOnlySpan<char> readOnlySpan = inputSpan.Slice(num);
				if ((uint)readOnlySpan.Length < 36u || !char.IsAsciiHexDigit(readOnlySpan[0]) || !char.IsAsciiHexDigit(readOnlySpan[1]) || !char.IsAsciiHexDigit(readOnlySpan[2]) || !char.IsAsciiHexDigit(readOnlySpan[3]) || !char.IsAsciiHexDigit(readOnlySpan[4]) || !char.IsAsciiHexDigit(readOnlySpan[5]) || !char.IsAsciiHexDigit(readOnlySpan[6]) || !char.IsAsciiHexDigit(readOnlySpan[7]) || readOnlySpan[8] != '-' || !char.IsAsciiHexDigit(readOnlySpan[9]) || !char.IsAsciiHexDigit(readOnlySpan[10]) || !char.IsAsciiHexDigit(readOnlySpan[11]) || !char.IsAsciiHexDigit(readOnlySpan[12]) || readOnlySpan[13] != '-' || !char.IsAsciiHexDigit(readOnlySpan[14]) || !char.IsAsciiHexDigit(readOnlySpan[15]) || !char.IsAsciiHexDigit(readOnlySpan[16]) || !char.IsAsciiHexDigit(readOnlySpan[17]) || readOnlySpan[18] != '-' || !char.IsAsciiHexDigit(readOnlySpan[19]) || !char.IsAsciiHexDigit(readOnlySpan[20]) || !char.IsAsciiHexDigit(readOnlySpan[21]) || !char.IsAsciiHexDigit(readOnlySpan[22]) || readOnlySpan[23] != '-' || !char.IsAsciiHexDigit(readOnlySpan[24]) || !char.IsAsciiHexDigit(readOnlySpan[25]) || !char.IsAsciiHexDigit(readOnlySpan[26]) || !char.IsAsciiHexDigit(readOnlySpan[27]) || !char.IsAsciiHexDigit(readOnlySpan[28]) || !char.IsAsciiHexDigit(readOnlySpan[29]) || !char.IsAsciiHexDigit(readOnlySpan[30]) || !char.IsAsciiHexDigit(readOnlySpan[31]) || !char.IsAsciiHexDigit(readOnlySpan[32]) || !char.IsAsciiHexDigit(readOnlySpan[33]) || !char.IsAsciiHexDigit(readOnlySpan[34]) || !char.IsAsciiHexDigit(readOnlySpan[35]))
				{
					return false;
				}
				Capture(0, start, runtextpos = num + 36);
				return true;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__GuidPattern_10 Instance = new _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__GuidPattern_10();

	private _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__GuidPattern_10()
	{
		pattern = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}
