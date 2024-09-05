namespace AngryBirds.Payments.Exceptions;

public class PaymentException : Exception
{
    public string ErrorCode { get; }

    public PaymentException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public PaymentException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}