using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
internal sealed class _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__AmdModel_6 : Regex
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
				if (num <= inputSpan.Length - 5)
				{
					int num2 = inputSpan.Slice(num).IndexOf("rx", StringComparison.OrdinalIgnoreCase);
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
				int pos = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (!_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.IsBoundary(inputSpan, num))
				{
					UncaptureUntil(0);
					return false;
				}
				if ((uint)span.Length < 2u || !span.StartsWith("rx", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				int i;
				for (i = 2; (uint)i < (uint)span.Length && char.IsWhiteSpace(span[i]); i++)
				{
				}
				span = span.Slice(i);
				num += i;
				int start2 = num;
				int num2 = num;
				int j;
				for (j = 0; j < 4 && (uint)j < (uint)span.Length && char.IsDigit(span[j]); j++)
				{
				}
				if (j < 3)
				{
					UncaptureUntil(0);
					return false;
				}
				span = span.Slice(j);
				num += j;
				int num3 = num;
				num2 += 3;
				while (true)
				{
					int capturePosition = Crawlpos();
					Capture(1, start2, num);
					int num4 = num;
					int k;
					for (k = 0; (uint)k < (uint)span.Length && char.IsWhiteSpace(span[k]); k++)
					{
					}
					span = span.Slice(k);
					num += k;
					int num5 = num;
					while (true)
					{
						int capturePosition2 = Crawlpos();
						int num6 = 0;
						while (true)
						{
							_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPush(ref runstack, ref pos, Crawlpos(), num);
							num6++;
							int num7 = num;
							int arg = num;
							int arg2 = Crawlpos();
							if ((uint)span.Length < 3u || !span.StartsWith("xtx", StringComparison.OrdinalIgnoreCase))
							{
								goto IL_0220;
							}
							_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPush(ref runstack, ref pos, 0, arg, arg2);
							num += 3;
							span = inputSpan.Slice(num);
							goto IL_036b;
							IL_03f3:
							if (!_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.IsBoundary(inputSpan, num))
							{
								if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
								{
									CheckTimeout();
								}
								if (num6 != 0)
								{
									num7 = runstack[--pos];
									if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
									{
										CheckTimeout();
									}
									_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPop(runstack, ref pos, out arg2, out arg);
									switch (runstack[--pos])
									{
									case 0:
										break;
									case 1:
										goto IL_0278;
									case 2:
										goto IL_02d0;
									default:
										goto IL_036b;
									case 3:
										goto IL_03a1;
									}
									goto IL_0220;
								}
								break;
							}
							runtextpos = num;
							Capture(0, start, num);
							return true;
							IL_03a1:
							if (--num6 < 0)
							{
								break;
							}
							num = runstack[--pos];
							UncaptureUntil(runstack[--pos]);
							span = inputSpan.Slice(num);
							goto IL_03f3;
							IL_0220:
							num = arg;
							span = inputSpan.Slice(num);
							UncaptureUntil(arg2);
							if ((uint)span.Length < 2u || !span.StartsWith("xt", StringComparison.OrdinalIgnoreCase))
							{
								goto IL_0278;
							}
							_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPush(ref runstack, ref pos, 1, arg, arg2);
							num += 2;
							span = inputSpan.Slice(num);
							goto IL_036b;
							IL_02d0:
							num = arg;
							span = inputSpan.Slice(num);
							UncaptureUntil(arg2);
							if (!span.IsEmpty && (span[0] | 0x20) == 109)
							{
								_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPush(ref runstack, ref pos, 3, arg, arg2);
								num++;
								span = inputSpan.Slice(num);
								goto IL_036b;
							}
							goto IL_03a1;
							IL_036b:
							Capture(2, num7, num);
							_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPush(ref runstack, ref pos, num7);
							if (num6 == 0)
							{
								continue;
							}
							goto IL_03f3;
							IL_0278:
							num = arg;
							span = inputSpan.Slice(num);
							UncaptureUntil(arg2);
							if ((uint)span.Length < 3u || !span.StartsWith("gre", StringComparison.OrdinalIgnoreCase))
							{
								goto IL_02d0;
							}
							_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPush(ref runstack, ref pos, 2, arg, arg2);
							num += 3;
							span = inputSpan.Slice(num);
							goto IL_036b;
						}
						UncaptureUntil(capturePosition2);
						if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						if (num4 >= num5)
						{
							break;
						}
						num = --num5;
						span = inputSpan.Slice(num);
					}
					UncaptureUntil(capturePosition);
					if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					if (num2 >= num3)
					{
						break;
					}
					num = --num3;
					span = inputSpan.Slice(num);
				}
				UncaptureUntil(0);
				return false;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void UncaptureUntil(int num8)
				{
					while (Crawlpos() > num8)
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

	internal static readonly _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__AmdModel_6 Instance = new _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__AmdModel_6();

	private _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__AmdModel_6()
	{
		pattern = "\\bRX\\s*(?<model>\\d{3,4})\\s*(?<suffix>XTX|XT|GRE|M)?\\b";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		CapNames = new Hashtable
		{
			{ "0", 0 },
			{ "model", 1 },
			{ "suffix", 2 }
		};
		capslist = new string[3] { "0", "model", "suffix" };
		capsize = 3;
	}
}
