﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.Validators;

public class PredicateValidator<T> : IValidator<T>
{
    public PredicateValidator(Func<T, bool> doValidate)
    {
        this.doValidate = doValidate;
    }

    public bool Validate(T input)
    {
        return doValidate(input);
    }

    private readonly Func<T, bool> doValidate;
}
