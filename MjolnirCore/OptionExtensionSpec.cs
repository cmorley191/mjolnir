using System;
using System.Collections.Generic;
using NUnit.Framework;
using MjolnirCore.Extensions;
using Microsoft.FSharp.Core;


namespace MjolnirCore {

	[TestFixture]
	public class OptionExtensionSpec {
		
		private readonly FSharpOption<int> nullOption = null;
		private readonly FSharpOption<int> nonNullOption = new FSharpOption<int>(1);

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

	}
}
