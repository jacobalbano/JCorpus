using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Validators.Decorators;

public static class InvertExtensions
{
    public static InvertDecorator<T> Inverted<T>(this IValidator<T> validator) => new InvertDecorator<T>(validator);
}

public class InvertDecorator<T> : IValidator<T>
{
    public InvertDecorator(IValidator<T> validator)
    {
        this.validator = validator;
    }

    public bool Validate(T input) => !validator.Validate(input);

    private readonly IValidator<T> validator;
}
