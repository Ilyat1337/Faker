using System;

namespace FakerLib
{
    internal interface IObjectCreator
    {
        object Create(Type objectType);
    }
}
