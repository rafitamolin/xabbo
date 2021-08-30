﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.Hosting;
using Xabbo.GEarth;

namespace b7.Xabbo.Services
{
    public class GEarthWpfExtensionLifetime : IHostLifetime
    {
        private readonly IHostApplicationLifetime _hostAppLifetime;
        private readonly Application _application;

        private readonly GEarthExtension _extension;

        public GEarthWpfExtensionLifetime(IHostApplicationLifetime hostAppLifetime, Application application,
            GEarthExtension extension)
        {
            _hostAppLifetime = hostAppLifetime;
            _application = application;

            _extension = extension;
            _extension.InterceptorConnectionFailed += OnInterceptorConnectionFailed;
            _extension.InterceptorConnected += OnInterceptorConnected;
            _extension.Clicked += OnExtensionClicked;
            _extension.InterceptorDisconnected += OnInterceptorDisconnected;

            _hostAppLifetime.ApplicationStopping.Register(() => application.Shutdown());
        }

        private void OnInterceptorConnectionFailed(object? sender, global::Xabbo.Interceptor.ConnectionFailedEventArgs e)
        {
            MessageBox.Show(
                $"Failed to connect to G-Earth on port {_extension.Options.Port}", "xabbo",
                MessageBoxButton.OK, MessageBoxImage.Error
            );

            _application.Shutdown();
        }

        private void OnInterceptorConnected(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_extension.Options.Cookie))
            {
                _application.MainWindow.Show();
            }
        }

        private void OnExtensionClicked(object? sender, EventArgs e)
        {
            if (_application.MainWindow.IsVisible)
            {
                _application.MainWindow.Activate();
            }
            else
            {
                _application.MainWindow.Show();
            }
        }

        private void OnInterceptorDisconnected(object? sender, global::Xabbo.Interceptor.DisconnectedEventArgs e)
        {
            _application.Shutdown();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _application.Shutdown();

            return Task.CompletedTask;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            _application.Exit += (s, e) => _hostAppLifetime.StopApplication();

            return Task.CompletedTask;
        }
    }
}