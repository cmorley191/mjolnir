using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MjolnirCore.Extensions {
  public static class StringExtensions {

    public static string ToObjectString<TObj>(this TObj it) {
      var str = "";

      if (it == null)
        str += "null";
      else
        str += it.ToString();

      if (it != null && it.GetType() != typeof(TObj))
        str += $" : {it.GetType().FullName}";

      return str;
    }

    public static string ToObjectString<TKey, TValue>(this KeyValuePair<TKey, TValue> it) =>
      "{ " + it.Key.ToObjectString() + ", " + it.Value.ToObjectString() + " }";

    public static string ToDictString<TKey, TValue>(this Dictionary<TKey, TValue> dict) {
      return "{\n" +
        "\t" + string.Join(",\n\t", dict.Select(pair => pair.ToObjectString())) +
        "\n}";
    }
  }
}
