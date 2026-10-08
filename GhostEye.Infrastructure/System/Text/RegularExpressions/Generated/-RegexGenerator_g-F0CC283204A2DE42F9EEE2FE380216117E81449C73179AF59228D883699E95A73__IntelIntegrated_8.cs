using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
internal sealed class _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__IntelIntegrated_8 : Regex
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
						int num3 = readOnlySpan.Slice(num2).IndexOfAny(_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_ascii_3200000032000);
						if (num3 < 0)
						{
							break;
						}
						num2 += num3;
						if ((uint)(num2 + 1) >= (uint)readOnlySpan.Length)
						{
							break;
						}
						ulong num4;
						if ((long)((ulong)(-8646348332316098560L << (int)(num4 = (uint)(readOnlySpan[num2 + 1] - 68))) & (num4 - 64)) < 0L)
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
				int arg = 0;
				int arg2 = 0;
				int arg3 = 0;
				int arg4 = 0;
				int pos = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (!_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.IsBoundary(inputSpan, num))
				{
					UncaptureUntil(0);
					return false;
				}
				int start2 = num;
				if (span.IsEmpty)
				{
					UncaptureUntil(0);
					return false;
				}
				switch (span[0])
				{
				case 'I':
				case 'i':
					if ((uint)span.Length < 4u || !span.Slice(1).StartsWith("ris", StringComparison.OrdinalIgnoreCase))
					{
						UncaptureUntil(0);
						return false;
					}
					num += 4;
					span = inputSpan.Slice(num);
					break;
				case 'U':
				case 'u':
					if ((uint)span.Length < 3u || !span.Slice(1).StartsWith("hd", StringComparison.OrdinalIgnoreCase))
					{
						UncaptureUntil(0);
						return false;
					}
					num += 3;
					span = inputSpan.Slice(num);
					break;
				case 'H':
				case 'h':
					if ((uint)span.Length < 2u || (span[1] | 0x20) != 100)
					{
						UncaptureUntil(0);
						return false;
					}
					num += 2;
					span = inputSpan.Slice(num);
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
				int num2 = 0;
				while (true)
				{
					_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPush(ref runstack, ref pos, Crawlpos(), num);
					num2++;
					int i;
					for (i = 0; (uint)i < (uint)span.Length && char.IsWhiteSpace(span[i]); i++)
					{
					}
					span = span.Slice(i);
					num += i;
					if (!span.IsEmpty && span[0] == '(')
					{
						span = span.Slice(1);
						num++;
					}
					ulong num3;
					if (!span.IsEmpty && (long)((ulong)(-6917529024956727296L << (int)(num3 = (uint)(span[0] - 82))) & (num3 - 64)) < 0L)
					{
						num++;
						span = inputSpan.Slice(num);
						arg = num;
						if (!span.IsEmpty && (span[0] | 0x20) == 109)
						{
							span = span.Slice(1);
							num++;
						}
						arg2 = num;
						goto IL_02b7;
					}
					goto IL_0367;
					IL_0367:
					if (--num2 < 0)
					{
						UncaptureUntil(0);
						return false;
					}
					num = runstack[--pos];
					UncaptureUntil(runstack[--pos]);
					span = inputSpan.Slice(num);
					goto IL_03c6;
					IL_0347:
					_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPush(ref runstack, ref pos, arg3, arg4, Crawlpos());
					if (num2 == 0)
					{
						continue;
					}
					goto IL_03c6;
					IL_02b7:
					_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPush(ref runstack, ref pos, arg, arg2, Crawlpos());
					arg3 = num;
					if (!span.IsEmpty && span[0] == ')')
					{
						span = span.Slice(1);
						num++;
					}
					arg4 = num;
					goto IL_0347;
					IL_03c6:
					int num4 = num;
					int j;
					for (j = 0; (uint)j < (uint)span.Length && char.IsWhiteSpace(span[j]); j++)
					{
					}
					span = span.Slice(j);
					num += j;
					int num5 = num;
					while (true)
					{
						int capturePosition = Crawlpos();
						int num6 = 0;
						while (true)
						{
							_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPush(ref runstack, ref pos, Crawlpos(), num);
							num6++;
							if ((uint)span.Length < 8u || !span.StartsWith("graphics", StringComparison.OrdinalIgnoreCase))
							{
								goto IL_0487;
							}
							num += 8;
							span = inputSpan.Slice(num);
							if (num6 == 0)
							{
								continue;
							}
							goto IL_04c1;
							IL_0487:
							if (--num6 < 0)
							{
								break;
							}
							num = runstack[--pos];
							UncaptureUntil(runstack[--pos]);
							span = inputSpan.Slice(num);
							goto IL_04c1;
							IL_04c1:
							int num7 = num;
							int k;
							for (k = 0; (uint)k < (uint)span.Length && char.IsWhiteSpace(span[k]); k++)
							{
							}
							span = span.Slice(k);
							num += k;
							int num8 = num;
							while (true)
							{
								int capturePosition2 = Crawlpos();
								int num9 = 0;
								while (true)
								{
									_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPush(ref runstack, ref pos, Crawlpos(), num);
									num9++;
									int start3 = num;
									int l;
									for (l = 0; l < 4 && (uint)l < (uint)span.Length && char.IsDigit(span[l]); l++)
									{
									}
									if (l >= 3)
									{
										span = span.Slice(l);
										num += l;
										Capture(2, start3, num);
										if (num9 == 0)
										{
											continue;
										}
									}
									else
									{
										if (num9 - 1 < 0)
										{
											break;
										}
										num = runstack[--pos];
										UncaptureUntil(runstack[pos - 1]);
										span = inputSpan.Slice(num);
									}
									runtextpos = num;
									Capture(0, start, num);
									return true;
								}
								UncaptureUntil(capturePosition2);
								if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
								{
									CheckTimeout();
								}
								if (num7 >= num8)
								{
									break;
								}
								num = --num8;
								span = inputSpan.Slice(num);
							}
							goto IL_0487;
						}
						UncaptureUntil(capturePosition);
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
					if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					if (num2 == 0)
					{
						break;
					}
					UncaptureUntil(runstack[--pos]);
					_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPop(runstack, ref pos, out arg4, out arg3);
					if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					if (arg3 >= arg4)
					{
						UncaptureUntil(runstack[--pos]);
						_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPop(runstack, ref pos, out arg2, out arg);
						if (_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						if (arg < arg2)
						{
							num = --arg2;
							span = inputSpan.Slice(num);
							goto IL_02b7;
						}
						goto IL_0367;
					}
					num = --arg4;
					span = inputSpan.Slice(num);
					goto IL_0347;
				}
				UncaptureUntil(0);
				return false;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void UncaptureUntil(int num10)
				{
					while (Crawlpos() > num10)
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

	internal static readonly _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__IntelIntegrated_8 Instance = new _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__IntelIntegrated_8();

	private _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__IntelIntegrated_8()
	{
		pattern = "\\b(?<family>Iris|UHD|HD)\\b(?:\\s*\\(?[RT]M?\\)?)?\\s*(?:Graphics)?\\s*(?<model>\\d{3,4})?";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		CapNames = new Hashtable
		{
			{ "0", 0 },
			{ "family", 1 },
			{ "model", 2 }
		};
		capslist = new string[3] { "0", "family", "model" };
		capsize = 3;
	}
}
