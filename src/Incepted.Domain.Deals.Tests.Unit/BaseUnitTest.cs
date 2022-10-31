using FluentAssertions;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared.DTOs;
using NUnit.Framework;
using System.Linq;

namespace Incepted.Domain.Deals.Tests.Unit;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
public class BaseUnitTest
{
    internal void CompareDealEntityAndDTOBasics(DealSubmission SUT, DealSubmissionDTO result)
    {
        result.Id.Should().Be(SUT.Id);
        result.Name.Should().Be(SUT.Name);
        for (int i = 0; i < SUT.Assignees.Count(); i++)
        {
            var expected = SUT.Assignees.ToList()[i];
            var actual = result.Assignees.ToList()[i];
            expected.Name.First.Should().Be(actual.FirstName);
            expected.Name.Last.Should().Be(actual.LastName);
        }

        if (result is DealSubmissionDTO)
        {
            for (int i = 0; i < SUT.Files.Count(); i++)
            {
                var expected = SUT.Files.ToList()[i];
                var actual = ((DealSubmissionDTO)result).Files.ToList()[i];
                expected.Id.Should().Be(actual.Id);
                expected.FileName.Should().Be(actual.FileName);
                expected.LastModified.Should().Be(actual.LastModified);
            }
        }
    }
    internal void CompareDealEntityAndDTOBasics(DealSubmission SUT, DealListItemDTO result)
    {
        result.Id.Should().Be(SUT.Id);
        result.Name.Should().Be(SUT.Name);
        for (int i = 0; i < SUT.Assignees.Count(); i++)
        {
            var expected = SUT.Assignees.ToList()[i];
            var actual = result.Assignees.ToList()[i];
            expected.Name.First.Should().Be(actual.FirstName);
            expected.Name.Last.Should().Be(actual.LastName);
        }
    }
}
