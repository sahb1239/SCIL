namespace SCIL.Processor.Nodes.Visitor
{
    public interface IVisitor
    {
        void Visit(Node node);
        void Visit(Block block);
        void Visit(Method method);
        void Visit(Type type);
        void Visit(Module module);
    }
}
