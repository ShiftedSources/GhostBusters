using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
internal sealed class _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__PercentPattern_4 : Regex
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
					int num2 = inputSpan.Slice(num).IndexOfAnyDigit();
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
				ReadOnlySpan<char> readOnlySpan = inputSpan.Slice(num);
				int start2 = num;
				int num2 = num;
				int i;
				for (i = 0; i < 3 && (uint)i < (uint)readOnlySpan.Length && char.IsDigit(readOnlySpan[i]); i++)
				{
				}
				if (i == 0)
				{
					UncaptureUntil(0);
					return false;
				}
				readOnlySpan = readOnlySpan.Slice(i);
				num += i;
				int num3 = num;
				num2++;
				while (true)
				{
					int capturePosition = Crawlpos();
					int num4 = 0;
					while (true)
					{
						_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.StackPush(ref runstack, ref pos, Crawlpos(), num);
						num4++;
						if (!readOnlySpan.IsEmpty && (readOnlySpan[0] | 2) == 46)
						{
							num++;
							readOnlySpan = inputSpan.Slice(num);
							int j;
							for (j = 0; (uint)j < (uint)readOnlySpan.Length && char.IsDigit(readOnlySpan[j]); j++)
							{
							}
							if (j != 0)
							{
								readOnlySpan = readOnlySpan.Slice(j);
								num += j;
								if (num4 == 0)
								{
									continue;
								}
								goto IL_0179;
							}
						}
						goto IL_013f;
						IL_0179:
						Capture(1, start2, num);
						if (!readOnlySpan.IsEmpty && char.IsWhiteSpace(readOnlySpan[0]))
						{
							readOnlySpan = readOnlySpan.Slice(1);
							num++;
						}
						if (!readOnlySpan.IsEmpty && readOnlySpan[0] == '%')
						{
							Capture(0, start, runtextpos = num + 1);
							return true;
						}
						goto IL_013f;
						IL_013f:
						if (--num4 < 0)
						{
							break;
						}
						num = runstack[--pos];
						UncaptureUntil(runstack[--pos]);
						readOnlySpan = inputSpan.Slice(num);
						goto IL_0179;
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
					readOnlySpan = inputSpan.Slice(num);
				}
				UncaptureUntil(0);
				return false;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void UncaptureUntil(int num5)
				{
					while (Crawlpos() > num5)
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

	internal static readonly _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__PercentPattern_4 Instance = new _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__PercentPattern_4();

	private _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__PercentPattern_4()
	{
		pattern = "(\\d{1,3}(?:[.,]\\d+)?)\\s?%";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}
