using System;

namespace SCIL
{
    class EmitterOrderAttribute : Attribute
    {
        public EmitterOrderAttribute(uint order)
        {
            Order = order;
        }

        public uint Order { get; }
    }
}