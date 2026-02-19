namespace Typewriter.TemplateEditor.Lexing
{
    public class ContextSpan
    {
        public ContextSpan(int start, int end, Context context, Context parentContext, ContextType type)
        {
            Start = start;
            End = end;
            Context = context;
            ParentContext = parentContext;
            Type = type;
        }

        public int Start { get; }

        public int End { get; }

        public Context Context { get; }

        public Context ParentContext { get; }

        public ContextType Type { get; }
    }
}