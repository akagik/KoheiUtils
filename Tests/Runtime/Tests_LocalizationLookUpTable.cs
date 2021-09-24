using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace KoheiUtils.Tests
{
    public class Tests_LocalizationLookUpTable
    {
        [Test]
        public void Test_LookUpTable()
        {
            var localization = new LocalizationLookUpTable();

            var table0 = ScriptableObject.CreateInstance<LocalizationTable>();
            table0.rows = new List<LocalizationData>();
            table0.rows.Add(new LocalizationData()
            {
                key = "hello",
                en = "Hello!",
                ja = "こんにちは!",
            });
            table0.rows.Add(new LocalizationData()
            {
                key = "bye",
                en = "Bye!",
                ja = "さようなら!",
            });
            table0.rows.Add(new LocalizationData()
            {
                key = "tell",
                en = "Tell {0}",
                ja = "{0}に伝えて",
            });

            localization.AddLocalizationTable(table0);

            localization.SetLanguage("ja");
            Assert.IsTrue(localization.ContainsKey("hello"));
            Assert.IsFalse(localization.ContainsKey("hello2"));

            Assert.AreEqual("さようなら!", localization.Get("bye"));

            if (localization.TryGetValue("hello", out var result))
            {
                Assert.AreEqual("こんにちは!", result);
            }

            Assert.IsFalse(localization.TryGetValue("hello2", out var result1));

            // 英語に切り替え
            localization.SetLanguage("en");
            Assert.AreEqual("Bye!", localization.Get("bye"));
            Assert.AreEqual("Hello!", localization.Get("hello"));

            Assert.AreEqual("HELLO", localization.Get("hello2", "HELLO"));
            Assert.AreEqual("Hello!", localization.Get("hello", "HELLO"));
            Assert.AreEqual("Tell me", localization.Format("tell", "me"));
        }
    }
}