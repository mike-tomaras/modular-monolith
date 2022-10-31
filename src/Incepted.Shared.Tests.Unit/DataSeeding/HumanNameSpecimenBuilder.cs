using AutoFixture.Kernel;
using Incepted.Shared.ValueTypes;
using System;

namespace Incepted.Shared.Tests.Unit.DataSeeding;
public class HumanNameSpecimenBuilder : ISpecimenBuilder
{
    private string[] letters = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(HumanName))
        {
            return new HumanName($"{RandomInitial()}{Guid.NewGuid()}", $"{RandomInitial()}{Guid.NewGuid()}");
        }

        return new NoSpecimen();
    }

    private string RandomInitial()
    {
        return letters[new Random().Next(0, letters.Length)]; 
    }
}