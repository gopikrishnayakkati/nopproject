﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.Square.Services;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Payments.Square.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring services on application startup
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        /// <param name="environment">Environment of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            //client to request Square authorization service
            services.AddHttpClient<SquareAuthorizationHttpClient>().WithProxy();
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 101;
    }
}