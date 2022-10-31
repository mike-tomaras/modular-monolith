using AutoFixture.Kernel;
using Incepted.Shared.ValueTypes;
using System;

namespace Incepted.Shared.Tests.Unit.DataSeeding;
public class UserIdSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(UserId))
        {
            return new UserId($"auth0|{Guid.NewGuid()}");
        }

        return new NoSpecimen();
    }
}