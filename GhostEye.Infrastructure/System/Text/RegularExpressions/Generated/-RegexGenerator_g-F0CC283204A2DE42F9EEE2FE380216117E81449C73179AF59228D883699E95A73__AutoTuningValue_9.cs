using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
internal sealed class _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__AutoTuningValue_9 : Regex
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
				if (num <= inputSpan.Length - 7)
				{
					int num2 = inputSpan.Slice(num).IndexOf(':');
					if (num2 >= 0)
					{
						runtextpos = num + num2;
						return true;
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
				if (readOnlySpan.IsEmpty || readOnlySpan[0] != ':')
				{
					UncaptureUntil(0);
					return false;
				}
				int i;
				for (i = 1; (uint)i < (uint)readOnlySpan.Length && char.IsWhiteSpace(readOnlySpan[i]); i++)
				{
				}
				readOnlySpan = readOnlySpan.Slice(i);
				num += i;
				int start2 = num;
				if (readOnlySpan.IsEmpty)
				{
					UncaptureUntil(0);
					return false;
				}
				switch (readOnlySpan[0])
				{
				case 'D':
				case 'd':
					if ((uint)readOnlySpan.Length < 8u || !readOnlySpan.Slice(1).StartsWith("isabled", StringComparison.OrdinalIgnoreCase))
					{
						UncaptureUntil(0);
						return false;
					}
					num += 8;
					readOnlySpan = inputSpan.Slice(num);
					break;
				case 'H':
				case 'h':
					if ((uint)readOnlySpan.Length < 16u || !readOnlySpan.Slice(1).StartsWith("ighlyrestricted", StringComparison.OrdinalIgnoreCase))
					{
						UncaptureUntil(0);
						return false;
					}
					num += 16;
					readOnlySpan = inputSpan.Slice(num);
					break;
				case 'R':
				case 'r':
					if ((uint)readOnlySpan.Length < 10u || !readOnlySpan.Slice(1).StartsWith("estricted", StringComparison.OrdinalIgnoreCase))
					{
						UncaptureUntil(0);
						return false;
					}
					num += 10;
					readOnlySpan = inputSpan.Slice(num);
					break;
				case 'N':
				case 'n':
					if ((uint)readOnlySpan.Length < 6u || !readOnlySpan.Slice(1).StartsWith("ormal", StringComparison.OrdinalIgnoreCase))
					{
						UncaptureUntil(0);
						return false;
					}
					num += 6;
					readOnlySpan = inputSpan.Slice(num);
					break;
				case 'E':
				case 'e':
					if ((uint)readOnlySpan.Length < 12u || !readOnlySpan.Slice(1).StartsWith("xperimental", StringComparison.OrdinalIgnoreCase))
					{
						UncaptureUntil(0);
						return false;
					}
					num += 12;
					readOnlySpan = inputSpan.Slice(num);
					break;
				default:
					UncaptureUntil(0);
					return false;
				}
				Capture(1, start2, num);
				if (!_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.IsBoundary(inputSpan, num))
				{
					UncaptureUntil(0);
					return false;
				}
				runtextpos = num;
				Capture(0, start, num);
				return true;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void UncaptureUntil(int capturePosition)
				{
					while (Crawlpos() > capturePosition)
					{
						Uncapture();
					}
				}
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__AutoTuningValue_9 Instance = new _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__AutoTuningValue_9();

	private _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__AutoTuningValue_9()
	{
		pattern = ":\\s*(disabled|highlyrestricted|restricted|normal|experimental)\\b";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}
