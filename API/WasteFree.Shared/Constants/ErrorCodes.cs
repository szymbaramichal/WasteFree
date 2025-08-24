namespace WasteFree.Shared.Constants;

public static class ApiErrorCodes
{
    public const string UsernameTaken = "USERNAME_TAKEN";
    public const string EmailTaken = "EMAIL_TAKEN";
    public const string LoginFailed = "LOGIN_FAILED";
}

public static class ValidationErrorCodes
{
    public const string NotEmpty = "ERR_NOT_EMPTY";
    public const string InvalidEmail = "ERR_INVALID_EMAIL";
    public const string TooShort = "ERR_TOO_SHORT";

    public const string UsernameRequired = "ERR_USERNAME_REQUIRED";
    public const string PasswordRequired = "ERR_PASSWORD_REQUIRED";
    public const string EmailRequired = "ERR_EMAIL_REQUIRED";
    public const string GroupNameRequired = "ERR_GROUP_NAME_REQUIRED";
    public const string GroupDescriptionRequired = "ERR_GROUP_DESCRIPTION_REQUIRED";
}
