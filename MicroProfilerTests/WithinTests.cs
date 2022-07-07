using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicroProfilerTests.TestingAssets;
using FluentAssertions;
using MicroProfiler;
using System.Collections.Concurrent;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MicroProfilerTests;

[TestClass]
public class WithinTest
{
    [TestMethod]
    public void Within_Should_Be_Correct_In_Linear_Steps()
    {
        using var profiler = Profilers.StartNew<TestProfilingStepsOne>();

        using(profiler.Step(TestProfilingStepsOne.FirstStep))
        {
            using(profiler.Step(TestProfilingStepsOne.SecondStep))
            {
                using(profiler.Step(TestProfilingStepsOne.ThirdStep))
                {
                    Thread.Sleep(1);
                }
            }   
        }

        var (steps, size) = profiler.GetData();
        size.Should().Be(3);
        var foundSteps = new List<TestProfilingStepsOne>();
        for(int i = 0; i < 3; ++i)
        {
            foundSteps.Add(steps[i].StepType);
            switch(steps[i].StepType)
            {
                case TestProfilingStepsOne.FirstStep:
                    steps[i].WithinIndex.Should().Be(-1);
                    break;
                case TestProfilingStepsOne.SecondStep:
                    steps[steps[i].WithinIndex].StepType.Should().Be(TestProfilingStepsOne.FirstStep);
                    break;
                case TestProfilingStepsOne.ThirdStep:
                    steps[steps[i].WithinIndex].StepType.Should().Be(TestProfilingStepsOne.SecondStep);
                    break;
            }
        }

        foundSteps.Should().Contain(new[] { TestProfilingStepsOne.FirstStep, TestProfilingStepsOne.SecondStep, TestProfilingStepsOne.ThirdStep });
    }
}