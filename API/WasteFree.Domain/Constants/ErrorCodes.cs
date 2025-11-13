namespace WasteFree.Domain.Constants;

public static class ApiErrorCodes
{
    public const string UsernameTaken = "USERNAME_TAKEN";
    public const string EmailTaken = "EMAIL_TAKEN";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string GenericError = "STH_WENT_WRONG";
    public const string EmptyImage = "EMPTY_IMAGE";
    public const string TooBigImage = "TOO_BIG_IMAGE";
    public const string UnsupportedImageType = "UNSUPPORTED_IMAGE_TYPE";
    public const string InvalidRegistrationToken = "INVALID_REGISTRATION_TOKEN";
    public const string InvalidUser = "INVALID_USER";
    public const string UserAccountNotActivated = "USER_NOT_ACTIVATED";
    public const string InvalidTopupCode = "INVALID_TOPUP_CODE";
    public const string MissingAccountNumber = "MISSING_ACCOUNT_NUMBER";
    public const string NotEnoughFunds = "NOT_ENOUGH_FUNDS";
    public const string NotFound = "NOT_FOUND";
    public const string Forbidden = "FORBIDDEN";
    public const string PaymentAlreadyCompleted = "PAYMENT_ALREADY_COMPLETED";
    public const string InvalidNotificationType = "INVALID_NOTIFICATION_TYPE";
    public const string InvitedUserNotFound = "INVITED_USER_NOT_FOUND";
    public const string AlreadyInGroup = "ALREADY_IN_GROUP";
    public const string ConsentNotFound = "CONSENT_NOT_FOUND";
	public const string ConsentContentRequired = "CONSENT_CONTENT_REQUIRED";
    public const string GeocodingFailed = "GEOCODING_FAILED";
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
    public const string GroupStreetRequired = "ERR_GROUP_STREET_REQUIRED";
    public const string ChatMessageRequired = "ERR_CHAT_MESSAGE_REQUIRED";
    public const string ChatMessageTooLong = "ERR_CHAT_MESSAGE_TOO_LONG";
    public const string InvalidRole = "INVALID_ROLE";
    public const string InvalidLang = "INVALID_LANG";
    
    public const string AmountOutsideRange = "AMOUNT_OUTSIDE_RANGE";
    public const string PaymentPropertyRequired = "PAYMENT_PROPERTY_REQUIRED";
    public const string PaymentCodeRequired = "PAYMENT_CODE_REQUIRED";
    
    public const string InvalidPaymentCode = "INVALID_PAYMENT_CODE";
    public const string PickupOptionInvalid = "ERR_PICKUP_OPTION_INVALID";
    public const string ContainerSizeRequired = "ERR_CONTAINER_SIZE_REQUIRED";
    public const string PickupDateRequired = "ERR_PICKUP_DATE_REQUIRED";
    public const string PickupDateInPast = "ERR_PICKUP_DATE_IN_PAST";
    public const string DropOffDateInPast = "ERR_DROP_OFF_DATE_IN_PAST";
    public const string UserIdsEmpty = "ERR_USER_IDS_EMPTY";
}
