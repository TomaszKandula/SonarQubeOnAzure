﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using FluentValidation.Results;

namespace SonarQubeProxy.WebApi.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class ValidationException : BusinessException
{
    public ValidationResult ValidationResult { get; } = new();

    protected ValidationException(SerializationInfo serializationInfo, 
        StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

    public ValidationException(ValidationResult validationResult, string errorMessage = "") : base(errorMessage)
        => ValidationResult = validationResult;
}