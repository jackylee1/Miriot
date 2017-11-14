﻿using Android.OS;
using Microsoft.ConnectedDevices;
using Miriot.Model;
using Miriot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Miriot.Common.Model;
using Newtonsoft.Json;

namespace Miriot.Mobile.Droid.Services
{
    public class RomeService : IRomeService
    {
        private RemoteSystemWatcher _remoteSystemWatcher;
        private List<RomeRemoteSystem> _remoteSystems;
        private AppServiceConnection _appServiceClientConnection;

        public bool IsInitialized { get; set; }

        public IReadOnlyList<RomeRemoteSystem> RemoteSystems => _remoteSystems.ToList();

        public Action<RomeRemoteSystem> Added { get; set; }

        public async Task InitializeAsync()
        {
            _remoteSystems = new List<RomeRemoteSystem>();

            if (_remoteSystemWatcher == null)
            {
                _remoteSystemWatcher = RemoteSystem.CreateWatcher();
                _remoteSystemWatcher.RemoteSystemAdded += RemoteSystemWatcherOnRemoteSystemAdded;
                _remoteSystemWatcher.Start();
            }

            await Task.FromResult(0);
        }

        private void RemoteSystemWatcherOnRemoteSystemAdded(RemoteSystemWatcher watcher, RemoteSystemAddedEventArgs args)
        {
            var system = ToRomeRemoteSystem(args.P0);

            _remoteSystems.Add(system);
            Added?.Invoke(system);
        }

        private static RomeRemoteSystem ToRomeRemoteSystem(RemoteSystem rs)
        {
            return new RomeRemoteSystem(rs)
            {
                DisplayName = rs.DisplayName,
                Id = rs.Id,
                Kind = rs.Kind.ToString(),
                Status = rs.Status.ToString()
            };
        }

        public Task<bool> RemoteLaunchUri(RomeRemoteSystem remoteSystem, Uri uri)
        {
            throw new NotImplementedException();
        }

        public async Task<T> CommandAsync<T>(RomeRemoteSystem remoteSystem, string command)
        {
            //   return RemoteLaunchUri(remoteSystem, new Uri(command));
            if (_appServiceClientConnection == null)
            {
                var system = (RemoteSystem)remoteSystem.NativeObject;
                var connectionRequest = new RemoteSystemConnectionRequest(system);

                // Construct an AppServiceClientConnection 
                _appServiceClientConnection =
                    new AppServiceConnection("", "", connectionRequest);

                try
                {
                    AppServiceConnectionStatus status = await _appServiceClientConnection.OpenRemoteAsync();
                    if (status != AppServiceConnectionStatus.Success)
                    {
                        _appServiceClientConnection = null;
                        return default(T);
                    }
                }
                catch (ConnectedDevicesException e)
                {
                    _appServiceClientConnection = null;
                    return default(T);
                }
            }

            var message = new Bundle();
            message.PutString("Command", command);
            try
            {
                var response = await _appServiceClientConnection.SendMessageAsync(message);

                if (response.Status == AppServiceResponseStatus.Success)
                {
                    var res = response.Message.GetString("Result");
                    return JsonConvert.DeserializeObject<T>(res.ToString());
                }
            }
            catch (Exception e)
            {
                return default(T);
            }

            return default(T);
        }

        public Task<bool> ConnectAsync(RomeRemoteSystem remoteSystem)
        {
            throw new NotImplementedException();
        }

        public async Task<RomeRemoteSystem> GetDeviceByAddressAsync(string ipAddress)
        {
            throw new NotImplementedException();
        }

        public Task CommandAsync(string command)
        {
            throw new NotImplementedException();
        }

        public Task<T> CommandAsync<T>(string command)
        {
            throw new NotImplementedException();
        }
    }
}