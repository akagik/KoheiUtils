using NUnit.Framework;
using KoheiUtils;
using KoheiUtils.Reflections;

public class Test_EditorKoheiUtils
{
    [Test]
    public void Test_MethodReflection1()
    {
        bool result1 = MethodReflection.TryParse("KoheiUtils.KoheiEditorTools.CaptureScreenshot", out var info, suppressLog: true);

        Assert.IsFalse(result1);
        // Assert.AreEqual(typeof(KoheiEditorTools), info.type);
        // Assert.AreEqual(typeof(KoheiEditorTools).GetMethod("CaptureScreenshot"), info.methodInfo);
    }

    [Test]
    public void Test_MethodReflectionWithAssembly()
    {
        bool result1 = MethodReflection.TryParse("KoheiUtils.KoheiEditorTools.CaptureScreenshot, Kohei.KoheiUtils.Editor", out var info);

        Assert.IsTrue(result1);
        Assert.AreEqual(typeof(KoheiEditorTools), info.type);
        Assert.AreEqual(typeof(KoheiEditorTools).GetMethod("CaptureScreenshot"), info.methodInfo);
    }

    [Test]
    public void Test_TypeReflection()
    {
        // bool result1 = TypeReflection.TryParse("KoheiUtils.KoheiEditorTools", out var info);

        // Assert.IsFalse(result1);
        // Assert.AreEqual(typeof(KoheiEditorTools), info.type);
    }

    [Test]
    public void Test_TypeReflectionWithAssembly()
    {
        bool result1 = TypeReflection.TryParse("KoheiUtils.KoheiEditorTools, Kohei.KoheiUtils.Editor", out var info);

        Assert.IsTrue(result1);
        Assert.AreEqual(typeof(KoheiEditorTools), info.type);
    }
}