using AutoFixture.Kernel;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using System;

namespace Incepted.Shared.Tests.Unit.DataSeeding;
public class MoneySpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(Money))
        {
            var random = new Random();
            return new Money(random.NextInt64(1, 999999999999), (Currency)random.Next(0, 2));
        }

        return new NoSpecimen();
    }
}