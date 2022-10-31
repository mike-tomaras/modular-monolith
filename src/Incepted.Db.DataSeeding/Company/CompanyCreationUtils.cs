using Incepted.Db.DataModels.CompanyDMs;
using Incepted.Db.DataModels.SharedDMs;
using Incepted.Shared.Enums;
using RandomNameGeneratorLibrary;

namespace Incepted.Db.DataSeeding.Company;

internal static class CompanyCreationUtils
{
    /// <summary>
    /// Creates a company with no employees
    /// </summary>
    /// <param name="type">The type of the company</param>
    /// <returns>A populated company</returns>
    public static CompanyDM CreateCompany(CompanyType type)
    {
        Console.Write("Creating company...");

        var companyId = Guid.NewGuid();
        CompanyDM company = new CompanyDM
        {
            Version = 1,
            Id = companyId,
            PartitionKey_CompanyId = companyId,
            Name = $"{type} Company {companyId.ToString().Substring(0, 6)}",
            CompanyType = type,
            Employees = new List<EmployeeDM>()
        };

        if (type == CompanyType.Insurer)
            company.TsAndCs = new FileDM
            {
                Version = 1,
                Id = Guid.NewGuid(),
                FileName = "ts and cs.pdf",
                StoredFileName = Path.GetRandomFileName(),
                FileType = FileType.InsurerTCs,
                LastModified = DateTimeOffset.Now.AddDays(-3)
            };

        Console.WriteLine("DONE");

        return company;
    }

    /// <summary>
    /// Sets company id
    /// </summary>
    /// <param name="company">The company</param>
    /// <param name="id">The id to set</param>
    /// <returns>The company with the id specified</returns>
    public static CompanyDM WithId(this CompanyDM company, Guid id)
    {
        Console.Write("Setting company id...");

        company.Id = id;

        Console.WriteLine("DONE");

        return company;
    }

    /// <summary>
    /// Sets company name
    /// </summary>
    /// <param name="company">The company</param>
    /// <param name="name">The name to set</param>
    /// <returns>The company with the name specified</returns>
    public static CompanyDM WithName(this CompanyDM company, string name)
    {
        Console.Write("Setting company name...");

        company.Name = name;

        Console.WriteLine("DONE");

        return company;
    }

    /// <summary>
    /// Adds a single employee
    /// </summary>
    /// <param name="company">The company to add the employee to</param>
    /// <param name="employee">The employees to add</param>
    /// <returns>The company with the added employees</returns>
    public static CompanyDM WithEmployee(this CompanyDM company, EmployeeDM employee)
    {
        Console.Write("Adding a company employee...");

        company.Employees = company.Employees.ToList().Concat(new List<EmployeeDM> { employee });

        Console.WriteLine("DONE");

        return company;
    }

    /// <summary>
    /// Adds employees with auth0 ids from the dev auth0 tenant
    /// </summary>
    /// <param name="company">The company to add employees to</param>
    /// <returns>The company with the added employees</returns>
    /// <exception cref="NotImplementedException">Throws if the company type is not implemented</exception>
    public static CompanyDM WithDevEmployees(this CompanyDM company)
    {
        Console.Write("Setting company employees from auth0 dev tenant...");

        List<EmployeeDM> employees;
        if (company.CompanyType == CompanyType.Insurer)
        {
            employees = new List<EmployeeDM>
            {
                new EmployeeDM
                {
                    Version = 1,
                    Id = Guid.NewGuid(),
                    UserId = "auth0|62818ca9b0997000699020da",
                    Name = new HumanNameDM{ First = "MikeIns", Last = "Tomaras" },
                    Email = "miket969+insurer@gmail.com"
                }
            };
        }
        else if (company.CompanyType == CompanyType.Broker)
        {
            employees = new List<EmployeeDM>
            {
                new EmployeeDM
                {
                    Version = 1,
                    Id = Guid.NewGuid(),
                    UserId = "auth0|62492cd27810d5006999aa22",
                    Name = new HumanNameDM{ First = "MikeBrok", Last = "Tomaras" },
                    Email = "miket969+broker@gmail.com"
                }
            };
        }
        else
        {
            throw new NotImplementedException($"There is no implementation for seeding employees for a company of type {company.CompanyType}");
        }

        company.Employees = company.Employees.ToList().Concat(employees);

        Console.WriteLine("DONE");

        return company;
    }

    /// <summary>
    /// Adds employees with auth0 ids from the prod auth0 tenant
    /// </summary>
    /// <param name="company">The company to add employees to</param>
    /// <returns>The company with added employees</returns>
    /// <exception cref="NotImplementedException">Throws if the company type is not implemented</exception>
    public static CompanyDM WithProdEmployees(this CompanyDM company)
    {
        Console.Write("Setting company employees from auth0 prod tenant...");

        List<EmployeeDM> employees;
        if (company.CompanyType == CompanyType.Insurer)
        {
            employees = new List<EmployeeDM>
            {
                new EmployeeDM
                {
                    Version = 1,
                    Id = Guid.NewGuid(),
                    UserId = "auth0|62c95739d3dc4f88c18c0b18",
                    Name = new HumanNameDM{ First = "KonradIns", Last = "Rotthege" },
                    Email = "konrad+insurer@incepted.io"
                },
                new EmployeeDM
                {
                    Version = 1,
                    Id = Guid.NewGuid(),
                    UserId = "auth0|62c956f07b12303583eef4ab",
                    Name = new HumanNameDM{ First = "JamieIns", Last = "Brown" },
                    Email = "jamie+insurer@incepted.io"
                },
                new EmployeeDM
                {
                    Version = 1,
                    Id = Guid.NewGuid(),
                    UserId = "auth0|62c9565049c4f87c02fce444",
                    Name = new HumanNameDM{ First = "MikeIns", Last = "Tomaras" },
                    Email = "miket969+insurer@gmail.com"
                }
            };
        }
        else if (company.CompanyType == CompanyType.Broker)
        {
            employees = new List<EmployeeDM>
            {
                new EmployeeDM
                {
                    Version = 1,
                    Id = Guid.NewGuid(),
                    UserId = "auth0|626e80056c48dc006a2de396",
                    Name = new HumanNameDM{ First = "KonradBrok", Last = "Rotthege" },
                    Email = "konrad+broker@incepted.io"
                },
                new EmployeeDM
                {
                    Version = 1,
                    Id = Guid.NewGuid(),
                    UserId = "auth0|626e80401d742f006f2ab6a6",
                    Name = new HumanNameDM{ First = "JamieBrok", Last = "Brown" },
                    Email = "jamie+broker@incepted.io"
                },
                new EmployeeDM
                {
                    Version = 1,
                    Id = Guid.NewGuid(),
                    UserId = "auth0|6255a0d5c0f77100691fba5c",
                    Name = new HumanNameDM{ First = "MikeBrok", Last = "Tomaras" },
                    Email = "miket969+broker@gmail.com"
                }
            };
        }
        else
        {
            throw new NotImplementedException($"There is no implementation for seeding employees for a company of type {company.CompanyType}");
        }

        company.Employees = company.Employees.ToList().Concat(employees);

        Console.WriteLine("DONE");

        return company;
    }

    /// <summary>
    /// Adds employees with random auth0 ids
    /// </summary>
    /// <param name="company">The company to add employees to</param>
    /// <param name="number">The number of employees to add, defaults to 3</param>
    /// <returns>The company with added employees</returns>
    public static CompanyDM WithRandomEmployees(this CompanyDM company, int number = 3)
    {
        Console.Write("Setting random company employees...");

        var personGenerator = new PersonNameGenerator();
        var employees = new List<EmployeeDM>();

        for (int i = 0; i < number; i++)
        {
            var id = Guid.NewGuid();
            var firstName = personGenerator.GenerateRandomFirstName();
            var lastName = personGenerator.GenerateRandomLastName();

            employees.Add(
                new EmployeeDM
                {
                    Version = 1,
                    Id = id,
                    UserId = $"auth0|{id.ToString().Replace("-", string.Empty)}",
                    Name = new HumanNameDM { First = firstName, Last = lastName },
                    Email = $"{firstName}.{lastName}@{company.Name.Replace(" ", string.Empty)}.com".ToLower()
                });
        }

        company.Employees = company.Employees.ToList().Concat(employees);

        Console.WriteLine("DONE");

        return company;
    }
}
