using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.Validators;

public class OptionValidator<T> : IValidator<T>
{
    public OptionValidator(params T[] options)
    {
        this.options = new HashSet<T>(options);
    }

    public bool Validate(T input)
    {
        return options.Contains(input);
    }

    private readonly HashSet<T> options;
}
