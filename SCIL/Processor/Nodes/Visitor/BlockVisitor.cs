namespace SCIL.Processor.Nodes.Visitor
{
    public interface IVisitor
    {
        void Visit(Block block);
        void Visit(Node node);
        void Visit(Method method);
    }
}
