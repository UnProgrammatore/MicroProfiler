using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicroProfilerTests.TestingAssets;
using FluentAssertions;
using MicroProfiler;
using System.Collections.Concurrent;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MicroProfilerTests;

[TestClass]
public class ProfilerProviderTests
{
    [TestMethod]
    public void Profilers_Current_Should_Return_Current_Profiler()
    {
        using(var pStarted = Profilers.StartNew<TestProfilingStepsOne>())
        {
            var pCurrent = Profilers.Current<TestProfilingStepsOne>();
            pStarted.Should().BeSameAs(pCurrent);
        }
    }

    [TestMethod]
    public async Task Profilers_Current_Should_Return_Same_Profiler_After_Await() 
    {
        using(Profilers.StartNew<TestProfilingStepsOne>())
        {
            var pBefore = Profilers.Current<TestProfilingStepsOne>();
            await Task.Delay(1);
            var pAfter = Profilers.Current<TestProfilingStepsOne>();
            pBefore.Should().BeSameAs(pAfter);
        }
    }

    [TestMethod]
    public async Task Profilers_Current_Should_Return_Same_Profiler_Inside_Awaited_Func()
    {
        async Task<(IProfiler<TestProfilingStepsOne> BeforeAwait, IProfiler<TestProfilingStepsOne> AfterAwait)> DoStuff()
        {
            var pBefore = Profilers.Current<TestProfilingStepsOne>();
            await Task.Delay(1);
            var pAfter = Profilers.Current<TestProfilingStepsOne>();
            return (pBefore, pAfter);
        }
        using(Profilers.StartNew<TestProfilingStepsOne>())
        {
            var pOriginal = Profilers.Current<TestProfilingStepsOne>();
            var (pBefore, pAfter) = await DoStuff();
            pOriginal.Should().BeSameAs(pBefore).And.BeSameAs(pAfter);
        }
    }

    [TestMethod]
    public void Profilers_Current_Should_Return_Different_Profiler_Inside_Parallel_For()
    {
        using(var pOriginal = Profilers.StartNew<TestProfilingStepsOne>())
        {
            var res = new IProfiler<TestProfilingStepsOne>[2];
            Parallel.For(0, 2, i =>
            {
                res[i] = Profilers.Current<TestProfilingStepsOne>();
            });

            foreach(var p in res)
            {
                pOriginal.Should().NotBeSameAs(p);
            }
        }
    }
}