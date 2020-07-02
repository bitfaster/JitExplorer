using ICSharpCode.AvalonEdit.CodeCompletion;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JitExplorer.Completion
{
    public class RoslynCodeCompletion
    {
        private readonly AdhocWorkspace workspace;
        private Project project;

        private static readonly object mutex = new object();
        private static bool isIinitialzed;

        public RoslynCodeCompletion(IEnumerable<MetadataReference> metadataReferences)
        {
            workspace = new AdhocWorkspace(MefHostServices.Create(MefHostServices.DefaultAssemblies));

            var projectInfo = ProjectInfo
                .Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Project", "Project", LanguageNames.CSharp)
                .WithMetadataReferences(metadataReferences);

            project = workspace.AddProject(projectInfo);
        }

        public void Initialize()
        {
            lock (mutex)
            {
                if (!isIinitialzed)
                {
                    this.CompleteAsync(string.Empty, 0, null)
                        .ConfigureAwait(false);

                    isIinitialzed = true;
                }
            }
        }

        public async Task<IEnumerable<ICompletionData>> CompleteAsync(string sourceCode, int position, char? triggerChar)
        {
            var sourceText = SourceText.From(sourceCode);
            var document = project.AddDocument("File.cs", sourceText);

            project = document.Project;

            var completionService = CompletionService.GetService(document);
            var completionTrigger = GetCompletionTrigger(triggerChar);

            var data = await completionService
                    .GetCompletionsAsync(document, position, completionTrigger, null, null, CancellationToken.None)
                    .ConfigureAwait(false);

            if (data == null || data.Items == null)
                return Array.Empty<ICompletionData>();

            var helper = CompletionHelper.GetHelper(document);
            var textSpanToText = new Dictionary<TextSpan, string>();

            return data.Items
                    .Where(item => MatchesFilterText(helper, item, sourceText, textSpanToText))
                    .Select(x => new CompletionData(x.DisplayText))
                    //.Distinct(item => x.DisplayText)       
                    .ToArray();
        }

        private static CompletionTrigger GetCompletionTrigger(char? triggerChar)
            => triggerChar != null
                ? CompletionTrigger.CreateInsertionTrigger(triggerChar.Value)
                : CompletionTrigger.Invoke;

        // ref: RoslynPad -- https://github.com/aelij/RoslynPad
        private static bool MatchesFilterText(CompletionHelper helper, CompletionItem item, SourceText text, Dictionary<TextSpan, string> textSpanToText)
        {
            var filterText = GetFilterText(item, text, textSpanToText);

            return string.IsNullOrEmpty(filterText) || helper.MatchesPattern(item.FilterText, filterText, CultureInfo.InvariantCulture);
        }

        private static string GetFilterText(CompletionItem item, SourceText text, Dictionary<TextSpan, string> textSpanToText)
        {
            var textSpan = item.Span;

            if (textSpanToText.TryGetValue(textSpan, out var filterText) == false)
            {
                filterText = text.GetSubText(textSpan).ToString();
                textSpanToText[textSpan] = filterText;
            }

            return filterText;
        }
    }
}
