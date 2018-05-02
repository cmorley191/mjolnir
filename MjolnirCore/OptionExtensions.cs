using Microsoft.FSharp.Core;
using Mjolnir.Core;
using System;
using System.Threading.Tasks;

namespace MjolnirCore {
    public static class OptionExtensions {
        public static FSharpOption<T> Some<T>(T it) => FSharpOption<T>.Some(it);
        public static FSharpOption<T> None<T>() => FSharpOption<T>.None;

        public static bool IsSome<T>(this FSharpOption<T> o) => Option.IsSome(o);
        public static bool IsSome<T>(this FSharpOption<T> o, Func<T, bool> predicate) =>
            o.IsSome() && predicate(o.Value);
        public static bool IsNone<T>(this FSharpOption<T> o) => Option.IsNone(o);

        public static bool TryGetValue<T>(this FSharpOption<T> o, out T value) => Option.TryGetValue(o, out value);

        public static void Match<T>(this FSharpOption<T> o, Action<T> some, Action none) {
            if (o.IsSome())
                some.Invoke(o.Value);
            else
                none.Invoke();
        }
        public static async Task Match<T>(this FSharpOption<T> o, Func<T, Task> some, Func<Task> none) {
            if (o.IsSome())
                await some.Invoke(o.Value);
            else
                await none.Invoke();
        }

        public static TOut Match<TOp, TOut>(this FSharpOption<TOp> o, Func<TOp, TOut> some, Func<TOut> none) =>
            (o.IsSome()) ? some.Invoke(o.Value) : none.Invoke();
        public static Task<TOut> Match<TOp, TOut>(this FSharpOption<TOp> o, Func<TOp, Task<TOut>> some, Func<Task<TOut>> none) =>
            (o.IsSome()) ? some.Invoke(o.Value) : none.Invoke();

        public static FSharpOption<TOut> Map<TIn, TOut>(this FSharpOption<TIn> o, Func<TIn, TOut> f) =>
            (o.IsSome()) ? Some(f(o.Value)) : None<TOut>();
        public static T Default<T>(this FSharpOption<T> o, T defaultValue) =>
            (o.IsSome()) ? o.Value : defaultValue;

        public static void IfSome<T>(this FSharpOption<T> o, Action<T> act) {
            if (o.IsSome())
                act(o.Value);
        }
    }
}