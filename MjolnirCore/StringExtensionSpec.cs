using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MjolnirCore.Extensions;

namespace MjolnirCore {

	[TestFixture]
    public class StringExtensionSpec {
		
		[Test]
		public void TurnsNullObjectIntoStringNull() {
			Object nullObject = null;
			Assert.AreEqual("null", nullObject.ToObjectString());
			Assert.AreNotEqual("null", new Object().ToObjectString());
		}

		[Test]
		public void NonNullObjectsTurnsToObjectToString() {
			var object1 = "this is my 2 string";
			var object2 = 47;
			Assert.AreEqual("this is my 2 string", object1.ToObjectString());
			Assert.AreEqual("47", object2.ToObjectString());
		}

		[Test]
		public void CustomObjectToObjectStringReturnsObjectName() {
			var foo = new NonTObj();
			Assert.AreEqual("MjolnirCore.StringExtensionSpec+NonTObj", foo.ToObjectString());
			Assert.AreNotEqual("not returned by a ToString()", foo.ToObjectString());
		}

		private class NonTObj {
			private readonly string testField;
			public NonTObj() {
				this.testField = "not returned by a ToString()";
			}
		}
		
    }
}
