using System.Collections.Generic;

namespace KoheiUtils.Tests
{
    using NUnit.Framework;
    using KoheiUtils;

    public class Tests_DefaultDict
    {
        [Test]
        public void Test_DefaultDictExample()
        {
            var dict1 = new DefaultDictionary<string, int>();

            Assert.AreEqual(dict1["apple"], 0);
            dict1["banana"] += 10;
            Assert.AreEqual(dict1["banana"], 10);
            dict1["apple"] += 3;
            Assert.AreEqual(dict1["apple"], 3);
        }

        [Test]
        public void Test_DefaultDictExample_List()
        {
            var dict1 = new DefaultDictionary<string, List<int>>();

            Assert.IsTrue(dict1["apple"].Count == 0);

            dict1["banana"].Add(32);
            dict1["banana"].Add(9);
            Assert.AreEqual(32, dict1["banana"][0]);
            Assert.AreEqual(9, dict1["banana"][1]);
            Assert.AreEqual(2, dict1["banana"].Count);

            dict1["apple"].Add(203);
            Assert.AreEqual(203, dict1["apple"][0]);
            
            dict1["banana"].RemoveAt(0);
            Assert.AreEqual(1, dict1["banana"].Count);
            Assert.AreEqual(9, dict1["banana"][0]);
        }
    }
}