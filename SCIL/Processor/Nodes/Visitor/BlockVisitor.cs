namespace SCIL.Processor.Nodes.Visitor
{
    public interface IVisitor
    {
        void Visit(Node node);
        void Visit(Block block);
        void Visit(Method block);
        void Visit(Type type);
        void Visit(Module module);
    }
}
