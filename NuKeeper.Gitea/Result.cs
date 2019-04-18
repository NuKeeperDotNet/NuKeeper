using System;

namespace NuKeeper.Gitea
{
    class Result<T>
    {
        private readonly T _value;
        public bool IsSuccessful { get; }

        public T Value => IsSuccessful
            ? _value
            : throw new InvalidOperationException("Trying to request a value of a failed result.");

        private Result(T value, bool isSuccessful)
        {
            _value = value;
            IsSuccessful = isSuccessful;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(value, true);
        }

        public static Result<T> Failure()
        {
            return new Result<T>(default, false);
        }
    }
}
