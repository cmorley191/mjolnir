using Microsoft.FSharp.Core;
using Mjolnir.Core;

namespace MjolnirCore
{
	public static class OptionExtensions
	{
		public static bool IsSome<T>(this FSharpOption<T> o) => Option.IsSome(o);
		public static bool IsNone<T>(this FSharpOption<T> o) => Option.IsNone(o);
		public static bool TryGetValue<T>(this FSharpOption<T> o, out T value) => Option.TryGetValue(o, out value);
	}
}