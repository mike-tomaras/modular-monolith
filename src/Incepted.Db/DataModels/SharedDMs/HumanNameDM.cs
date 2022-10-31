using Incepted.Shared.ValueTypes;

namespace Incepted.Db.DataModels.SharedDMs;

internal class HumanNameDM
{
    public string First { get; set; }
    public string Last { get; set; }

    public static class Factory
    {
        public static HumanNameDM ToDataModel(HumanName name) =>
            new HumanNameDM
            {
                First = name.First,
                Last = name.Last
            };

        public static HumanName ToEntity(HumanNameDM name) =>
            new HumanName(name.First, name.Last);
    }
}
