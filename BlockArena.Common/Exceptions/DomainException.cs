using System;

namespace BlockArena.Common.Exceptions
{
    public class DomainException(string message) : Exception(message)
    {

    }
}