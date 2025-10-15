namespace WasteFree.Shared.Constants;

public static class ApiErrorCodes
{
    public const string UsernameTaken = "USERNAME_TAKEN";
    public const string EmailTaken = "EMAIL_TAKEN";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string GenericError = "STH_WENT_WRONG";
    public const string InvalidRegistrationToken = "INVALID_REGISTRATION_TOKEN";
    public const string InvalidUser = "INVALID_USER";
    public const string UserAccountNotActivated = "USER_NOT_ACTIVATED";
    public const string InvalidTopupCode = "INVALID_TOPUP_CODE";
    public const string MissingAccountNumber = "MISSING_ACCOUNT_NUMBER";
    public const string NotEnoughFunds = "NOT_ENOUGH_FUNDS";
    public const string NotFound = "NOT_FOUND";
    public const string Forbidden = "FORBIDDEN";
    public const string InvalidNotificationType = "INVALID_NOTIFICATION_TYPE";
    public const string InvitedUserNotFound = "INVITED_USER_NOT_FOUND";
    public const string AlreadyInGroup = "ALREADY_IN_GROUP";
}

public static class ValidationErrorCodes
{
    public const string InvalidEmail = "ERR_EMAIL_INVALID";
    
    public const string PasswordTooShort = "ERR_PASSWORD_TOO_SHORT";
    public const string UsernameRequired = "ERR_USERNAME_REQUIRED";
    public const string PasswordRequired = "ERR_PASSWORD_REQUIRED";
    public const string EmailRequired = "ERR_EMAIL_REQUIRED";
    public const string GroupNameRequired = "ERR_GROUP_NAME_REQUIRED";
    public const string GroupDescriptionRequired = "ERR_GROUP_DESCRIPTION_REQUIRED";
    public const string GroupCityRequired = "ERR_GROUP_CITY_REQUIRED";
    public const string GroupPostalCodeRequired = "ERR_GROUP_POSTAL_CODE_REQUIRED";
    public const string GroupPostalCodeInvalid = "ERR_GROUP_POSTAL_CODE_INVALID";
    public const string GroupAddressRequired = "ERR_GROUP_ADDRESS_REQUIRED";
    public const string InvalidRole = "INVALID_ROLE";
    public const string InvalidLang = "INVALID_LANG";
    
    public const string AmountOutsideRange = "AMOUNT_OUTSIDE_RANGE";
    public const string PaymentPropertyRequired = "PAYMENT_PROPERTY_REQUIRED";
    public const string PaymentCodeRequired = "PAYMENT_CODE_REQUIRED";
    
    public const string InvalidPaymentCode = "INVALID_PAYMENT_CODE";
}
