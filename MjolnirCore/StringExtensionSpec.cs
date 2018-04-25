using System;
using System.Collections.Generic;
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

		[Test]
		public void ConvertAKeyValuePairToAnObjectString() {
			var arbitrarypair = new KeyValuePair<int, string>(30, "i print this when tested!");
			Assert.AreEqual("{ 30, i print this when tested! }", arbitrarypair.ToObjectString());
		}

		[Test]
		public void ConvertADictionaryToADictString() {
			var arbitrarypair1 = new KeyValuePair<int, string>(30, "i print this when tested!");
			var arbitrarypair2 = new KeyValuePair<int, string>(31, "why have you done this?");

			var dict = new Dictionary<int, string>();
			dict.Add(arbitrarypair1.Key, arbitrarypair1.Value);
			dict.Add(arbitrarypair2.Key, arbitrarypair2.Value);

			var expectedString = "{\n\t{ 30, i print this when tested! },\n\t{ 31, why have you done this? }\n}";
			Assert.AreEqual(expectedString, dict.ToDictString());
		}

		private class NonTObj {
			private readonly string testField;
			public NonTObj() {
				this.testField = "not returned by a ToString()";
			}
		}
		
    }
}
