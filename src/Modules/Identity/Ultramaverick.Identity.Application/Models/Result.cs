using System;
using System.Collections.Generic;
using System.Text;

namespace Ultramaverick.Identity.Application.Models
{
    public readonly record struct Result(bool Succeeded, string? Error)
    {
        public static Result Success() => new(true, null);
        public static Result Failure(string error) => new(false, error);
    }

    public readonly record struct Result<T>(bool Succeeded, T? Value, string? Error)
    {
        public static Result<T> Success(T value) => new(true, value, null);
        public static Result<T> Failure(string error) => new(false, default, error);
    }
}
