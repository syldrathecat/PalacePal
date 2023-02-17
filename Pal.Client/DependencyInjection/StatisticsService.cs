﻿using System;
using System.Threading.Tasks;
using Dalamud.Game.Gui;
using Grpc.Core;
using Pal.Client.Configuration;
using Pal.Client.Extensions;
using Pal.Client.Net;
using Pal.Client.Properties;
using Pal.Client.Windows;

namespace Pal.Client.DependencyInjection
{
    internal sealed class StatisticsService
    {
        private readonly IPalacePalConfiguration _configuration;
        private readonly RemoteApi _remoteApi;
        private readonly StatisticsWindow _statisticsWindow;
        private readonly Chat _chat;

        public StatisticsService(IPalacePalConfiguration configuration, RemoteApi remoteApi,
            StatisticsWindow statisticsWindow, Chat chat)
        {
            _configuration = configuration;
            _remoteApi = remoteApi;
            _statisticsWindow = statisticsWindow;
            _chat = chat;
        }

        public void ShowGlobalStatistics()
        {
            Task.Run(async () => await FetchFloorStatistics());
        }

        private async Task FetchFloorStatistics()
        {
            if (!_configuration.HasRoleOnCurrentServer(RemoteApi.RemoteUrl, "statistics:view"))
            {
                _chat.Error(Localization.Command_pal_stats_CurrentFloor);
                return;
            }

            try
            {
                var (success, floorStatistics) = await _remoteApi.FetchStatistics();
                if (success)
                {
                    _statisticsWindow.SetFloorData(floorStatistics);
                    _statisticsWindow.IsOpen = true;
                }
                else
                {
                    _chat.Error(Localization.Command_pal_stats_UnableToFetchStatistics);
                }
            }
            catch (RpcException e) when (e.StatusCode == StatusCode.PermissionDenied)
            {
                _chat.Error(Localization.Command_pal_stats_CurrentFloor);
            }
            catch (Exception e)
            {
                _chat.Error(e.ToString());
            }
        }
    }
}
