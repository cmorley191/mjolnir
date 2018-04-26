using Microsoft.FSharp.Core;
using Mjolnir.Core;
using System;

namespace MjolnirCore {
    public static class OptionExtensions {
        public static bool IsSome<T>(this FSharpOption<T> o) => Option.IsSome(o);
        public static bool IsSome<T>(this FSharpOption<T> o, Func<T, bool> predicate) => o.IsSome() && predicate(o.Value);
        public static bool IsNone<T>(this FSharpOption<T> o) => Option.IsNone(o);
        public static bool TryGetValue<T>(this FSharpOption<T> o, out T value) => Option.TryGetValue(o, out value);
        public static void IfSome<T>(this FSharpOption<T> o, Action<T> act) {
            if (o.IsSome())
                act(o.Value);
        }
    }
}