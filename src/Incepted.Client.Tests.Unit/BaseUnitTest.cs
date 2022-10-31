using AutoFixture;
using Incepted.Shared.Tests.Unit.DataSeeding;
using NUnit.Framework;

namespace Incepted.Client.Tests.Unit;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
public class BaseUnitTest
{
    public Fixture DataGenerator { get; set; } = FixtureFactory.Create();
}
