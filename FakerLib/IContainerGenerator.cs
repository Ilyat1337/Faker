using System;
using System.Collections.Generic;
using System.Text;

namespace FakerLib
{
    interface IContainerGenerator
    {
        object Generate(Type elementType);
    }
}
