using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
internal sealed class _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__MsiProductCode_1 : Regex
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
				if (num <= inputSpan.Length - 38)
				{
					ReadOnlySpan<char> readOnlySpan = inputSpan.Slice(num);
					int num2;
					for (num2 = 0; num2 < readOnlySpan.Length - 37; num2++)
					{
						int num3 = readOnlySpan.Slice(num2).IndexOf('{');
						if (num3 < 0)
						{
							break;
						}
						num2 += num3;
						if ((uint)(num2 + 2) >= (uint)readOnlySpan.Length)
						{
							break;
						}
						ulong num4;
						if ((long)((ulong)(-6919763510146625600L << (int)(num4 = (uint)(readOnlySpan[num2 + 1] - 45))) & (num4 - 64)) < 0L && (long)((ulong)(-6919763510146625600L << (int)(num4 = (uint)(readOnlySpan[num2 + 2] - 45))) & (num4 - 64)) < 0L)
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
				if ((uint)readOnlySpan.Length < 38u || readOnlySpan[0] != '{')
				{
					return false;
				}
				if (readOnlySpan.Slice(1, 36).IndexOfAnyExcept(_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_ascii_20FF037E0000007E000000) >= 0)
				{
					return false;
				}
				if (readOnlySpan[37] != '}')
				{
					return false;
				}
				Capture(0, start, runtextpos = num + 38);
				return true;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__MsiProductCode_1 Instance = new _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__MsiProductCode_1();

	private _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__MsiProductCode_1()
	{
		pattern = "\\{[0-9A-Fa-f\\-]{36}\\}";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}
