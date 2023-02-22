﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pal.Client.Database;

namespace Pal.Client.Floors.Tasks
{
    internal sealed class LoadTerritory : DbTask<LoadTerritory>
    {
        private readonly MemoryTerritory _territory;

        public LoadTerritory(IServiceScopeFactory serviceScopeFactory, MemoryTerritory territory)
            : base(serviceScopeFactory)
        {
            _territory = territory;
        }

        protected override void Run(PalClientContext dbContext, ILogger<LoadTerritory> logger)
        {
            lock (_territory.LockObj)
            {
                if (_territory.IsReady)
                {
                    logger.LogInformation("Territory {Territory} is already loaded", _territory.TerritoryType);
                    return;
                }

                logger.LogInformation("Loading territory {Territory}", _territory.TerritoryType);
                List<ClientLocation> locations = dbContext.Locations
                    .Where(o => o.TerritoryType == (ushort)_territory.TerritoryType)
                    .Include(o => o.ImportedBy)
                    .Include(o => o.RemoteEncounters)
                    .AsSplitQuery()
                    .ToList();
                _territory.Initialize(locations.Select(ToMemoryLocation));

                logger.LogInformation("Loaded {Count} locations for territory {Territory}", locations.Count,
                    _territory.TerritoryType);
            }
        }

        public static PersistentLocation ToMemoryLocation(ClientLocation location)
        {
            return new PersistentLocation
            {
                LocalId = location.LocalId,
                Type = ToMemoryLocationType(location.Type),
                Position = new Vector3(location.X, location.Y, location.Z),
                Seen = location.Seen,
                RemoteSeenOn = location.RemoteEncounters.Select(o => o.AccountId).ToList(),
            };
        }

        private static MemoryLocation.EType ToMemoryLocationType(ClientLocation.EType type)
        {
            return type switch
            {
                ClientLocation.EType.Trap => MemoryLocation.EType.Trap,
                ClientLocation.EType.Hoard => MemoryLocation.EType.Hoard,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
