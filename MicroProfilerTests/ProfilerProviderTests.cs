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
    public void Profilers_Current_Should_Return_Same_Profiler_Inside_Parallel_For()
    {
        using var pOriginal = Profilers.StartNew<TestProfilingStepsOne>();
        var res = new IProfiler<TestProfilingStepsOne>[4];
        Parallel.For(0, 4, i =>
        {
            res[i] = Profilers.Current<TestProfilingStepsOne>();
        });

        foreach(var p in res)
        {
            pOriginal.Should().BeSameAs(p);
        }
    }

    [TestMethod]
    public async Task Profilers_Current_Should_Return_Same_Profiler_Inside_Tasks()
    {
        using var pOriginal = Profilers.StartNew<TestProfilingStepsOne>();
        var tasks = new Task[4];
        var res = new IProfiler<TestProfilingStepsOne>[4];
        for(int i = 0; i < 4; ++i)
        {
            var y = i;
            tasks[y] = Task.Run(() =>
            {
                res[y] = Profilers.Current<TestProfilingStepsOne>();
            });
        }

        await Task.WhenAll(tasks);

        foreach(var p in res)
        {
            pOriginal.Should().BeSameAs(p);
        }
    }

    [TestMethod]
    public async Task Profilers_Created_From_Different_Tasks_Should_Be_Different_And_Not_Override_Original()
    {
        using var pOriginal = Profilers.StartNew<TestProfilingStepsOne>();
        var tasks = new Task[4];
        var res = new IProfiler<TestProfilingStepsOne>[4];
        var safe = new IProfiler<TestProfilingStepsOne>[4];
        try
        {
            var sem = new SemaphoreSlim(0, 3);
            var count = 0;
            for(int i = 0; i < 4; ++i)
            {
                var y = i;
                tasks[y] = Task.Run(() =>
                {
                    safe[y] = Profilers.StartNew<TestProfilingStepsOne>();
                    var nval = Interlocked.Increment(ref count);
                    if(nval != 4)
                        sem.Wait();
                    else
                        sem.Release(3);
                    res[y] = Profilers.Current<TestProfilingStepsOne>();
                });
            }

            await Task.WhenAll(tasks);

            var pFinal = Profilers.Current<TestProfilingStepsOne>();

            pOriginal.Should().BeSameAs(pFinal);
            foreach(var r in res)
            {
                pOriginal.Should().NotBeSameAs(r);
            }
            for(int i = 0; i < 4; ++i)
            {
                res[i].Should().BeSameAs(safe[i]);
                for(int j = 0; j < 4; ++j)
                {
                    if(i != j)
                        res[i].Should().NotBeSameAs(res[j]);
                }
            }
        }
        finally
        {
            foreach(var r in safe)
                if(r != null)
                    r.Dispose();
        }
    }
}