using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
internal sealed class _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__NvidiaModel_5 : Regex
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
					ReadOnlySpan<char> readOnlySpan = inputSpan.Slice(num);
					int num2;
					for (num2 = 0; num2 < readOnlySpan.Length - 4; num2++)
					{
						int num3 = readOnlySpan.Slice(num2).IndexOfAny(_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_ascii_8000040080000400);
						if (num3 < 0)
						{
							break;
						}
						num2 += num3;
						if ((uint)(num2 + 1) >= (uint)readOnlySpan.Length)
						{
							break;
						}
						if ((readOnlySpan[num2 + 1] | 0x20) == 116)
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
				int num2 = 0;
				int capturePosition = 0;
				int num3 = 0;
				int pos = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (!_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.IsBoundary(inputSpan, num))
				{
					UncaptureUntil(0);
					return false;
				}
				int start2 = num;
				int num4 = num;
				int capturePosition2 = Crawlpos();
				if ((uint)span.Length < 3u || !span.StartsWith("rtx", StringComparison.OrdinalIgnoreCase))
				{
					goto IL_0094;
				}
				int num5 = 0;
				num += 3;
				span = inputSpan.Slice(num);
				goto IL_018e;
				IL_0173:
				num5 = 1;
				goto IL_018e;
				IL_0094:
				num = num4;
				span = inputSpan.Slice(num);
				UncaptureUntil(capturePosition2);
				if (span.IsEmpty || (span[0] | 0x20) != 103)
				{
					UncaptureUntil(0);
					return false;
				}
				num3 = num;
				capturePosition = Crawlpos();
				if ((uint)span.Length < 3u || !span.Slice(1).StartsWith("tx", StringComparison.OrdinalIgnoreCase))
				{
					goto IL_010c;
				}
				num2 = 0;
				num += 3;
				span = inputSpan.Slice(num);
				goto IL_0173;
				IL_0156:
				if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
				{
					CheckTimeout();
				}
				if (num2 == 0)
				{
					goto IL_010c;
				}
				if (num2 == 1)
				{
					UncaptureUntil(0);
					return false;
				}
				goto IL_0173;
				IL_018e:
				while (true)
				{
					Capture(1, start2, num);
					int i;
					for (i = 0; (uint)i < (uint)span.Length && char.IsWhiteSpace(span[i]); i++)
					{
					}
					span = span.Slice(i);
					num += i;
					int start3 = num;
					int num6 = num;
					int j;
					for (j = 0; j < 4 && (uint)j < (uint)span.Length && char.IsDigit(span[j]); j++)
					{
					}
					if (j >= 3)
					{
						span = span.Slice(j);
						num += j;
						int num7 = num;
						num6 += 3;
						while (true)
						{
							int capturePosition3 = Crawlpos();
							Capture(2, start3, num);
							int num8 = num;
							int k;
							for (k = 0; (uint)k < (uint)span.Length && char.IsWhiteSpace(span[k]); k++)
							{
							}
							span = span.Slice(k);
							num += k;
							int num9 = num;
							while (true)
							{
								int capturePosition4 = Crawlpos();
								int num10 = 0;
								while (true)
								{
									_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPush(ref runstack, ref pos, Crawlpos(), num);
									num10++;
									int start4 = num;
									if (!span.IsEmpty)
									{
										switch (span[0])
										{
										case 'T':
										case 't':
											if ((uint)span.Length < 2u || (span[1] | 0x20) != 105)
											{
												break;
											}
											num += 2;
											span = inputSpan.Slice(num);
											goto IL_03b2;
										case 'S':
										case 's':
											if ((uint)span.Length < 5u || !span.Slice(1).StartsWith("uper", StringComparison.OrdinalIgnoreCase))
											{
												break;
											}
											num += 5;
											span = inputSpan.Slice(num);
											goto IL_03b2;
										case 'M':
										case 'm':
											num++;
											span = inputSpan.Slice(num);
											goto IL_03b2;
										}
									}
									goto IL_03c5;
									IL_03ff:
									if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.IsBoundary(inputSpan, num))
									{
										runtextpos = num;
										Capture(0, start, num);
										return true;
									}
									goto IL_03c5;
									IL_03b2:
									Capture(3, start4, num);
									if (num10 == 0)
									{
										continue;
									}
									goto IL_03ff;
									IL_03c5:
									if (--num10 < 0)
									{
										break;
									}
									num = runstack[--pos];
									UncaptureUntil(runstack[--pos]);
									span = inputSpan.Slice(num);
									goto IL_03ff;
								}
								UncaptureUntil(capturePosition4);
								if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
								{
									CheckTimeout();
								}
								if (num8 >= num9)
								{
									break;
								}
								num = --num9;
								span = inputSpan.Slice(num);
							}
							UncaptureUntil(capturePosition3);
							if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
							{
								CheckTimeout();
							}
							if (num6 >= num7)
							{
								break;
							}
							num = --num7;
							span = inputSpan.Slice(num);
						}
					}
					if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					if (num5 == 0)
					{
						break;
					}
					if (num5 != 1)
					{
						continue;
					}
					goto IL_0156;
				}
				goto IL_0094;
				IL_010c:
				num = num3;
				span = inputSpan.Slice(num);
				UncaptureUntil(capturePosition);
				if ((uint)span.Length < 2u || (span[1] | 0x20) != 116)
				{
					UncaptureUntil(0);
					return false;
				}
				num2 = 1;
				num += 2;
				span = inputSpan.Slice(num);
				goto IL_0173;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void UncaptureUntil(int num11)
				{
					while (Crawlpos() > num11)
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

	internal static readonly _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__NvidiaModel_5 Instance = new _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__NvidiaModel_5();

	private _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__NvidiaModel_5()
	{
		pattern = "\\b(?<series>RTX|GTX|GT)\\s*(?<model>\\d{3,4})\\s*(?<suffix>Ti|SUPER|M)?\\b";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		CapNames = new Hashtable
		{
			{ "0", 0 },
			{ "model", 2 },
			{ "series", 1 },
			{ "suffix", 3 }
		};
		capslist = new string[4] { "0", "series", "model", "suffix" };
		capsize = 4;
	}
}
