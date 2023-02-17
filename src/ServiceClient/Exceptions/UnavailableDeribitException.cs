using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace ServiceClient.Exceptions;

public class UnavailableDeribitException : Exception
{
    public UnavailableDeribitException() :  base("Sorry, Deribit API is not available in this moment.")
    { }
}

