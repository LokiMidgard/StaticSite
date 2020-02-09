﻿using Stasistium.Documents;
using System.IO;

namespace Stasistium.Stages
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public class SidecarHelper<TPreviousItemCache, TPreviousListCache>
        where TPreviousListCache : class
    where TPreviousItemCache : class
    {
        private readonly MultiStageBase<Stream, TPreviousItemCache, TPreviousListCache> stage;
        private readonly string? name;

        public SidecarHelper(MultiStageBase<Stream, TPreviousItemCache, TPreviousListCache> stage, string? name = null)
        {
            this.stage = stage;
            this.name = name;
        }

        public SidecarMetadata<TMetadata, TPreviousItemCache, TPreviousListCache> For<TMetadata>(string extension, MetadataUpdate<TMetadata>? updateCallback = null)
        {
            return new SidecarMetadata<TMetadata, TPreviousItemCache, TPreviousListCache>(this.stage, extension, updateCallback, this.stage.Context, this.name);
        }
    }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning restore CA1819 // Properties should not return arrays


}
