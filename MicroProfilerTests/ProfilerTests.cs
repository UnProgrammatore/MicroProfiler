using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicroProfilerTests.TestingAssets;
using FluentAssertions;
using MicroProfiler;

namespace MicroProfilerTests;

[TestClass]
public class ProfilerTests
{
    [TestMethod]
    public void Profilers_StartNew_Should_Get_New_Profiler_With_No_Data()
    {
        using var profiler = Profilers.StartNew<TestProfilingStepsOne>();
        profiler.GetData().Size.Should().Be(0);
    }

    [TestMethod]
    public void Profilers_StartNew_Should_Reuse_Free_Profilers()
    {
        using var profiler1 = Profilers.StartNew<TestProfilingStepsForProfilerReuseTestExclusively>();
        profiler1.Dispose();

        using var profiler2 = Profilers.StartNew<TestProfilingStepsForProfilerReuseTestExclusively>();
        profiler1.Should().BeSameAs(profiler2);
    }

    [TestMethod]
    public void Profilers_StartNew_Should_Use_Different_Profilers_If_Both_Are_Active()
    {
        using var profiler1 = Profilers.StartNew<TestProfilingStepsOne>();
        using var profiler2 = Profilers.StartNew<TestProfilingStepsTwo>();

        profiler1.Should().NotBeSameAs(profiler2);
    }
}