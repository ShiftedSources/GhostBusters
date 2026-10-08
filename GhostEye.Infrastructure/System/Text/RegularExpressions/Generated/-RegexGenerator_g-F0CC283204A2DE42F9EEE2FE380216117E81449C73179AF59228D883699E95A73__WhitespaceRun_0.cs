using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
internal sealed class _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__WhitespaceRun_0 : Regex
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
				if (num <= inputSpan.Length - 2)
				{
					ReadOnlySpan<char> readOnlySpan = inputSpan.Slice(num);
					int num2;
					for (num2 = 0; num2 < readOnlySpan.Length - 1; num2++)
					{
						int num3 = readOnlySpan.Slice(num2).IndexOfAnyWhiteSpace();
						if (num3 < 0)
						{
							break;
						}
						num2 += num3;
						if ((uint)(num2 + 1) >= (uint)readOnlySpan.Length)
						{
							break;
						}
						if (char.IsWhiteSpace(readOnlySpan[num2 + 1]))
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
				int i;
				for (i = 0; (uint)i < (uint)readOnlySpan.Length && char.IsWhiteSpace(readOnlySpan[i]); i++)
				{
				}
				if (i < 2)
				{
					return false;
				}
				readOnlySpan = readOnlySpan.Slice(i);
				Capture(0, start, runtextpos = num + i);
				return true;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__WhitespaceRun_0 Instance = new _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__WhitespaceRun_0();

	private _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__WhitespaceRun_0()
	{
		pattern = "\\s{2,}";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}
