using System.Collections.Generic;
using System.Linq;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.Nodes
{
    public abstract class Element
    {
        public abstract void Accept(IVisitor visitor);

        private readonly IList<object> _information = new List<object>();

        public void AddElement<TElement>(TElement element)
            where TElement : class
        {
            _information.Add(element);
        }

        public IEnumerable<TElement> GetElementInformations<TElement>()
            where TElement : class
        {
            return _information.OfType<TElement>();
        }
    }
}