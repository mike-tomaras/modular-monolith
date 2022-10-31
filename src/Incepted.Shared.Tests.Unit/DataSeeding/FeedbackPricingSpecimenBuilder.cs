using AutoFixture.Kernel;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using System;
using System.Collections.Generic;

namespace Incepted.Shared.Tests.Unit.DataSeeding;
public class FeedbackPricingSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(FeedbackPricing))
        {
            var random = new Random();
            var currency = (Currency)random.Next(0, 2);
            return new FeedbackPricing(
                new Money(random.NextInt64(10000000, 1000000000), currency), 
                new Money(random.NextInt64(1, 99999), currency), 
                new Money(random.NextInt64(1, 99999), currency),
                true,
                new Money(random.NextInt64(1, 99999), currency),
                true,
                new List<PricingOption>
                {
                    new PricingOption(new Limit(1, 0.15, true), new Retention(0.05, true), new Money(random.NextInt64(1, 99999), currency)),
                    new PricingOption(new Limit(1, 0.15, true), new Retention(0.035, true), new Money(random.NextInt64(1, 99999), currency)),
                    new PricingOption(new Limit(1, 0.15, true), new Retention(0.035, true), new Money(random.NextInt64(1, 99999), currency)),                    
                    new PricingOption(new Limit(2, 0.20, true), new Retention(0.05, true), new Money(random.NextInt64(1, 99999), currency)),
                    new PricingOption(new Limit(2, 0.20, true), new Retention(0.035, true), new Money(random.NextInt64(1, 99999), currency)),
                    new PricingOption(new Limit(2, 0.20, true), new Retention(0.035, true), new Money(random.NextInt64(1, 99999), currency)),
                    new PricingOption(new Limit(3, 0.25, true), new Retention(0.05, true), new Money(random.NextInt64(1, 99999), currency)),
                    new PricingOption(new Limit(3, 0.25, true), new Retention(0.035, true), new Money(random.NextInt64(1, 99999), currency)),
                    new PricingOption(new Limit(3, 0.25, true), new Retention(0.035, true), new Money(random.NextInt64(1, 99999), currency)),
                });
        }

        return new NoSpecimen();
    }
}