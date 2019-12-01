﻿using System.IO;
using System.Linq;
using System.Reflection;
using Warcraft.NET.Attribute;
using Warcraft.NET.Extensions;
using Warcraft.NET.Files.Interfaces;
using Warcraft.NET.Files.WMO.Chunks;
using Warcraft.NET.Files.WMO.Chunks.BfA;
using Warcraft.NET.Files.WMO.Chunks.Legion;

namespace Warcraft.NET.Files.WMO.WorldMapObject.BfA
{
    public class WorldMapObjectRoot : WorldMapObjectRootBase
    {
        /// <summary>
        /// Gets or sets the WMO header
        /// </summary>
        [ChunkOrder(2)]
        public MOHD Header { get; set; }

        /// <summary>
        /// Gets or sets textures.
        /// Starting with 8.1, MOTX is no longer used.
        /// The texture references in MOMT are file data ids directly.
        /// As of that version, there is a fallback mode though and some files still use MOTX for sake of avoiding re-export.
        /// To check if texture references in MOMT are file data ids, simply check if MOTX exist in file 
        /// </summary>
        [ChunkOrder(3), ChunkOptional]
        public MOTX Textures { get; set; }

        /// <summary>
        /// Gets or sets the materials.
        /// </summary>
        [ChunkOrder(4)]
        public MOMT Materials { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Legion.WorldMapObjectRoot"/> class.
        /// </summary>
        /// <param name="inData">The binary data.</param>
        public WorldMapObjectRoot(byte[] inData) : base(inData)
        {
        }

        /// <summary>
        /// Serializes the current object into a byte array.
        /// </summary>
        /// <returns>The serialized object.</returns>
        public override byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                var terrainChunkProperties = GetType()
                    .GetProperties()
                    .OrderBy(p => ((ChunkOrderAttribute)p.GetCustomAttributes(typeof(ChunkOrderAttribute), false).Single()).Order);

                foreach (PropertyInfo chunkPropertie in terrainChunkProperties)
                {
                    IIFFChunk chunk = (IIFFChunk)chunkPropertie.GetValue(this);

                    if (chunk != null)
                    {
                        bw
                        .GetType()
                        .GetExtensionMethod(Assembly.GetExecutingAssembly(), "WriteIFFChunk")
                        .MakeGenericMethod(chunkPropertie.PropertyType)
                        .Invoke(null, new object[] { bw, chunkPropertie.GetValue(this), false });
                    }
                }

                return ms.ToArray();
            }
        }
    }
}
