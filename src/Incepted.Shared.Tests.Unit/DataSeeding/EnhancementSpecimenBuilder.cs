using AutoFixture.Kernel;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using System;

namespace Incepted.Shared.Tests.Unit.DataSeeding;
public class EnhancementSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(Enhancement))
        {
            var random = new Random();
            return new Enhancement(EnhancementType.Request, $"Title{Guid.NewGuid()}", $"Description{Guid.NewGuid()}", $"Comment{Guid.NewGuid()}", random.NextDouble(), false, false);
        }

        return new NoSpecimen();
    }
}