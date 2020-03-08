using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace ApiContractGenerator
{
    [DebuggerDisplay("{ToString(),nq}")]
    public sealed class TargetFrameworkAssembly
    {
        public TargetFrameworkAssembly(IAssemblySymbol assemblySymbol, ImmutableArray<string> uniquePreprocessorSymbols)
        {
            AssemblySymbol = assemblySymbol ?? throw new ArgumentNullException(nameof(assemblySymbol));
            UniquePreprocessorSymbols = uniquePreprocessorSymbols.IsDefault ? ImmutableArray<string>.Empty : uniquePreprocessorSymbols;
        }

        public IAssemblySymbol AssemblySymbol { get; }
        public ImmutableArray<string> UniquePreprocessorSymbols { get; }

        public static async Task<ImmutableArray<TargetFrameworkAssembly>> FromProjectAsync(Project project, CancellationToken cancellationToken)
        {
            var buildsOfSameAssembly = project.Solution.Projects.Where(p => p.AssemblyName == project.AssemblyName).ToList();

            var commonPreprocessorSymbols = buildsOfSameAssembly
                .Select(b => b.ParseOptions.PreprocessorSymbolNames)
                .IntersectAll();

            var targets = ImmutableArray.CreateBuilder<TargetFrameworkAssembly>(buildsOfSameAssembly.Count);

            foreach (var build in buildsOfSameAssembly)
            {
                var compilation = await build.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

                targets.Add(new TargetFrameworkAssembly(
                    compilation.Assembly,
                    build.ParseOptions.PreprocessorSymbolNames
                        .Where(name => !commonPreprocessorSymbols.Contains(name))
                        .ToImmutableArray()));
            }

            return targets.MoveToImmutable();
        }

        public override string ToString()
        {
            return UniquePreprocessorSymbols.Any()
                ? $"{AssemblySymbol.Name} ({string.Join(", ", UniquePreprocessorSymbols)})"
                : AssemblySymbol.Name;
        }
    }
}
