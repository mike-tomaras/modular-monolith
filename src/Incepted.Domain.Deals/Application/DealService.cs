using Incepted.Domain.Companies.Application;
using Incepted.Domain.Companies.Entities;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using Optional;
using Optional.Collections;
using Optional.Linq;
using System.Collections.Immutable;
using EmptyFile = Incepted.Domain.Deals.Domain.EmptyFile;

namespace Incepted.Domain.Deals.Application;

internal class DealService : IDealService
{
    private readonly IDealRepo _dealRepo;
    private readonly ICompanyService _companyService;
    private readonly IDealFileService _fileService;
    private readonly IDealNotificationService _notificationService;

    public DealService(IDealRepo dealRepo, ICompanyService companyService, IDealFileService fileService, IDealNotificationService notificationService)
    {
        _dealRepo = dealRepo;
        _companyService = companyService;
        _fileService = fileService;
        _notificationService = notificationService;
    }

    public Option<IImmutableList<DealListItemDTO>, ErrorCode> GetSubmissionsQuery(UserId userId)
    {
        return _companyService
            .GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .FlatMap(company => _dealRepo.GetSubmissions(company.Id, company.Type).Result);
    }

    public Option<DealSubmissionDTO, ErrorCode> GetSubmissionDetailsQuery(UserId userId, Guid submissionId)
    {
        return _companyService
            .GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .FlatMap(_ => _dealRepo.GetSubmissionDetails(submissionId).Result)
            .Map(DealSubmission.Factory.ToDTO);
    }

    public Option<SubmissionFeedbackDTO, ErrorCode> GetSubmissionFeedbackDetailsQuery(UserId userId, Guid submissionId)
    {
        return _companyService
            .GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .FlatMap(company => _dealRepo.GetFeedbackDetails(company.Id, submissionId).Result)
            .Map(SubmissionFeedback.Factory.ToDTO);
    }

    public Option<IImmutableList<SubmissionFeedbackDTO>, ErrorCode> GetAllFeedbackDetailsOfSubmissionQuery(UserId userId, Guid submissionId)
    {
        return _companyService
           .GetCompanyOfUserQuery(userId)//verify the user is part of a company
           .FlatMap(_ => _dealRepo.GetAllFeedbackDetails(submissionId).Result)
           .Map(feedbacks => feedbacks.Select(SubmissionFeedback.Factory.ToDTO).ToImmutable());
    }

    public Option<Guid, ErrorCode> CreateSubmissionCommand(UserId userId, string name)
    {
        var newId = Guid.NewGuid();

        return _companyService
            .GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .Map(company => new DealSubmission
                            (
                                newId,
                                name,
                                company.Name,
                                company.Id,
                                new BasicTerms(),
                                new SubmissionPricing(),
                                Enhancement.Factory.Default,
                                ImmutableList.Create<Warranty>(),
                                ImmutableList.Create<Assignee>(),
                                ImmutableList.Create<DealFile>(),
                                ImmutableList.Create<FeedbackDetails>(),
                                ImmutableList.Create<Modification>(),
                                null
                            )
            )
            .FlatMap(deal => _dealRepo.Create(deal).Result)
            .Map(_ => newId);
    }

    public Option<DealSubmissionDTO, ErrorCode> UpdateSubmissionDetailsCommand(UserId userId, DealSubmissionDTO submission)
    {
        DealSubmission result = new EmptySubmission();
        
        return
            _companyService.GetCompanyOfUserQuery(userId)//verify the user is part of a company        
            .FlatMap(_ =>_dealRepo.GetSubmissionDetails(submission.Id).Result)//verify the deal is the company's
            .FlatMap(dealDetails => dealDetails.Update(submission))
            .Map(updatedDeal => result = updatedDeal)
            .FlatMap(updatedDeal => _dealRepo.Update(updatedDeal).Result)
            .Map(_ => DealSubmission.Factory.ToDTO(result));
    }

    public Option<SubmissionFeedbackDTO, ErrorCode> UpdateSubmissionFeedbackCommand(UserId userId, SubmissionFeedbackDTO feedback)
    {
        SubmissionFeedback result = new EmptyFeedback();

        return
            _companyService.GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .FlatMap(_ => _dealRepo.GetFeedbackDetails(feedback.InsuranceCompanyId, feedback.SubmissionId).Result)//verify the feedback is the company's
            .FlatMap(feedbackDetails => feedbackDetails.Update(feedback))
            .Map(updatedFeedback => result = updatedFeedback)
            .FlatMap(updatedFeedback => _dealRepo.Update(updatedFeedback).Result)
            .Map(_ => SubmissionFeedback.Factory.ToDTO(result));
    }

    public Option<Unit, ErrorCode> UpdateDealAssigneesCommand(UserId userId, UpdateDealAssigneesDTO assigneesData)
    {
        var companyId = Guid.Empty;

        return
            _companyService.GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .Filter(company => company.AreAssigneesValidEmployees(assigneesData.AssigneeIds), DealErrorCodes.AssigneesAreNotValid)//verify the assignees belong to the company
            .Map(company => companyId = company.Id)
            .FlatMap(company => _dealRepo.GetSubmissionDetails(assigneesData.DealId).Result)//verify the deal is the company's
            .FlatMap(deal => deal.UpdateAssignees(assigneesData.Assignees.Select(Assignee.Factory.ToEntity).ToImmutable(), companyId))//immutable update of deal's assignees
            .FlatMap(deal => _dealRepo.Update(deal).Result);
    }

    public Option<Unit, ErrorCode> SubmitSumbissionCommand(UserId userId, SubmitDealDTO submitData)
    {
        Company brokerCompany = new EmptyCompany();
        DealSubmission deal = new EmptySubmission();
        var feedbacks = new List<SubmissionFeedback>();

        return
            _companyService.GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .Map(company => brokerCompany = company)
            .FlatMap(_ => _dealRepo.GetSubmissionDetails(submitData.DealId).Result)//verify the deal is the company's
            .Map(dbDeal => deal = dbDeal)
            .Map(deal => feedbacks = submitData //create feedback entities
                .InsurersToSubmitTo
                .Select(insurer => SubmissionFeedback.Factory.Create(insurer.Id, insurer.Name, deal))
                .ToList())
            .Map(_ => feedbacks //create feedback details for the submission
                .Select(f => FeedbackDetails.Factory.Create(f.Id, f.InsuranceCompanyId))
                .ToImmutable())
            .FlatMap(insurersSubmittedTo => deal.Submit(insurersSubmittedTo, submitData.SubmissionDeadline))
            .FlatMap(updatedDeal => _dealRepo.Update(updatedDeal).Result)
            .Map(_ => feedbacks
                .Select(feedback => _dealRepo.Create(feedback, deal).Result)
                .GetLastErrorOrLastValue())
            .Map(_ => submitData
                .InsurersToSubmitTo
                .Select(insurer => _companyService.GetEmployeesForCompanyQuery(insurer.Id))
                .Values()//ignoring errors, it's ok (for now) if some notifications do not get sent
                .SelectMany(e => e)
                .ToImmutable()
            )
            .FlatMap(insurers => _notificationService
                .SendNotification(
                    type: NotificationType.Insurer_NewSubmission,
                    receipients: insurers.Select(e => new RecipientDTO(e.FirstName, e.LastName, e.Email)).ToImmutable(),
                    data: new Dictionary<string, string> { 
                            { "broker_company", brokerCompany.Name }, 
                            { "deal_id", submitData.DealId.ToString() } 
                        }.ToImmutableDictionary()
                    )
            );
        //.FlatMap(insurers => _notificationService
        //                        .SendNotification(
        //                            type: NotificationType.Insurer_Invite,
        //                            receipients: submitData.Emails.Select(e => new RecipientDTO(new HumanName(string.Empty, string.Empty), e)).ToImmutable(),
        //                            data: new Dictionary<string, string> { { "broker_company", brokerCompany.Name } }.ToImmutableDictionary()
        //                            )
        //);
    }

    public Option<Unit, ErrorCode> ModifySubmissionCommand(UserId userId, ModifySubmittedDealDTO modifyData)
    {
        Company brokerCompany = new EmptyCompany();
        DealSubmission deal = new EmptySubmission();

        return
            _companyService.GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .Map(company => brokerCompany = company)
            .FlatMap(_ => _dealRepo.GetSubmissionDetails(modifyData.DealId).Result)//verify the deal is the company's
            .Map(dealDetails => deal = dealDetails)
            .FlatMap(dealDetails => dealDetails.ModifySubmission(modifyData.Notes))
            .FlatMap(updatedDeal => _dealRepo.Update(updatedDeal).Result)
            .FlatMap(_ => MarkExistingFeedbackToBeReviewed(deal.Feedbacks.Select(i => i.InsuranceCompanyId), deal.Id))
            .FlatMap(_ => NotifyInsurersOfSubmissionModification(deal.Feedbacks.Select(i => i.InsuranceCompanyId), deal.Id, deal.Name, brokerCompany.Name));            
    }
    private Option<Unit, ErrorCode> MarkExistingFeedbackToBeReviewed(IEnumerable<Guid> insurerCompanyIds, Guid dealId) => 
        insurerCompanyIds
            .Select(insurerId => _dealRepo.GetFeedbackDetails(insurerId, dealId).Result).Values()
            .Where(feedback => feedback.Submitted)
            .Select(feedback => feedback.SubmissionModified()).Values()
            .Select(feedback => _dealRepo.Update(feedback).Result)
            .GetLastErrorOrLastValue();
    private Option<Unit, ErrorCode> NotifyInsurersOfSubmissionModification(IEnumerable<Guid> insurerCompanyIds, Guid dealId, string dealName, string brokerName) => 
        insurerCompanyIds
            .Select(id => _companyService.GetEmployeesForCompanyQuery(id))
            .Values()//ignoring errors, it's ok (for now) if some notifications do not get sent
            .SelectMany(e => e).Some<IEnumerable<EmployeeDTO>, ErrorCode>()
            .FlatMap(insurers => _notificationService
                .SendNotification(
                    type: NotificationType.Insurer_SubmissionModified,
                    insurers.Select(e => new RecipientDTO(e.FirstName, e.LastName, e.Email)).ToImmutable(),
                    data: new Dictionary<string, string> {
                            { "project_name", dealName },
                            { "broker_company", brokerName },
                            { "deal_id", dealId.ToString() } }
                        .ToImmutableDictionary()
                    )
            );

    public Option<Unit, ErrorCode> GoLiveCommand(UserId userId, GoLiveDTO goliveData)
    {
        DealSubmission deal = new EmptySubmission();
        SubmissionFeedback feedback = new EmptyFeedback();

        return
            _companyService.GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .FlatMap(_ => _dealRepo.GetSubmissionDetails(goliveData.SubmissionId).Result)//verify the deal is the company's
            .Map(d => deal = d)
            .FlatMap(dealDetails => dealDetails.GoLive(goliveData.FeedbackId))
            .FlatMap(updatedDeal => _dealRepo.Update(updatedDeal).Result)
            .FlatMap(_ => _dealRepo.GetFeedbackDetails(goliveData.InsuranceCompanyId, goliveData.SubmissionId).Result)
            .Map(f => feedback = f)
            .FlatMap(feedbackDetails => feedbackDetails.GoLive())            
            .FlatMap(updatedFeedback => _dealRepo.Update(updatedFeedback).Result)
            .Map(_ => LiveDeal.Factory.Create(deal, feedback))
            .FlatMap(newLiveDeal => _dealRepo.Create(newLiveDeal).Result)
            .FlatMap(_ => NotifyInsurerOfSubmissionSuccess(goliveData.InsuranceCompanyId, deal.Id, deal.Name))
            .Map(_ => deal.Feedbacks.Where(i => !i.IsLive).Select(i => i.InsuranceCompanyId))
            .FlatMap(declinedInsurers => NotifyInsurersOfSubmissionDecline(declinedInsurers, deal.Id, deal.Name));
    }
    private Option<Unit, ErrorCode> NotifyInsurerOfSubmissionSuccess(Guid insuranceCompanyId, Guid dealId, string dealName) =>
        _companyService.GetEmployeesForCompanyQuery(insuranceCompanyId)
            .FlatMap(insurers => _notificationService
                .SendNotification(
                    type: NotificationType.Insurer_SubmissionFeedbackAccepted,
                    receipients: insurers.Select(e => new RecipientDTO(e.FirstName, e.LastName, e.Email)).ToImmutable(),
                    data: new Dictionary<string, string> {
                            { "project_name", dealName },
                            { "deal_id", dealId.ToString() } }
                        .ToImmutableDictionary()
                    )
            );
    private Option<Unit, ErrorCode> NotifyInsurersOfSubmissionDecline(IEnumerable<Guid> insuranceCompanyIds, Guid dealId, string dealName) =>
        insuranceCompanyIds
            .Select(id => _companyService.GetEmployeesForCompanyQuery(id))
            .Values()//ignoring errors, it's ok (for now) if some notifications do not get sent
            .SelectMany(e => e).Some<IEnumerable<EmployeeDTO>, ErrorCode>()
            .FlatMap(insurers => _notificationService
                .SendNotification(
                    type: NotificationType.Insurer_SubmissionFeedbackDeclined,
                    receipients: insurers.Select(e => new RecipientDTO(e.FirstName, e.LastName, e.Email)).ToImmutable(),
                    data: new Dictionary<string, string> {
                            { "project_name", dealName },
                            { "deal_id", dealId.ToString() } }
                        .ToImmutableDictionary()
                    )
            );

    public Option<Unit, ErrorCode> SubmitFeedbackCommand(UserId userId, SubmitDealFeedbackDTO submitData)
    {
        Company insurerCompany = new EmptyCompany();
        DealSubmission deal = new EmptySubmission();

        return
            _companyService.GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .Map(company => insurerCompany = company)
            .FlatMap(_ => _dealRepo.GetFeedbackDetails(insurerCompany.Id, submitData.SubmissionId).Result)//verify the feedback is the company's
            .FlatMap(feedbackDetails => feedbackDetails.Submit())
            .FlatMap(updatedFeedback => _dealRepo.Update(updatedFeedback).Result)
            .FlatMap(_ => _dealRepo.GetSubmissionDetails(submitData.SubmissionId).Result)
            .Map(dealFromDb => deal = dealFromDb)
            .FlatMap(deal => _companyService.GetEmployeesForCompanyQuery(deal.BrokerCompanyId))
            .FlatMap(brokers => _notificationService
                .SendNotification(
                    type: NotificationType.Broker_NewSubmissionFeedback,
                    receipients: brokers.Select(e => new RecipientDTO(e.FirstName, e.LastName, e.Email)).ToImmutable(),
                    data: new Dictionary<string, string> {
                            { "project_name", deal.Name },
                            { "insurer_company", insurerCompany.Name },
                            { "deal_id", deal.Id.ToString() } }
                        .ToImmutableDictionary()
                    )
            ); ;
    }

    public Option<Unit, ErrorCode> DeclineSubmissionCommand(UserId userId, SubmitDealFeedbackDTO submitData)
    {
        Company insurerCompany = new EmptyCompany();
        DealSubmission deal = new EmptySubmission();

        return
            _companyService.GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .Map(company => insurerCompany = company)
            .FlatMap(_ => _dealRepo.GetFeedbackDetails(insurerCompany.Id, submitData.SubmissionId).Result)//verify the feedback is the company's
            .FlatMap(feedbackDetails => feedbackDetails.Decline())
            .FlatMap(updatedFeedback => _dealRepo.Update(updatedFeedback).Result)
            .FlatMap(_ => _dealRepo.GetSubmissionDetails(submitData.SubmissionId).Result)
            .Map(dealFromDb => deal = dealFromDb)
            .FlatMap(deal => _companyService.GetEmployeesForCompanyQuery(deal.BrokerCompanyId))
            .FlatMap(brokers => _notificationService
                .SendNotification(
                    type: NotificationType.Broker_SubmissionDeclined,
                    receipients: brokers.Select(e => new RecipientDTO(e.FirstName, e.LastName, e.Email)).ToImmutable(),
                    data: new Dictionary<string, string> {
                            { "project_name", deal.Name },
                            { "insurer_company", insurerCompany.Name },
                            { "deal_id", deal.Id.ToString() } }
                        .ToImmutableDictionary()
                    )
            );
    }

    public Option<Unit, ErrorCode> NudgeInsurerForFeedback(UserId userId, NudgeDTO nudgeDetails)
    {
        SubmissionFeedback feedback = new EmptyFeedback();

        return
            _companyService.GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .FlatMap(_ => _dealRepo.GetFeedbackDetails(nudgeDetails.InsuranceCompanyId, nudgeDetails.SubmissionId).Result)//verify the feedback exists
            .FlatMap(feedback => feedback.NoneWhen(f => f.Submitted, DealErrorCodes.FeedbackNudge_AlreadySubmitted))
            .FlatMap(feedback => feedback.NoneWhen(f => f.Declined, DealErrorCodes.FeedbackNudge_AlreadyDeclined))
            .Map(f => feedback = f)
            .FlatMap(_ => _companyService.GetEmployeesForCompanyQuery(nudgeDetails.InsuranceCompanyId))
            .FlatMap(insurers => _notificationService
                .SendNotification(
                    type: NotificationType.Insurer_SubmissionFeedbackNudge,
                    receipients: insurers.Select(e => new RecipientDTO(e.FirstName, e.LastName, e.Email)).ToImmutable(),
                    data: new Dictionary<string, string> {
                            { "project_name", feedback.Name },                                            
                            { "deal_id", feedback.SubmissionId.ToString() } }
                        .ToImmutableDictionary()
                    )
            );
    }

    public Option<IImmutableList<FileUploadResult>, ErrorCode> UploadSubmissionFilesCommand(UserId userId, Guid submissionId, IEnumerable<(FileDTO metadata, Stream content)> files)
    {
        DealSubmission deal = new EmptySubmission();
        IImmutableList<FileUploadResult> uploadResults = ImmutableList.Create<FileUploadResult>();

        return _companyService
            .GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .FlatMap(_ => _dealRepo.GetSubmissionDetails(submissionId).Result)//verify the deal exists
            .Map(d => deal = d)
            .Map(_ => files
                .ToList()
                .Select(f => _fileService.UploadAsync(submissionId, f.metadata, f.content)))
            .Map(async tasks => await Task.WhenAll(tasks))
            .Map(allTasks => uploadResults = allTasks.Result.ToImmutable()) 
            .Map(uploadResults => uploadResults
                .Where(r => r.Uploaded)
                .Select(r => DealFile.Factory.ToEntity(r.File))
                .ToImmutable()) 
            .FlatMap(uploadedFiles => deal.AddFiles(uploadedFiles))
            .FlatMap(deal => _dealRepo.Update(deal).Result)
            .Map(_ => uploadResults);
    }

    public Option<Unit, ErrorCode> DeleteSubmissionFileCommand(UserId userId, Guid submissionId, Guid fileId)
    {
        DealSubmission deal = new EmptySubmission();

        return _companyService
            .GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .FlatMap(_ => _dealRepo.GetSubmissionDetails(submissionId).Result)//verify the deal exists
            .Map(d => deal = d)
            .FlatMap(d => deal.Files.SingleOrDefault(f => f.Id == fileId).SomeNotNull(), DealErrorCodes.DealFileNotFound)
            .Map(async file => await _fileService.DeleteAsync(submissionId, file.StoredFileName))
            .FlatMap(_ => deal.RemoveFile(fileId))
            .FlatMap(deal => _dealRepo.Update(deal).Result);
    }

    public Option<FileDownloadResult, ErrorCode> DownloadSubmissionFileCommand(UserId userId, Guid submissionId, Guid fileId)
    {
        DealFile file = new EmptyFile();

        return _companyService
            .GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .FlatMap(_ => _dealRepo.GetSubmissionDetails(submissionId).Result)//verify the deal exists
            .FlatMap(deal => deal.Files.SingleOrDefault(f => f.Id == fileId).SomeNotNull(), DealErrorCodes.DealFileNotFound)
            .Map(dealFile => file = dealFile)
            .Map(async dealFile => await _fileService.DownloadAsync(submissionId, dealFile.StoredFileName, dealFile.Type))
            .FlatMap(task => task.Result)
            .Map(content => new FileDownloadResult(DealFile.Factory.ToDTO(file), content));
    }

    public Option<Unit, ErrorCode> AcceptFileCommand(UserId userId, AcceptFileDTO acceptData)
    {
        return
            _companyService.GetCompanyOfUserQuery(userId)//verify the user is part of a company
            .FlatMap(_ => _dealRepo.GetFeedbackDetails(acceptData.InsuranceCompanyId, acceptData.SubmissionId).Result)//verify the feedback is the company's
            .FlatMap(feedbackDetails => feedbackDetails.AcceptNda())
            .FlatMap(updatedFeedback => _dealRepo.Update(updatedFeedback).Result);
    }
}
