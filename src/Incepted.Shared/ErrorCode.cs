namespace Incepted.Shared;

public record class ErrorCode(string type, string title, int status, string traceId, ErrorCodeDetail errors);
public record class ErrorCodeDetail(List<string> name);

public static class EmptyErrorCode
{
    public static ErrorCode Empty = new ErrorCode(
           string.Empty,
           string.Empty,
           0,
           string.Empty,
           new ErrorCodeDetail(new List<string>())
           );
}

public static class DealErrorCodes
{
    public static ErrorCode DealNotFound = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            "Deal could not be found",
            404,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Something went wrong while getting the deal details. Please reload the page and call Incepted support if the problem persists." })
            );
    public static ErrorCode FeedbackNotFound = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            "Deal feedback could not be found",
            404,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Something went wrong while getting the deal feedback details. Please reload the page and call Incepted support if the problem persists." })
            );
    public static ErrorCode NoDealsFound = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            "No Deals could be found",
            404,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "No Deals could be found." })
            );
    public static ErrorCode AssigneesAreNotValid = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "Assignees of the deal are not all part of the target company",
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't update deal assignees, please reload the page and try again." })
            );
    public static ErrorCode AssigneesCompanyDoesNotExistInSubmission = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "Broker or feedback Insurer company ids do not match the company id passed to updating assignees",
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't update deal assignees, please reload the page and try again." })
            );
    public static ErrorCode DataHasChanged = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "Optimistic concurrency check with ETag failed",
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "The data has changed since you loaded it but before you saved it. Please reload the page and try again." })
            );
    public static ErrorCode FailedToCreateSubmission_DbError(Exception e) => new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            e.Message,
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't create the submission. Please try again and contact incepted support if the problem persists." })
            );
    public static ErrorCode FailedToUpdateSubmission_DbError(Exception e) => new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            e.Message,
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't update the submission. Please try again and contact incepted support if the problem persists." })
            );
    public static ErrorCode FailedToCreateFeedback_DbError(Exception e) => new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            e.Message,
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't create the submission feedback. Please try again and contact incepted support if the problem persists." })
            );
    public static ErrorCode FailedToUpdateFeedback_DbError(Exception e) => new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            e.Message,
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't update the submission feedback. Please try again and contact incepted support if the problem persists." })
            );
    public static ErrorCode FailedToCreateLiveDeal_DbError(Exception e) => new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            e.Message,
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't create the live deal. Please try again and contact incepted support if the problem persists." })
            );
    public static ErrorCode FailedToUpdateLiveDeal_DbError(Exception e) => new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            e.Message,
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't update the live deal. Please try again and contact incepted support if the problem persists." })
            );
    public static ErrorCode FailedToUpdateDeal_InvalidName = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "Could not update deal because name is empty",
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't update a deal with an empty name." })
            );
    public static ErrorCode FailedToUpdateDeal_InvalidDeadline = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "Could not update deal because name is empty",
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't update a deal with an deadline in the past." })
            );    
    public static ErrorCode DealFileNotFound = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            "Deals file could not be found",
            404,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "The deal does have such a file, please reload the page and try again." })
            );
    public static ErrorCode EnhancementAPNotValid = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "The AP value is not a valid percentage",
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Please enter a value between 0 and 100 for AP." })
            );
    public static ErrorCode FailedToModifyDeal_NoNotes = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "When modifying a deal you must add notes",
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "You must enter some notes for the insurer to understand what changed in the submission." })
            );
    public static ErrorCode FailedToModifyDeal_NotSubmittedYet = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "Can't modify a submission that is not submitted yet",
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "A deal must be submitted to at least one insurer if you are to modify it." })
            );
    public static ErrorCode FailedToGoLive_NotSubmittedYet = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "Can't go live with a submission/feedback that is not submitted yet",
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "A deal/feedback must be submitted be able to go live with it." })
            );
    public static ErrorCode FailedToModifyFeedback_NotSubmittedYet = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "Can't modify a submission feedback that is not submitted yet",
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "The feedback must be submitted if you are to modify it." })
            );
    public static ErrorCode FeedbackNudge_AlreadySubmitted = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "Can't send remider for submitted feedback",
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Submission feedback is already submitted. Refresh the page to see it." })
            );
    public static ErrorCode FeedbackNudge_AlreadyDeclined = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "Can't send remider for declined feedback",
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Submission feedback is declined. Please refresh the page." })
            );
    public static ErrorCode FailedToRetrieveAllSubmissionFeedbacks_DbError(Exception e) => new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            e.Message,
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't retrieve the submission feedbacks. Please try again and contact incepted support if the problem persists." })
            );
    public static ErrorCode FailedToRetrieveSingleSubmissionFeedback_DbError(Exception e) => new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            e.Message,
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't retrieve the submission feedback. Please try again and contact incepted support if the problem persists." })
            );
    public static ErrorCode FailedToRetrieveSingleSubmission_DbError(Exception e) => new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            e.Message,
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't retrieve the submission details. Please try again and contact incepted support if the problem persists." })
            );
    public static ErrorCode FailedToRetrieveAllSubmissions_DbError(Exception e) => new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            e.Message,
            400,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't retrieve the list of submissions. Please try again and contact incepted support if the problem persists." })
            );
}

public static class PricingErrorCodes
{
    public static ErrorCode NoLimits = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.6.1",
           "Can't have pricing with no limits",
           500,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "You have to select at least one limit of liability." })
           );

    public static ErrorCode NoRetentions = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.6.1",
           "Can't have pricing with no retentions",
           500,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "You have to select at least one retention/threshold." })
           );
}

public static class CompanyErrorCodes
{
    public static ErrorCode CompanyNotFound = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            "Company of employee could not be found or the employee is not assigned to a company",
            404,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Could not get the company of the logged in user. Please contact Incepted support and we'll get right on it." })
            );
    public static ErrorCode EmployeeNotFound = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            "Employee could not be found",
            404,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Can't find this employee in our database. Please contact Incepted support and we'll get right on it." })
            );
    public static ErrorCode FailedToGetCompanies = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.6.4",
            "Error in getting companies",
            500,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Oops! Something went wrong but it's not you, it's us. Please contact Incepted support and we'll get right on it." })
            );
    public static ErrorCode DefaultAssigneesNotFound_ByUserId = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.5.1",
           "A default assignee could not be found",
           400,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "A default assignee could not be found to create a deal with." })
           );
    public static ErrorCode DefaultTCsNotFound = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.5.4",
           "A default T&Cs file could not be found",
           404,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "Error getting the default T&Cs of the company." })
           );
    public static ErrorCode FailedToUploadDefaultTCs = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.5.1",
           "Default T&Cs file could not be uploaded",
           400,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "Could not upload the default T&Cs of the company." })
           );
    public static ErrorCode FirstNameEmpty = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.5.1",
           "The First name can't be empty",
           400,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "The first name can't be empty." })
           );
    public static ErrorCode LastNameEmpty = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.5.1",
           "The Last name can't be empty",
           400,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "The last name can't be empty." })
           );
    public static ErrorCode InvalidEmail = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.5.1",
           "The Email is invalid",
           400,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "The email is not valid." })
           );
}

public static class FileErrorCodes
{
    public static ErrorCode TooManyFiles = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.5.1",
           "Too many files attempted to be uploaded at the same time",
           400,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "Can't upload so many files at once! Try few at a time." })
           );
    public static ErrorCode ExtensionNotAllowed = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.5.1",
           "The file extension is not in the allowed list",
           400,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "File does not have an allowed extension. Try .pdf or .docx" })
           );
    public static ErrorCode FileTooLarge = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.5.1",
           "The file extension is too big",
           400,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "File is too big. We only allow uploads up to 100MB for the time being." })
           );
    public static ErrorCode FileSignatureNotValid = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.5.1",
           "The file extension did not match any valid file signatures",
           400,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "File contents do not match its extension. We can't upload the file as it looks suspicious." })
           );
    public static ErrorCode FileNameTooLong = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.5.1",
           "The file name exceeded the allowed length",
           400,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "File has a file name that is too long. Rename to something shorter and try again please!" })
           );
    public static ErrorCode InvalidCharacters = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.5.1",
           "The file name has some invalid characters",
           400,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "File has some characters that are not allowed for security reasons. Can you only use alphanumeric, brackets, dots, hyphens and underscores?" })
           );
    public static ErrorCode BlobContainerNotFound = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            "Azure storage container could not be found",
            404,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Oops! Something went wrong. Please contact Incepted support and we'll get right on it." })
            );
    public static ErrorCode RemoteStorageFailure_Fetch = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.6.1",
           "Error while retrieving files from remote storage",
           500,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "Could not retrieve files from the secure storage. Please reload the page and call Incepted support if the problem persists." })
           );
    public static ErrorCode RemoteStorageFailure_Save = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.6.1",
           "Error while saving file to remote storage",
           500,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "The file could not be saved in the secure storage. Please reload the page and call Incepted support if the problem persists." })
           );
    public static ErrorCode RemoteStorageFailure_Delete = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.6.1",
           "Error while deleting file from remote storage",
           500,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "The file could not be deleted from the secure storage. Please reload the page and call Incepted support if the problem persists." })
           );
    public static ErrorCode RemoteStorageFailure_Download = new ErrorCode(
           "https://tools.ietf.org/html/rfc7231#section-6.6.1",
           "Error while downloading file from remote storage",
           500,
           string.Empty,
           new ErrorCodeDetail(new List<string> { "The file could not be downloaded from the secure storage. Please reload the page and call Incepted support if the problem persists." })
           );
}

public static class NotificationErrorCodes
{
    public static ErrorCode FailedToSendNotification_Email = new ErrorCode(
            "https://tools.ietf.org/html/rfc7231#section-6.6.4",
            "Error when sending email notification",
            500,
            string.Empty,
            new ErrorCodeDetail(new List<string> { "Something went wrong while sending some email notifications. We'll look into it. In the meantime please reach out to the recipient(s) if needed." })
            );    
}