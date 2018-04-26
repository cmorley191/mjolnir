using System;
using System.Collections.Generic;
using NUnit.Framework;
using MjolnirCore.Extensions;
using static MjolnirCore.OptionExtensions;
using Microsoft.FSharp.Core;

namespace MjolnirCore.Extensions.Specs {

    [TestFixture]
    public class EnumerableExtensionsSpec {

        [Test]
        public void SelectSomeSelectsOnlyOptionsWithValue() {
            Assert.AreEqual(
                new[] { 2, 5, 11 },
                new[] { Some(2), None<int>(), Some(5), Some(11), None<int>() }
                    .SelectSome());

            Assert.AreEqual(new int[0], new[] { None<int>(), None<int>() }.SelectSome());
            Assert.AreEqual(new int[0], new FSharpOption<int>[0].SelectSome());
        }
    }
}
