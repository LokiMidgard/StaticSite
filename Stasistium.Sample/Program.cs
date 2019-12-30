﻿using Stasistium.Documents;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Stasistium.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var context = new GeneratorContext(objectToStingRepresentation: obj =>


                obj switch
                {
                    GitMetadata meta => $"{meta.Name}|{meta.Type}",
                    _ => throw new NotSupportedException()
                }

            );
            var startModule = context.StageFromResult("https://github.com/nota-game/nota.git", x => x);

            var layoutProvider = context.StageFromResult("layout", x => x).FileSystem().FileProvider("Layout");

            var generatorOptions = new GenerationOptions()
            {
                CompressCache = true,
                Refresh = false
            };
            var s = System.Diagnostics.Stopwatch.StartNew();
            var files = startModule
                .GitModul()
                .SelectMany(input =>

                    input
                    .Transform(x => x.With(x.Metadata.Add(new GitMetadata() { Name = x.Value.FrindlyName, Type = x.Value.Type })))
                    .GitRefToFiles()
                    .Sidecar()
                        .For<BookMetadata>(".metadata")
                    .Where(x => System.IO.Path.GetExtension(x.Id) == ".md")
                    .Select(x => x.Markdown().MarkdownToHtml().TextToStream())
                    .Transform(x => x.WithId(Path.Combine(Enum.GetName(typeof(GitRefType), x.Metadata.GetValue<GitMetadata>()!.Type)!, x.Metadata.GetValue<GitMetadata>()!.Name, x.Id)))
                );
            //.Where(x => x.Id == "origin/master")
            //.SingleEntry()

            var razorProvider = files
                .FileProvider("Content")
                .Concat(layoutProvider)
                .RazorProvider("Content", "Layout/ViewStart.cshtml");



            var rendered = files.Select(x => x.Razor(razorProvider).TextToStream());

            var g = rendered
                .Transform(x => x.WithId(Path.ChangeExtension(x.Id, ".html")))
                .Persist(new DirectoryInfo("out"), generatorOptions)
                ;

            await g.UpdateFiles().ConfigureAwait(false);

        }


#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public class GitMetadata
        {
            public string Name { get; internal set; }
            public GitRefType Type { get; internal set; }
        }

        public class BookMetadata
        {
            public string Title { get; set; }
            public int Chapter { get; set; }
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
    public class PageLayoutMetadata
    {
        public string? Layout { get; set; }
    }

}
