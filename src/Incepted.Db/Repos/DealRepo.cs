using Incepted.Db.DataModels.CompanyDMs;
using Incepted.Db.DataModels.DealDMs;
using Incepted.Domain.Deals.Application;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Optional;
using System.Collections.Immutable;
using System.Net;

namespace Incepted.Db.Repos;

internal class DealRepo : IDealRepo
{
    private readonly ILogger<DealRepo> _logger;
    private readonly Container _dealContainer;
    private readonly Container _dealListContainer;

    public DealRepo(string connString, ILogger<DealRepo> logger)
    {
        var client = new CosmosClient(connString);
        Database database = client.GetDatabase(id: "portaldb");
        _dealContainer = database.GetContainer(id: "deals");
        _dealListContainer = database.GetContainer(id: "deals-list");
        _logger = logger;
    }

    public async Task<Option<Unit, ErrorCode>> Create(DealSubmission newSubmission)
    {
        try
        {
            double ruTotal = 0;

            var item = DealSubmissionDM.Factory.ToDataModel(newSubmission);
            ItemResponse<DealSubmissionDM> itemResponse = 
                await _dealContainer.CreateItemAsync(
                    item: item,
                    partitionKey: new PartitionKey(item.PartitionKey_DealId.ToString())
                );
            ruTotal += itemResponse.RequestCharge;

            var listlItem = SubmissionListItemDM.Factory.ToDataModel(newSubmission);
            ItemResponse<SubmissionListItemDM> listItemResponse = 
                await _dealListContainer.CreateItemAsync(
                    item: listlItem,
                    partitionKey: new PartitionKey(listlItem.PartitionKey_CompanyId.ToString())
                );
            ruTotal += listItemResponse.RequestCharge;

            _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "create submission", ruTotal);

            return Option.Some<Unit, ErrorCode>(new Unit());
        }
        catch (Exception e)
        {
            return Option.None<Unit, ErrorCode>(DealErrorCodes.FailedToCreateSubmission_DbError(e));
        }
    }

    public async Task<Option<Unit, ErrorCode>> Create(SubmissionFeedback newFeedback, DealSubmission submission)
    {
        try
        {
            double ruTotal = 0;

            var item = SubmissionFeedbackDM.Factory.ToDataModel(newFeedback);
            ItemResponse<SubmissionFeedbackDM> itemResponse = 
                await _dealContainer.CreateItemAsync(
                    item: item,
                    partitionKey: new PartitionKey(item.PartitionKey_DealId.ToString())
                );
            ruTotal += itemResponse.RequestCharge;

            var listItem = SubmissionListItemDM.Factory.ToDataModel(newFeedback, submission);
            ItemResponse<SubmissionListItemDM> listItemResponse = 
                await _dealListContainer.CreateItemAsync(
                    item: listItem,
                    partitionKey: new PartitionKey(listItem.PartitionKey_CompanyId.ToString())
                );
            ruTotal += listItemResponse.RequestCharge;

            _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "create feedback", ruTotal);

            return Option.Some<Unit, ErrorCode>(new Unit());
        }
        catch (Exception e)
        {
            return Option.None<Unit, ErrorCode>(DealErrorCodes.FailedToCreateFeedback_DbError(e));
        }
    }

    public async Task<Option<Unit, ErrorCode>> Create(LiveDeal newLiveDeal)
    {
        try
        {
            double ruTotal = 0;

            var item = LiveDealDM.Factory.ToDataModel(newLiveDeal);
            ItemResponse<LiveDealDM> itemResponse = 
                await _dealContainer.CreateItemAsync(
                    item: item,
                    partitionKey: new PartitionKey(item.PartitionKey_DealId.ToString())
                );
            ruTotal += itemResponse.RequestCharge;

            var brokerListItem = LiveListItemDM.Factory.ToBrokerDataModel(newLiveDeal);
            ItemResponse<LiveListItemDM> brokerListItemResponse = 
                await _dealListContainer.CreateItemAsync(
                    item: brokerListItem,
                    partitionKey: new PartitionKey(brokerListItem.PartitionKey_CompanyId.ToString())
                );
            ruTotal += brokerListItemResponse.RequestCharge;

            var insurerListItem = LiveListItemDM.Factory.ToInsurerDataModel(newLiveDeal);
            ItemResponse<LiveListItemDM> insurerListItemResponse = 
                await _dealListContainer.CreateItemAsync(
                    item: insurerListItem,
                    partitionKey: new PartitionKey(insurerListItem.PartitionKey_CompanyId.ToString())
                );
            ruTotal += insurerListItemResponse.RequestCharge;

            _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "create live deal", ruTotal);

            return Option.Some<Unit, ErrorCode>(new Unit());
        }
        catch (Exception e)
        {
            return Option.None<Unit, ErrorCode>(DealErrorCodes.FailedToCreateLiveDeal_DbError(e));
        }
    }

    public async Task<Option<IImmutableList<SubmissionFeedback>, ErrorCode>> GetAllFeedbackDetails(Guid submissionId)
    {
        try
        {
            var query = $"SELECT * FROM d WHERE d.dealId = '{submissionId}' AND d.Type = 'feedback'";
            var allFeedback = await _dealContainer.GetResultsAsync<SubmissionFeedbackDM>(query);

            _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "get all submission feedbacks", allFeedback.ruTotal);

            return allFeedback.values
                .Select(SubmissionFeedbackDM.Factory.ToEntity)
                .ToImmutable()
                .Some<IImmutableList<SubmissionFeedback>, ErrorCode>();
        }
        catch (Exception e)
        {
            return Option.None<IImmutableList<SubmissionFeedback>, ErrorCode>(DealErrorCodes.FailedToRetrieveAllSubmissionFeedbacks_DbError(e));
        }
    }

    public async Task<Option<SubmissionFeedback, ErrorCode>> GetFeedbackDetails(Guid insurerCompanyId, Guid submissionId)
    {
        try
        {
            var query = $"SELECT * FROM d WHERE d.dealId = '{submissionId}' AND d.InsuranceCompanyId = '{insurerCompanyId}' AND d.Type = 'feedback'";
            var result = await _dealContainer.GetResultsAsync<SubmissionFeedbackDM>(query);
            var feedback = result.values.FirstOrDefault();

            _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "get single feedback", result.ruTotal);

            if (feedback == null) return Option.None<SubmissionFeedback, ErrorCode>(DealErrorCodes.FeedbackNotFound);

            return SubmissionFeedbackDM.Factory.ToEntity(feedback).Some<SubmissionFeedback, ErrorCode>();
        }
        catch (Exception e)
        {
            return Option.None<SubmissionFeedback, ErrorCode>(DealErrorCodes.FailedToRetrieveSingleSubmissionFeedback_DbError(e));
        }
    }

    public async Task<Option<DealSubmission, ErrorCode>> GetSubmissionDetails(Guid submissionId)
    {
        try
        {
            ItemResponse<DealSubmissionDM> result = 
                await _dealContainer.ReadItemAsync<DealSubmissionDM>(
                   id: submissionId.ToString(),
                   partitionKey: new PartitionKey(submissionId.ToString())
                );

            _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "get single submission", result.RequestCharge);

            return DealSubmissionDM.Factory.ToEntity(result).Some<DealSubmission, ErrorCode>();
        }
        catch (Exception e)
        {
            return Option.None<DealSubmission, ErrorCode>(DealErrorCodes.FailedToRetrieveSingleSubmission_DbError(e));
        }
        
    }

    public async Task<Option<IImmutableList<DealListItemDTO>, ErrorCode>> GetSubmissions(Guid companyId, CompanyType type)
    {
        try
        {
            var result = await _dealListContainer.GetResultsAsync<SubmissionListItemDM>(
            query: $"SELECT * FROM d WHERE d.companyId = '{companyId}' AND d.Type = 'submission'"
            );

            _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "get all submissions", result.ruTotal);

            return result.values
                .Select(SubmissionListItemDM.Factory.ToDTO)
                .ToImmutable()
                .Some<IImmutableList<DealListItemDTO>, ErrorCode>();
        }
        catch (Exception e)
        {
            return Option.None<IImmutableList<DealListItemDTO>, ErrorCode>(DealErrorCodes.FailedToRetrieveAllSubmissions_DbError(e));
        }
    }

    public async Task<Option<Unit, ErrorCode>> Update(DealSubmission submission)
    {
        try
        {
            double ruTotal = 0;

            var item = DealSubmissionDM.Factory.ToDataModel(submission);
            ItemResponse<DealSubmissionDM> itemResponse =
                await _dealContainer.ReplaceItemAsync(
                    id: submission.Id.ToString(),
                    item: item,
                    partitionKey: new PartitionKey(item.PartitionKey_DealId.ToString()),
                    requestOptions: new ItemRequestOptions { IfMatchEtag = submission.ETag }
                );
            ruTotal += itemResponse.RequestCharge;

            var feedbacks = await _dealContainer.GetResultsAsync<SubmissionFeedbackDM>(
                query: $"SELECT * FROM d WHERE d.dealId = '{submission.Id}' AND d.Type = 'feedback'"
                );
            ruTotal += feedbacks.ruTotal;

            foreach (var feedback in feedbacks.values)
            {
                var newFeedback = feedback.Update(submission);
                ItemResponse<SubmissionFeedbackDM> newFeedbackResponse = 
                    await _dealContainer.ReplaceItemAsync(
                        id: newFeedback.Id.ToString(),
                        item: newFeedback,
                        partitionKey: new PartitionKey(newFeedback.PartitionKey_DealId.ToString())
                    );
                ruTotal += newFeedbackResponse.RequestCharge;
            }

            var listItemCompanyIds = submission.Feedbacks
                .Aggregate(
                    seed: $"'{submission.BrokerCompanyId}'",
                    func: (current, next) => $"{current}, '{next.InsuranceCompanyId}'"
                    );

            var listItems = await _dealListContainer.GetResultsAsync<SubmissionListItemDM>(
                query: $"SELECT * FROM d WHERE d.companyId IN ({listItemCompanyIds}) AND d.SubmissionId = '{submission.Id}' AND d.Type = 'submission'"
                );
            ruTotal += listItems.ruTotal;

            foreach (var listItem in listItems.values)
            {
                var newListItem = listItem.Update(submission);
                ItemResponse<SubmissionListItemDM> newListItemResponse = 
                    await _dealListContainer.ReplaceItemAsync(
                        id: newListItem.Id.ToString(),
                        item: newListItem,
                        partitionKey: new PartitionKey(newListItem.PartitionKey_CompanyId.ToString())
                    );
                ruTotal += newListItemResponse.RequestCharge;
            }

            _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "update submission", ruTotal);

            return Option.Some<Unit, ErrorCode>(new Unit());
        }
        catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            return Option.None<Unit, ErrorCode>(DealErrorCodes.DataHasChanged);
        }
        catch (Exception e)
        {
            return Option.None<Unit, ErrorCode>(DealErrorCodes.FailedToUpdateSubmission_DbError(e));
        }
    }

    public async Task<Option<Unit, ErrorCode>> Update(SubmissionFeedback feedback)
    {
        try
        {   
            var item = SubmissionFeedbackDM.Factory.ToDataModel(feedback);
            ItemResponse<SubmissionFeedbackDM> itemResponse =
                await _dealContainer.ReplaceItemAsync(
                    id: item.Id.ToString(),
                    item: item,
                    partitionKey: new PartitionKey(item.PartitionKey_DealId.ToString()),
                    requestOptions: new ItemRequestOptions { IfMatchEtag = feedback.ETag }
                );

            _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "update feedback", itemResponse.RequestCharge);
         
            return Option.Some<Unit, ErrorCode>(new Unit());
        }
        catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            return Option.None<Unit, ErrorCode>(DealErrorCodes.DataHasChanged);
        }
        catch (Exception e)
        {
            return Option.None<Unit, ErrorCode>(DealErrorCodes.FailedToUpdateFeedback_DbError(e));
        }
    }
}
