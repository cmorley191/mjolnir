using Microsoft.FSharp.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MjolnirCore {
    public static class EnumerableExtensions {
        public static IEnumerable<TOp> SelectSome<TOp>(this IEnumerable<FSharpOption<TOp>> it) =>
            it.Where(o => o.IsSome()).Select(o => o.Value);
    }
}
