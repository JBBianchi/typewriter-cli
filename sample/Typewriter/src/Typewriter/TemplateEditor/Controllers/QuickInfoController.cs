using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Typewriter.TemplateEditor.Controllers
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [ContentType(Constants.ContentType)]
    [Name("Tooltip Source Provider")]
    internal class QuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new QuickInfoSource(this, textBuffer);
        }
    }

    internal class QuickInfoSource : IAsyncQuickInfoSource
    {
        private readonly QuickInfoSourceProvider _provider;
        private readonly ITextBuffer _buffer;

        public QuickInfoSource(QuickInfoSourceProvider provider, ITextBuffer buffer)
        {
            _provider = provider;
            _buffer = buffer;
        }

        public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            var snapshot = _buffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(snapshot);
            if (triggerPoint.HasValue)
            {
                var extent = GetExtentOfWord(triggerPoint.Value);
                if (extent.HasValue)
                {
                    var info = Editor.Instance.GetQuickInfo(_buffer, extent.Value);

                    if (info != null)
                    {
                        var span = snapshot.CreateTrackingSpan(extent.Value, SpanTrackingMode.EdgeInclusive);
                        return Task.FromResult(new QuickInfoItem(span, info));
                    }
                }
            }

            return Task.FromResult(default(QuickInfoItem));
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            GC.SuppressFinalize(this);
            _disposed = true;
        }

        private static SnapshotSpan? GetExtentOfWord(SnapshotPoint point)
        {
            var line = point.GetContainingLine();

            if (line == null)
            {
                return null;
            }

            var text = line.GetText();
            var index = point - line.Start;

            var start = index;
            var length = 0;

            if (index > 0 && index < line.Length)
            {
                for (var i = index; i >= 0; i--)
                {
                    var current = text[i];
                    if (current == '$')
                    {
                        start = i;
                        length++;
                        break;
                    }

                    if (current != '_' && !char.IsLetterOrDigit(current))
                    {
                        break;
                    }

                    start = i;
                    length++;
                }
            }

            if (length > 0)
            {
                index++;

                if (index < line.Length)
                {
                    for (var i = index; i < line.Length; i++)
                    {
                        var current = text[i];
                        if (current != '_' && !char.IsLetterOrDigit(current))
                        {
                            break;
                        }

                        length++;
                    }
                }

                var span = new SnapshotSpan(point.Snapshot, start + line.Start, length);

                //Log.Debug("[" + span.GetText() + "]");
                return span;
            }

            return null;
        }
    }

    [Export(typeof(IIntellisenseControllerProvider))]
    [ContentType(Constants.ContentType)]
    [Name("Intellisense Controller Provider")]
    internal class QuickInfoControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        internal IAsyncQuickInfoBroker QuickInfoBroker { get; set; }

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            return new QuickInfoController(textView, subjectBuffers, this);
        }
    }

    internal class QuickInfoController : IIntellisenseController
    {
        private ITextView view;
        private readonly IList<ITextBuffer> buffers;
        private readonly QuickInfoControllerProvider provider;

        internal QuickInfoController(ITextView view, IList<ITextBuffer> buffers, QuickInfoControllerProvider provider)
        {
            this.view = view;
            this.buffers = buffers;
            this.provider = provider;

            view.MouseHover += OnTextViewMouseHover;
        }

        private void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            var point = view.BufferGraph.MapDownToFirstMatch(new SnapshotPoint(view.TextSnapshot, e.Position), PointTrackingMode.Positive,
                snapshot => buffers.Contains(snapshot.TextBuffer), PositionAffinity.Predecessor);

            if (point == null)
            {
                return;
            }

            if (!provider.QuickInfoBroker.IsQuickInfoActive(view))
            {
                var triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position, PointTrackingMode.Positive);
                provider.QuickInfoBroker.TriggerQuickInfoAsync(view, triggerPoint, QuickInfoSessionOptions.TrackMouse);
            }
        }

        public void Detach(ITextView textView)
        {
            if (view != textView)
            {
                return;
            }

            textView.MouseHover -= OnTextViewMouseHover;
            view = null;
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }
    }
}
