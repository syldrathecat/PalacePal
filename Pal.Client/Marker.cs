﻿using ECommons.SplatoonAPI;
using Pal.Client.Rendering;
using Pal.Common;
using Palace;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Pal.Client
{
    internal class Marker
    {
        public EType Type { get; set; } = EType.Unknown;
        public Vector3 Position { get; set; }

        /// <summary>
        /// Whether we have encountered the trap/coffer at this location in-game.
        /// </summary>
        public bool Seen { get; set; }

        /// <summary>
        /// Network id for the server you're currently connected to.
        /// </summary>
        [JsonIgnore]
        public Guid? NetworkId { get; set; }

        /// <summary>
        /// For markers that the server you're connected to doesn't know: Whether this was requested to be uploaded, to avoid duplicate requests.
        /// </summary>
        [JsonIgnore]
        public bool UploadRequested { get; set; }

        /// <summary>
        /// Which account ids this marker was seen. This is a list merely to support different remote endpoints
        /// (where each server would assign you a different id).
        /// </summary>
        public List<string> RemoteSeenOn { get; set; } = new();

        /// <summary>
        /// Whether this marker was requested to be seen, to avoid duplicate requests.
        /// </summary>
        [JsonIgnore]
        public bool RemoteSeenRequested { get; set; }

        /// <summary>
        /// To keep track of which markers were imported through a downloaded file, we save the associated import-id.
        /// 
        /// Importing another file for the same remote server will remove the old import-id, and add the new import-id here.
        /// </summary>
        public List<Guid> Imports { get; set; } = new();

        public bool WasImported { get; set; }

        /// <summary>
        /// To make rollbacks of local data easier, keep track of the version which was used to write the marker initially.
        /// </summary>
        public string? SinceVersion { get; set; }

        [JsonIgnore]
        public IRenderElement? RenderElement { get; set; }

        public Marker(EType type, Vector3 position, Guid? networkId = null)
        {
            Type = type;
            Position = position;
            NetworkId = networkId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, PalaceMath.GetHashCode(Position));
        }

        public override bool Equals(object? obj)
        {
            return obj is Marker otherMarker && Type == otherMarker.Type && PalaceMath.IsNearlySamePosition(Position, otherMarker.Position);
        }

        public static bool operator ==(Marker? a, object? b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(Marker? a, object? b)
        {
            return !Equals(a, b);
        }


        public bool IsPermanent() => Type == EType.Trap || Type == EType.Hoard;

        public enum EType
        {
            Unknown = ObjectType.Unknown,

            #region Permanent Markers
            Trap = ObjectType.Trap,
            Hoard = ObjectType.Hoard,

            [Obsolete]
            Debug = 3,
            #endregion

            # region Markers that only show up if they're currently visible
            SilverCoffer = 100,
            #endregion
        }
    }
}
