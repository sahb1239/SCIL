using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.Nodes
{
    public abstract class Element
    {
        public abstract void Accept(IVisitor visitor);
    }
}