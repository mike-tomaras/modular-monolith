using AutoFixture.Kernel;
using Incepted.Domain.Companies.Entities;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared.Enums;
using System;

namespace Incepted.Shared.Tests.Unit.DataSeeding;

public class DealFileSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        var fileTypeInt = new Random().Next(1, Enum.GetNames(typeof(FileType)).Length);
        if (request is Type type && type == typeof(DealFile))
        {
            return new DealFile(Guid.NewGuid(), $"Name{Guid.NewGuid()}.pdf", $"StoredName{Guid.NewGuid()}.xyz", (FileType)fileTypeInt, DateTimeOffset.Now.AddHours(-1));
        }

        return new NoSpecimen();
    }
}

public class CompanyFileSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(CompanyFile))
        {
            return new CompanyFile(Guid.NewGuid(), $"Name{Guid.NewGuid()}.pdf", $"StoredName{Guid.NewGuid()}.xyz", FileType.InsurerTCs, DateTimeOffset.Now.AddHours(-1));
        }

        return new NoSpecimen();
    }
}