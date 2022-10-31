using Incepted.Shared.Enums;
using Optional;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Incepted.Shared.ValueTypes;

public class Enhancement : IEquatable<Enhancement?>
{
    [JsonPropertyName("title")] public string Title { get; private init; }
    [JsonPropertyName("description")] public string Description { get; private init; }
    [JsonPropertyName("brokerRequestsIt")] public bool BrokerRequestsIt { get; private init; }
    [JsonPropertyName("insurerOffersIt")] public bool InsurerOffersIt { get; private init; }
    [JsonPropertyName("comment")] public string Comment { get; private init; } = string.Empty;
    [JsonPropertyName("ap")] public double AP { get; private init; }
    [JsonPropertyName("type")] public EnhancementType Type { get; private init; }

    [Newtonsoft.Json.JsonIgnore][JsonIgnore] public bool HasComment => !string.IsNullOrEmpty(Comment);
    [Newtonsoft.Json.JsonIgnore][JsonIgnore] public bool HasAP => AP > 0;

    public Enhancement(EnhancementType type, string title, string desc)
    {
        if (string.IsNullOrEmpty(title)) throw new ArgumentException("Enhancement title can't be empty", $"{nameof(Enhancement)} {nameof(title)}");
        if (string.IsNullOrEmpty(desc)) throw new ArgumentException("Enhancement description can't be empty", $"{nameof(Enhancement)} {nameof(desc)}");

        Type = type;
        Title = title;
        Description = desc;
        AP = 0;
        BrokerRequestsIt = false;
    }

    [JsonConstructor]
    [Newtonsoft.Json.JsonConstructor]//CosmosDB SDK uses Newtonsoft for serializing
    public Enhancement(EnhancementType type, string title, string description, string comment, double ap, bool brokerRequestsIt, bool insurerOffersIt)
    {
        if (string.IsNullOrEmpty(title)) throw new ArgumentException("Enhancement title can't be empty", $"{nameof(Enhancement)} {nameof(title)}");
        if (string.IsNullOrEmpty(description)) throw new ArgumentException("Enhancement description can't be empty", $"{nameof(Enhancement)} {nameof(description)}");
        if (ap < 0 || ap > 1) throw new ArgumentException("Enhancement AP must be between 0 and 1 inclusive", $"{nameof(Enhancement)} {nameof(ap)}");

        Type = type;
        Title = title;
        Description = description;
        Comment = comment;
        AP = ap;
        BrokerRequestsIt = brokerRequestsIt;
        InsurerOffersIt = insurerOffersIt;
    }

    public Enhancement SetBrokerSelected(bool value) => new Enhancement(Type, Title, Description, Comment, AP, value, InsurerOffersIt);
    public Enhancement SetInsurerSelected(bool value) => new Enhancement(Type, Title, Description, Comment, AP, BrokerRequestsIt, value);
    public Option<Enhancement, ErrorCode> SetAP(double ap) 
    {
        if (ap < 0 || ap > 100) 
            return Option.None<Enhancement, ErrorCode>(DealErrorCodes.EnhancementAPNotValid);

        var insurerSelected = ap > 0 ? true : InsurerOffersIt;

        return new Enhancement(Type, Title, Description, Comment, ap/100, BrokerRequestsIt, insurerSelected)
            .Some<Enhancement, ErrorCode>();
    }

    public Enhancement SetComment(string comment)
    {
        var insurerSelected = !string.IsNullOrEmpty(comment) ? true : InsurerOffersIt;

        return new Enhancement(Type, Title, Description, comment, AP, BrokerRequestsIt, insurerSelected);
    }

    public override bool Equals(object? obj) => Equals(obj as Enhancement);
    public bool Equals(Enhancement? other) => 
        other is not null &&
               Title == other.Title &&
               Description == other.Description &&
               BrokerRequestsIt == other.BrokerRequestsIt &&
               InsurerOffersIt == other.InsurerOffersIt &&
               Comment == other.Comment &&
               AP == other.AP &&
               Type == other.Type;
    public override int GetHashCode() => HashCode.Combine(Title, Description, BrokerRequestsIt, InsurerOffersIt, Comment, AP, Type);
    public static bool operator ==(Enhancement? left, Enhancement? right) => EqualityComparer<Enhancement>.Default.Equals(left, right);
    public static bool operator !=(Enhancement? left, Enhancement? right) => !(left == right);

    public static class Factory
    {
        public static IImmutableList<Enhancement> Default => ImmutableList.CreateRange(new List<Enhancement>
        {
            new Enhancement(EnhancementType.Request, "Geographical exposure", "Please let us know if there any jurisidctions you cannot cover? Please indicate any potential limitations with respect to foreign operations.", "comment!!!", 0.05, false, false),
            new Enhancement(EnhancementType.Request, "Full title top-up cover", "With respect to the fundamental warranties, please advise if additional coverage for loss up to the full equity value is available. Please indicate AP if any.", "comment!!!", 0.05, false, false),
            new Enhancement(EnhancementType.Request, "Synthetic Closing Cover", "Please advise if synthetic coverage for warranties as of closing is available. Please indicate AP, if any.", "comment!!!", 0.05, false, false),
            new Enhancement(EnhancementType.Request, "New Breach Cover", "Please indicate if you offer new breach cover. Please indicate AP, if any."),
            new Enhancement(EnhancementType.Request, "Loss Occurence", "Regardless of the SPA, could the policy state that loss is calculated either at the level of the Insured or at the level of the Target Group?"),
            new Enhancement(EnhancementType.Request, "Seller's Knowledge", "Could the policy follow the definition of \"Sellers' Knowledge\" in the SPA?"),
            new Enhancement(EnhancementType.Request, "Seller's Knowledge - Gross negligence I", "Could the policy follow a definition of Seller's Knowledge that contains \"gross negligence\"?"),
            new Enhancement(EnhancementType.Request, "Seller's Knowledge - Gross negligence II", "Could you offer a definition of Seller's Knowledge in the policy including \"gross negligence\" synthetically?"),
            new Enhancement(EnhancementType.Request, "Materiality Scrape", "Could you offer a materiality scrape in the policy (synthetically)? If not for all warranties, please indicate which warranties would remain materiality qualified. Please indicae AP, if any."),
            new Enhancement(EnhancementType.Request, "Fairly Disclosed I", "Please confirm that you can mirror the \"(Fairly) Disclosed\" definition in the SPA in the policy?"),
            new Enhancement(EnhancementType.Request, "Fairly Disclosed II", "Are you prepared to disregard the definition of disclosed in the SPA and apply the following definition: (TO BE INSERTED BY BROKER). Please amend as necessary."),
            new Enhancement(EnhancementType.Request, "COVID-19", "If you require a specific COVID-19 exclusion for this transaction, please provide the wording."),
            new Enhancement(EnhancementType.Request, "Fundamental Warranties", "Can you offer (i) nil De Minimis and (ii) nil Retention to the Fundamental Warranties?"),
            new Enhancement(EnhancementType.Request, "Knowledge Scrape", "Do you offer knowledge scraping? Please indicated in the warranty spreadsheet for which warraties. Any AP?"),
            new Enhancement(EnhancementType.Request, "Policy Period Extension", "Please indicate whether you will charge AP for a limitation period of 3 years after the Closing Date for the Business Warranties."),
            new Enhancement(EnhancementType.Request, "Limitation period for Tax Idemnity", "Please indicate whether you will charge AP for a limitation period of 10 years after the Closing Date for the Tax Indemnity."),
            new Enhancement(EnhancementType.Request, "Limitation period for Fundamental Warranties", "Please indicate whether you will charge AP for a limitation period of 10 years after the Closing Date for the Fundamental Warranties."),
            new Enhancement(EnhancementType.Request, "Pension Underfunding", "Please confirm that you can dis-apply the pension underfunding exclusion (if applicable) in case a warranty breach is the result of the non-disclosure of the relevant pension obligation and further provided that such obligation is not accounted for at all in the accounts."),
            new Enhancement(EnhancementType.Request, "Forward looking statements", "Would you be able to delete a general exclusion for forward looking statements?"),
            new Enhancement(EnhancementType.Request, "Non-Disclosure of Data Room", "Please indicate whether you can offer the data room not be considered disclosed for the purpose of the policy (i) in line with the SPA and (ii) synthetically. Please indicate the AP. Please note in this case the NCD would not contain a reference to the VDR."),
            new Enhancement(EnhancementType.Request, "Disclosed Items", "Can you remove DD Reports from disclosed items? Please indicate any AP. Please note in this case the NCD would not contain a reference to any DD reports."),
            new Enhancement(EnhancementType.Request, "Identified Risks", "Is cover available for identified risks relating to findings of past tax audits (Betriebsprüfungen) that have been resolved but may impact later tax periods?"),
            new Enhancement(EnhancementType.Request, "Ongoing audits", "Is cover available for unknown risks that may be identified in a threatened or ongoing tax audit (Betriebsprüfung)?"),
            new Enhancement(EnhancementType.Request, "Synthetic Tax Cover", "If you are you able to offer synthetic tax cover, please provide AP."),
            new Enhancement(EnhancementType.Request, "Low Risk Items", "Could “low risk” tax items detailed in the tax DD Report be considered non-disclosed?"),
            new Enhancement(EnhancementType.Request, "Blind Spot Cover", "Is cover available for non-material jurisdictions and subsidiaries without due diligence?"),
            new Enhancement(EnhancementType.Request, "No obligations to keep target insured", "-"),
            new Enhancement(EnhancementType.Request, "Formalities", "Clarification in the policy that reporting claims is not subject to formalities which are more burdensome than applicable law (it is understood that the claims made/notified principle will still apply)."),
            new Enhancement(EnhancementType.Request, "Follow Loss definition in SPA", "Can you follow the definition of \"Loss\" in the SPA?"),
            new Enhancement(EnhancementType.Request, "Consequential, loss of profit and indirect losses", "Would you be able to cover consequential, loss of profit and indirect loss to the extent reasonably forseeable?"),
            new Enhancement(EnhancementType.Request, "Consequential, loss of profit and indirect losses - synthetically", "Would you be able to cover consequential, loss of profit and indirect loss to the extent reasonably forseeable synthetically?"),
            new Enhancement(EnhancementType.Request, "Extended Loss Cover", "Would you be able to cover loss of opportunities, loss of goodwill or reputation, frustrated expenses, internal administrative and overhead costs?"),
            new Enhancement(EnhancementType.Request, "Cover for Fines", "Would you cover civil fines and penalties and criminal fines and penalties (in each case to the extent insurable under law)?"),
            new Enhancement(EnhancementType.Request, "Punitive or exemplary damages", "Would you cover punitive or exemplary damages?"),
            new Enhancement(EnhancementType.Request, "Loss based on a valuation of shares or multiples", "Would you cover Loss based on a valuation of the shares or based on multiples (such as EBIT, EBITDA or similar)?"),
            new Enhancement(EnhancementType.Request, "Multiples", "Assuming an exclusion for loss based on multipliers or other valuation methodologies is required, could breaches of fundamental warranties and the financial statements warranty be excempted from that exclusion?"),
            new Enhancement(EnhancementType.Assumption, "Nil Recourse", "A NIL recourse concept of the buyer against the seller for all insured warranties/indemnities under the SPA is accepted and will not attract any AP."),
            new Enhancement(EnhancementType.Assumption, "No bring-down for fundamentals", "No bring-down disclosure mechanism is required if only Fundamental Warranties (title and capacity) are given at closing (provided that this does not include insolvency warranties and that the scope is mirrored by attributable register knowledge)."),
            new Enhancement(EnhancementType.Assumption, "Serial Claims", "The de minimis for purpose of the policy will include serial demages (Serienschäden) qualifying all claims arising out of substantially similar sets of circumstances as one claim (i) in line with the SPA and (ii) not in line (synthetically) with the SPA."),
            new Enhancement(EnhancementType.Assumption, "Policy De Minimis", "The policy de minimis can be lower than the respective amount agreed in the SPA (provided that the amount will not be lower than relevant DD-thresholds."),
            new Enhancement(EnhancementType.Assumption, "Subrogation", "The insured’s external advisors are besides the Seller(s) also inlcuded in the subrogation clause in the policy; i. e. no subrogation against the insured’s external advisors save for fraud (Arglist)or willful misconduct (Vorsatz)"),
            new Enhancement(EnhancementType.Assumption, "Joint Liability", "All Clauses of the SPA relating to a several liability concept can be disregarded for policy purpose (i.e. could the policy follow a joint liability concept (gesamschuldnerische Haftung)"),
            new Enhancement(EnhancementType.Assumption, "Actual Knowledge Exclusion", "The actual knowledge exclusion with respect to the SPA, the Data Room and DD can be restricted - so that references to “facts, matters or circumstances that could reasonably be expected to give rise to a breach” were deleted i.e. only knowledge of a breach itself would be excluded."),
            new Enhancement(EnhancementType.Assumption, "True Warranty Spreadsheet", "A warranty being marked as “excluded” or “partially covered” in the warranty spreadsheet would not prevent coverage of other warranties not marked as “excluded” or “partial cover”."),
            new Enhancement(EnhancementType.Assumption, "Boxing", "The policy will not include a mechanism such as boxing of warranties (i.e. warranties can only be brought under the most specific relevant warranty) or lex specialis provisions."),
            new Enhancement(EnhancementType.Assumption, "Tax Gross Up", "Tax gross up is included for no AP."),
        });
    }    
}
