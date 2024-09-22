using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.Validators;

public interface IValidator<T>
{
    bool Validate(T input);
}
