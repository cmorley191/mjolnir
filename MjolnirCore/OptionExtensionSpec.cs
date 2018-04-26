using System;
using System.Collections.Generic;
using NUnit.Framework;
using MjolnirCore.Extensions;
using static MjolnirCore.OptionExtensions;
using Microsoft.FSharp.Core;


namespace MjolnirCore {

    [TestFixture]
    public class OptionExtensionSpec {

        private readonly FSharpOption<int> nullOption = null;
        private readonly FSharpOption<int> nonNullOption = new FSharpOption<int>(1);

        [Test]
        public void CanCreateOptionsWithoutTypingSoMuch() {
            Assert.AreEqual(FSharpOption<int>.Some(2), Some(2));
            Assert.AreEqual(FSharpOption<int>.None, None<int>());
        }

        [Test]

        public void IsNoneOnNullObjectIsTrue() {
            Assert.IsTrue(nullOption.IsNone());
        }

        [Test]
        public void IsNoneOnNonNullObjectIsFalse() {
            Assert.IsFalse(nonNullOption.IsNone());
        }

        [Test]
        public void IsSomeOnNonNullObjectIsTrue() {
            Assert.IsTrue(nonNullOption.IsSome());
        }

        [Test]
        public void IsSomeOnNullObjectIsFalse() {
            Assert.IsFalse(nullOption.IsSome());
        }

        [Test]
        public void TryGetValueOnNonNullObjectCanGetValue() {
            int foo;
            Assert.IsTrue(nonNullOption.TryGetValue(out foo));
            Assert.AreEqual(1, foo);
        }

        [Test]
        public void TryGetValueOnNullObjectReturnsFalse() {
            int foo;
            Assert.IsFalse(nullOption.TryGetValue(out foo));
        }

        [Test]
        public void IfSomeCanPerformAnActionWhenSome() {
            var x = 0;
            FSharpOption<int>.Some(2).IfSome(v => x = v);
            Assert.AreEqual(2, x, "Action was not performed.");
        }

        [Test]
        public void IfSomeDoesntPerformAnActionWhenNone() {
            var x = 0;
            FSharpOption<int>.None.IfSome(v => x = v);
            Assert.AreEqual(0, x, "Something happened when nothing was supposed to happen.");
        }
    }
}
