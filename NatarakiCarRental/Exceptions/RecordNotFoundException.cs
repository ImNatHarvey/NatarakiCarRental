namespace NatarakiCarRental.Exceptions;

public sealed class RecordNotFoundException : Exception
{
    public RecordNotFoundException(string message)
        : base(message)
    {
    }
}
