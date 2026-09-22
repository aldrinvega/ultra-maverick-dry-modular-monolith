using System;
using System.Collections.Generic;
using System.Text;

namespace Ultramaverick.Identity.Application.Abstractions
{
    public interface IPasswordHasher
    {
        string Hash(string plainText);
        bool Verify(string plainText, string encodedHash);
    }
}
