using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Typewriter.TemplateEditor.Controllers
{
    internal class CompletionSource : ICompletionSource
    {
        private readonly CompletionSourceProvider sourceProvider;
        private readonly ITextBuffer buffer;
        private readonly IGlyphService glyphService;

        public CompletionSource(CompletionSourceProvider sourceProvider, ITextBuffer buffer, IGlyphService glyphService)
        {
            this.sourceProvider = sourceProvider;
            this.buffer = buffer;
            this.glyphService = glyphService;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var point = session.TextView.Caret.Position.BufferPosition;
            var line = point.GetContainingLine();

            if (line == null)
            {
                return;
            }

            var text = line.GetText();
            var index = point - line.Start;

            var start = index;

            if (index > 0)
            {
                for (var i = index; i > 0; i--)
                {
                    var current = text[i - 1];
                    if (current == '$' || current == '#')
                    {
                        start = i - 1;
                        break;
                    }

                    if (current != '_' && !char.IsLetterOrDigit(current))
                    {
                        break;
                    }

                    start = i - 1;
                }
            }

            var span = new SnapshotSpan(point.Snapshot, start + line.Start, point - (start + line.Start));

            // Log.Debug("[" + span.GetText() + "]");
            var trackingSpan = buffer.CurrentSnapshot.CreateTrackingSpan(span.Start, span.Length, SpanTrackingMode.EdgeInclusive);
            var completions = Editor.Instance.GetCompletions(buffer, span, glyphService);

            completionSets.Add(new StringCompletionSet("Identifiers", trackingSpan, completions));
        }

        class StringCompletionSet : CompletionSet
        {
            public StringCompletionSet(string moniker, ITrackingSpan span, IEnumerable<Completion> completions)
                : base(moniker, nameof(Typewriter), span, completions, null) { }

            public override void SelectBestMatch()
            {
                base.SelectBestMatch(CompletionMatchType.MatchInsertionText, true);
                if (SelectionStatus.IsSelected)
                {
                    return;
                }

                base.SelectBestMatch(CompletionMatchType.MatchInsertionText, false);
            }
        }

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }
    }
}