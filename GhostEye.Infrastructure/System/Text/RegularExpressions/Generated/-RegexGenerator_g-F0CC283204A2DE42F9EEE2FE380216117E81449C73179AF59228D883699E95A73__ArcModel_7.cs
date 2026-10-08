using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
internal sealed class _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__ArcModel_7 : Regex
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
					int num2 = inputSpan.Slice(num).IndexOf("arc", StringComparison.OrdinalIgnoreCase);
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
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (!_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.IsBoundary(inputSpan, num))
				{
					UncaptureUntil(0);
					return false;
				}
				if ((uint)span.Length < 3u || !span.StartsWith("arc", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				if (!_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.IsBoundary(inputSpan, num + 3))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 3;
				span = inputSpan.Slice(num);
				int num2 = num;
				while (true)
				{
					int capturePosition = Crawlpos();
					if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.IsBoundary(inputSpan, num))
					{
						int start2 = num;
						if ((uint)span.Length >= 4u && (uint)((span[0] | 0x20) - 97) <= 1u && char.IsDigit(span[1]) && char.IsDigit(span[2]) && char.IsDigit(span[3]))
						{
							num += 4;
							inputSpan.Slice(num);
							Capture(1, start2, num);
							if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.IsBoundary(inputSpan, num))
							{
								break;
							}
						}
					}
					UncaptureUntil(capturePosition);
					if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					num = num2;
					span = inputSpan.Slice(num);
					if (span.IsEmpty || span[0] == '\n')
					{
						UncaptureUntil(0);
						return false;
					}
					num++;
					span = inputSpan.Slice(num);
					num2 = num;
				}
				runtextpos = num;
				Capture(0, start, num);
				return true;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void UncaptureUntil(int num3)
				{
					while (Crawlpos() > num3)
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

	internal static readonly _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__ArcModel_7 Instance = new _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__ArcModel_7();

	private _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__ArcModel_7()
	{
		pattern = "\\bArc\\b.*?\\b(?<model>[AB]\\d{3})\\b";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		CapNames = new Hashtable
		{
			{ "0", 0 },
			{ "model", 1 }
		};
		capslist = new string[2] { "0", "model" };
		capsize = 2;
	}
}
