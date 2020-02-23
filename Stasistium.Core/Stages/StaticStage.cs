﻿using Stasistium.Core;
using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class StaticStage<TResult> : StageBase<TResult, string>
    {
        private readonly string id;
        private readonly Func<TResult, string> hashFunction;
        public TResult Value { get; set; }

        public StaticStage(string id, TResult result, Func<TResult, string> hashFunction, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.id = id;
            this.Value = result;
            this.hashFunction = hashFunction ?? throw new ArgumentNullException(nameof(hashFunction));
        }

        protected override Task<StageResult<TResult, string>> DoInternal([AllowNull] string? cache, OptionToken options)
        {
            var contentHash = this.hashFunction(this.Value);
            var result = this.Context.CreateDocument(this.Value, contentHash, this.id);
            return Task.FromResult(StageResult.CreateStageResult(
                this.Context,
                result: result,
                cache: result.Hash,
                hasChanges: cache != result.Hash,
                documentId: this.id,
                hash: result.Hash));
        }
    }


    //public class ConcatStageMany2<T, TItemCache1, TCache1> : MultiStageBase<T, string, ConcatStageManyCache<TCache1>>
    //    where TItemCache1 : class
    //    where TCache1 : class
    //{
    //    MultiStageBase<T, TCache1, TCache1> input;

    //    public ConcatStageMany2(GeneratorContext context) : base(context)
    //    {
    //    }

    //    protected override async Task<StageResultList<T, string, ConcatStageManyCache<TCache1>>> DoInternal([AllowNull] ConcatStageManyCache<TCache1>? cache, OptionToken options)
    //    {
    //        var result = await this.input.DoIt(cache?.PreviousCache, options).ConfigureAwait(false);

    //        var task = LazyTask.Create(async () =>
    //        {
    //            var list = ImmutableList<StageResult<T, string>>.Empty.ToBuilder();
    //            var newCache = new ConcatStageManyCache<TCache1>();

    //            var hashList = new List<string>();
    //            if (result.HasChanges)
    //            {
    //                var performed = await result.Perform;
    //                newCache.Ids = new string[performed.Count];
    //                newCache.PreviousCache = result.Cache;

    //                for (int i = 0; i < performed.Count; i++)
    //                {
    //                    var child = performed[i];

    //                    if (child.HasChanges)
    //                    {
    //                        var childPerformed = await child.Perform;

    //                        string? oldHash = null;
    //                        if (cache != null && !cache.IdToHash.TryGetValue(childPerformed.Id, out oldHash))
    //                            throw this.Context.Exception("Should Not Happen");
    //                        var childHashChanges = oldHash != childPerformed.Hash;

    //                        list.Add(StageResult.CreateStageResult(this.Context, childPerformed, childHashChanges, childPerformed.Id, childPerformed.Hash, childPerformed.Hash));
    //                        newCache.IdToHash.Add(child.Id, childPerformed.Id);

    //                    }
    //                    else
    //                    {

    //                        var childTask = LazyTask.Create(async () =>
    //                        {
    //                            var childPerform = await child.Perform;
    //                            return childPerform;
    //                        });
    //                        if (cache is null || !cache.IdToHash.TryGetValue(child.Id, out var oldHash))
    //                            throw this.Context.Exception("Should Not Happen");
    //                        list.Add(StageResult.CreateStageResult(this.Context, childTask, false, child.Id, oldHash, oldHash));
    //                        newCache.IdToHash.Add(child.Id, oldHash);

    //                    }
    //                    newCache.Ids[i] = child.Id;
    //                    hashList.Add(child.Hash);
    //                }

    //            }
    //            else
    //            {
    //                if (cache is null)
    //                    throw this.Context.Exception("Should Not Happen");
    //                for (int i = 0; i < cache.Ids.Length; i++)
    //                {

    //                    var childTask = LazyTask.Create(async () =>
    //                    {
    //                        var performed = await result.Perform;
    //                        var chiledIndex = performed[i];
    //                        var childPerform = await chiledIndex.Perform;
    //                        // We are in the no changes part. So ther must be no changes.
    //                        System.Diagnostics.Debug.Assert(!chiledIndex.HasChanges);

    //                        return childPerform;
    //                    });

    //                    if (cache is null || !cache.IdToHash.TryGetValue(cache.Ids[i], out var oldHash))
    //                        throw this.Context.Exception("Should Not Happen");
    //                    list.Add(this.Context.CreateStageResult(childTask, false, cache.Ids[i], oldHash, oldHash));
    //                    hashList.Add(oldHash);
    //                }
    //                newCache.PreviousCache = cache.PreviousCache;
    //                newCache.Ids = cache.Ids;
    //                foreach (var id in newCache.Ids)
    //                {
    //                    if (!cache.IdToHash.TryGetValue(id, out var oldHash))
    //                        throw this.Context.Exception("Should Not Happen");
    //                    newCache.IdToHash.Add(id, oldHash);
    //                }
    //            }
    //            newCache.Hash = this.Context.GetHashForObject(hashList);


    //            return (result: list.ToImmutable(), cache: newCache);
    //        });

    //        var hasChanges = result.HasChanges;
    //        var ids = ImmutableList<string>.Empty.ToBuilder();

    //        if (hasChanges || cache is null)
    //        {
    //            var performed = await task;
    //            hasChanges = performed.result.Any(x => x.HasChanges);
    //            if (!hasChanges && cache != null)
    //            {
    //                hasChanges = !performed.cache.Ids1.SequenceEqual(cache.Ids);
    //            }
    //            ids.AddRange(performed.cache.Ids1);

    //            return this.Context.CreateStageResultList(performed.result, hasChanges, ids.ToImmutable(), performed.cache, performed.cache.Hash,, result.Cache);
    //        }
    //        else
    //        {
    //            ids.AddRange(cache.Ids);
    //        }
    //        var actualTask = LazyTask.Create(async () =>
    //        {
    //            var temp = await task;
    //            return temp.result;
    //        });
    //        return this.Context.CreateStageResultList(actualTask, hasChanges, ids.ToImmutable(), cache, cache.Hash, result.Cache);
    //    }
    //}

    ////public class ConcatStageManyCache<TPrevious>
    ////{
    ////    public TPrevious PreviouseCache1 { get; set; }
    ////    public string[] Ids1 { get; set; }
    ////    public Dictionary<string, string> IdToHash { get; set; }
    ////}


}
