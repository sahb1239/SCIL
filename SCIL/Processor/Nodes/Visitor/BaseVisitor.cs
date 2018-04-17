namespace SCIL.Processor.Nodes.Visitor
{
    public class BaseVisitor : IVisitor
    {
        public virtual void Visit(Block block)
        {
            block.Accept(this);
        }

        public virtual void Visit(Node node)
        {
            node.Accept(this);
        }

        public virtual void Visit(Method block)
        {
            block.Accept(this);
        }
    }
}