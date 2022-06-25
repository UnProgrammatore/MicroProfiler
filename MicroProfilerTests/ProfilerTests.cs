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
        using var profiler1 = Profilers.StartNew<TestProfilingStepsForProfilerReuseTestsExclusively>();
        profiler1.Dispose();

        using var profiler2 = Profilers.StartNew<TestProfilingStepsForProfilerReuseTestsExclusively>();
        profiler1.Should().BeSameAs(profiler2);
    }

    [TestMethod]
    public void Profilers_StartNew_Should_Use_Different_Profilers_If_Both_Are_Active()
    {
        using var profiler1 = Profilers.StartNew<TestProfilingStepsOne>();
        using var profiler2 = Profilers.StartNew<TestProfilingStepsOne>();

        profiler1.Should().NotBeSameAs(profiler2);
    }

    [TestMethod]
    public void Profilers_For_Different_Enums_Should_Be_Different() 
    {
        using var profiler1 = Profilers.StartNew<TestProfilingStepsOne>();
        profiler1.Dispose();

        using var profiler2 = Profilers.StartNew<TestProfilingStepsTwo>();
        profiler1.Should().NotBeSameAs(profiler2);
    }

    [TestMethod]
    public void Profilers_Should_Be_Different_When_Requested_Simultaneously()
    {
        IProfiler<TestProfilingStepsParallel>[]? profilers = null;
        try
        {
            var r = new Random();
            Func<IProfiler<TestProfilingStepsParallel>> makeFunc = () =>
            {
                Thread.Sleep(r.Next(5, 10));
                return Profilers.StartNew<TestProfilingStepsParallel>();
            };
            var tasks = new Task<IProfiler<TestProfilingStepsParallel>>[100];
            for(int i = 0; i < 100; ++i)
            {
                tasks[i] = Task.Run(makeFunc);
            }

            profilers = Task.WhenAll(tasks).GetAwaiter().GetResult();

            profilers.Length.Should().Be(100);

            for(int i = 0; i < profilers.Length; ++i)
                for(int j = 0; j < profilers.Length; j++)
                    if(i != j)
                        profilers[i].Should().NotBeSameAs(profilers[j]);
        }
        finally
        {
            if(profilers != null)
                foreach(var profiler in profilers)
                    profiler.Dispose();
        }
    }

    [TestMethod]
    public void Profielrs_Should_Be_Clean_When_Reused() 
    {
        using var profiler1 = Profilers.StartNew<TestProfilingStepsForProfilerReuseTestsExclusively>();
        using(profiler1.Step(TestProfilingStepsForProfilerReuseTestsExclusively.FirstStep))
        {
            Thread.Sleep(1);
        }
        profiler1.GetData().Size.Should().Be(1);
        profiler1.Dispose();

        using var profiler2 = Profilers.StartNew<TestProfilingStepsForProfilerReuseTestsExclusively>();
        profiler2.Should().BeSameAs(profiler1);
        profiler2.GetData().Size.Should().Be(0);
    }

    [TestMethod]
    public void Profiler_Should_Track_Multiple_Steps_Correctly()
    {
        using var profiler = Profilers.StartNew<TestProfilingStepsOne>();
        using(profiler.Step(TestProfilingStepsOne.FirstStep))
        {
            Thread.Sleep(1);
            using(profiler.Step(TestProfilingStepsOne.SecondStep))
            {
                Thread.Sleep(1);
                using(profiler.Step(TestProfilingStepsOne.ThirdStep))
                {
                    Thread.Sleep(1);
                }
            }
        }

        var (data, length) = profiler.GetData();
        length.Should().Be(3);
        for(int i = 0; i < length; ++i)
        {
            switch(data[i].StepType)
            {
                case TestProfilingStepsOne.FirstStep:
                    (data[i].EndTime - data[i].StartTime).Should().BeGreaterThan(TimeSpan.FromMilliseconds(3));
                    break;
                case TestProfilingStepsOne.SecondStep:
                    (data[i].EndTime - data[i].StartTime).Should().BeGreaterThan(TimeSpan.FromMilliseconds(2));
                    data[i].StartTime.Should().BeGreaterThan(TimeSpan.FromMilliseconds(1));
                    break;
                case TestProfilingStepsOne.ThirdStep:
                    (data[i].EndTime - data[i].StartTime).Should().BeGreaterThan(TimeSpan.FromMilliseconds(1));
                    data[i].StartTime.Should().BeGreaterThan(TimeSpan.FromMilliseconds(2));
                    break;
            }
        }
    }
}