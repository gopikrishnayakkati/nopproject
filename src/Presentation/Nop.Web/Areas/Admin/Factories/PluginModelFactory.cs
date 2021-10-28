using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Plugins.Marketplace;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Plugins;
using Nop.Web.Areas.Admin.Models.Plugins.Marketplace;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the plugin model factory implementation
    /// </summary>
    public partial class PluginModelFactory : IPluginModelFactory
    {
        #region Fields

        protected IAclSupportedModelFactory AclSupportedModelFactory { get; }
        protected IAuthenticationPluginManager AuthenticationPluginManager { get; }
        protected IBaseAdminModelFactory BaseAdminModelFactory { get; }
        protected ILocalizationService LocalizationService { get; }
        protected ILocalizedModelFactory LocalizedModelFactory { get; }
        protected IMultiFactorAuthenticationPluginManager MultiFactorAuthenticationPluginManager { get; }
        protected IPaymentPluginManager PaymentPluginManager { get; }
        protected IPickupPluginManager PickupPluginManager { get; }
        protected IPluginService PluginService { get; }
        protected IShippingPluginManager ShippingPluginManager { get; }
        protected IStaticCacheManager StaticCacheManager { get; }
        protected IStoreMappingSupportedModelFactory StoreMappingSupportedModelFactory { get; }
        protected ITaxPluginManager TaxPluginManager { get; }
        protected IWidgetPluginManager WidgetPluginManager { get; }
        protected IWorkContext WorkContext { get; }
        protected OfficialFeedManager OfficialFeedManager { get; }

        #endregion

        #region Ctor

        public PluginModelFactory(IAclSupportedModelFactory aclSupportedModelFactory,
            IAuthenticationPluginManager authenticationPluginManager,
            IBaseAdminModelFactory baseAdminModelFactory,
            ILocalizationService localizationService,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            ILocalizedModelFactory localizedModelFactory,
            IPaymentPluginManager paymentPluginManager,
            IPickupPluginManager pickupPluginManager,
            IPluginService pluginService,
            IShippingPluginManager shippingPluginManager,
            IStaticCacheManager staticCacheManager,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            ITaxPluginManager taxPluginManager,
            IWidgetPluginManager widgetPluginManager,
            IWorkContext workContext,
            OfficialFeedManager officialFeedManager)
        {
            AclSupportedModelFactory = aclSupportedModelFactory;
            AuthenticationPluginManager = authenticationPluginManager;
            BaseAdminModelFactory = baseAdminModelFactory;
            LocalizationService = localizationService;
            LocalizedModelFactory = localizedModelFactory;
            MultiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            PaymentPluginManager = paymentPluginManager;
            PickupPluginManager = pickupPluginManager;
            PluginService = pluginService;
            ShippingPluginManager = shippingPluginManager;
            StaticCacheManager = staticCacheManager;
            StoreMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            TaxPluginManager = taxPluginManager;
            WidgetPluginManager = widgetPluginManager;
            WorkContext = workContext;
            OfficialFeedManager = officialFeedManager;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare plugin model properties of the installed plugin
        /// </summary>
        /// <param name="model">Plugin model</param>
        /// <param name="plugin">Plugin</param>
        protected virtual void PrepareInstalledPluginModel(PluginModel model, IPlugin plugin)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));

            //prepare configuration URL
            model.ConfigurationUrl = plugin.GetConfigurationPageUrl();

            //prepare enabled/disabled (only for some plugin types)
            model.CanChangeEnabled = true;
            switch (plugin)
            {
                case IPaymentMethod paymentMethod:
                    model.IsEnabled = PaymentPluginManager.IsPluginActive(paymentMethod);
                    break;

                case IShippingRateComputationMethod shippingRateComputationMethod:
                    model.IsEnabled = ShippingPluginManager.IsPluginActive(shippingRateComputationMethod);
                    break;

                case IPickupPointProvider pickupPointProvider:
                    model.IsEnabled = PickupPluginManager.IsPluginActive(pickupPointProvider);
                    break;

                case ITaxProvider taxProvider:
                    model.IsEnabled = TaxPluginManager.IsPluginActive(taxProvider);
                    break;

                case IExternalAuthenticationMethod externalAuthenticationMethod:
                    model.IsEnabled = AuthenticationPluginManager.IsPluginActive(externalAuthenticationMethod);
                    break;

                case IMultiFactorAuthenticationMethod multiFactorAuthenticationMethod:
                    model.IsEnabled = MultiFactorAuthenticationPluginManager.IsPluginActive(multiFactorAuthenticationMethod);
                    break;

                case IWidgetPlugin widgetPlugin:
                    model.IsEnabled = WidgetPluginManager.IsPluginActive(widgetPlugin);
                    break;

                default:
                    model.CanChangeEnabled = false;
                    break;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare plugin search model
        /// </summary>
        /// <param name="searchModel">Plugin search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the plugin search model
        /// </returns>
        public virtual async Task<PluginSearchModel> PreparePluginSearchModelAsync(PluginSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available load plugin modes
            await BaseAdminModelFactory.PrepareLoadPluginModesAsync(searchModel.AvailableLoadModes, false);

            //prepare available groups
            await BaseAdminModelFactory.PreparePluginGroupsAsync(searchModel.AvailableGroups);

            //prepare page parameters
            searchModel.SetGridPageSize();

            searchModel.NeedToRestart = PluginService.IsRestartRequired();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged plugin list model
        /// </summary>
        /// <param name="searchModel">Plugin search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the plugin list model
        /// </returns>
        public virtual async Task<PluginListModel> PreparePluginListModelAsync(PluginSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter plugins
            var group = string.IsNullOrEmpty(searchModel.SearchGroup) || searchModel.SearchGroup.Equals("0") ? null : searchModel.SearchGroup;
            var loadMode = (LoadPluginsMode)searchModel.SearchLoadModeId;
            var friendlyName = string.IsNullOrEmpty(searchModel.SearchFriendlyName) ? null : searchModel.SearchFriendlyName;
            var author = string.IsNullOrEmpty(searchModel.SearchAuthor) ? null : searchModel.SearchAuthor;

            //filter visible plugins
            var plugins = (await PluginService.GetPluginDescriptorsAsync<IPlugin>(group: group, loadMode: loadMode, friendlyName: friendlyName, author: author))
                .Where(p => p.ShowInPluginsList)
                .OrderBy(plugin => plugin.Group).ToList()
                .ToPagedList(searchModel);

            //prepare list model
            var model = await new PluginListModel().PrepareToGridAsync(searchModel, plugins, () =>
            {
                return plugins.SelectAwait(async pluginDescriptor =>
                {
                    //fill in model values from the entity
                    var pluginModel = pluginDescriptor.ToPluginModel<PluginModel>();

                    //fill in additional values (not existing in the entity)
                    pluginModel.LogoUrl = await PluginService.GetPluginLogoUrlAsync(pluginDescriptor);

                    if (pluginDescriptor.Installed)
                        PrepareInstalledPluginModel(pluginModel, pluginDescriptor.Instance<IPlugin>());

                    return pluginModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare plugin model
        /// </summary>
        /// <param name="model">Plugin model</param>
        /// <param name="pluginDescriptor">Plugin descriptor</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the plugin model
        /// </returns>
        public virtual async Task<PluginModel> PreparePluginModelAsync(PluginModel model, PluginDescriptor pluginDescriptor, bool excludeProperties = false)
        {
            Action<PluginLocalizedModel, int> localizedModelConfiguration = null;

            if (pluginDescriptor != null)
            {
                //fill in model values from the entity
                model ??= pluginDescriptor.ToPluginModel(model);

                model.LogoUrl = await PluginService.GetPluginLogoUrlAsync(pluginDescriptor);
                model.SelectedStoreIds = pluginDescriptor.LimitedToStores;
                model.SelectedCustomerRoleIds = pluginDescriptor.LimitedToCustomerRoles;
                var plugin = pluginDescriptor.Instance<IPlugin>();
                if (pluginDescriptor.Installed)
                    PrepareInstalledPluginModel(model, plugin);

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.FriendlyName = await LocalizationService.GetLocalizedFriendlyNameAsync(plugin, languageId, false);
                };
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await LocalizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            //prepare model customer roles
            await AclSupportedModelFactory.PrepareModelCustomerRolesAsync(model);

            //prepare available stores
            await StoreMappingSupportedModelFactory.PrepareModelStoresAsync(model);

            return model;
        }

        /// <summary>
        /// Prepare search model of plugins of the official feed
        /// </summary>
        /// <param name="searchModel">Search model of plugins of the official feed</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the search model of plugins of the official feed
        /// </returns>
        public virtual async Task<OfficialFeedPluginSearchModel> PrepareOfficialFeedPluginSearchModelAsync(OfficialFeedPluginSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available versions
            var pluginVersions = await OfficialFeedManager.GetVersionsAsync();
            searchModel.AvailableVersions.Add(new SelectListItem { Text = await LocalizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });
            foreach (var version in pluginVersions)
                searchModel.AvailableVersions.Add(new SelectListItem { Text = version.Name, Value = version.Id.ToString() });

            //pre-select current version
            //current version name and named on official site do not match. that's why we use "Contains"
            var currentVersionItem = searchModel.AvailableVersions.FirstOrDefault(x => x.Text.Contains(NopVersion.CURRENT_VERSION));
            if (currentVersionItem != null)
            {
                searchModel.SearchVersionId = int.Parse(currentVersionItem.Value);
                currentVersionItem.Selected = true;
            }

            //prepare available plugin categories
            var pluginCategories = await OfficialFeedManager.GetCategoriesAsync();
            searchModel.AvailableCategories.Add(new SelectListItem { Text = await LocalizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });
            foreach (var pluginCategory in pluginCategories)
            {
                var pluginCategoryNames = new List<string>();
                var tmpCategory = pluginCategory;
                while (tmpCategory != null)
                {
                    pluginCategoryNames.Add(tmpCategory.Name);
                    tmpCategory = pluginCategories.FirstOrDefault(category => category.Id == tmpCategory.ParentCategoryId);
                }

                pluginCategoryNames.Reverse();

                searchModel.AvailableCategories.Add(new SelectListItem
                {
                    Value = pluginCategory.Id.ToString(),
                    Text = string.Join(" >> ", pluginCategoryNames)
                });
            }

            //prepare available prices
            searchModel.AvailablePrices.Add(new SelectListItem
            {
                Value = "0",
                Text = await LocalizationService.GetResourceAsync("Admin.Common.All")
            });
            searchModel.AvailablePrices.Add(new SelectListItem
            {
                Value = "10",
                Text = await LocalizationService.GetResourceAsync("Admin.Configuration.Plugins.OfficialFeed.Price.Free")
            });
            searchModel.AvailablePrices.Add(new SelectListItem
            {
                Value = "20",
                Text = await LocalizationService.GetResourceAsync("Admin.Configuration.Plugins.OfficialFeed.Price.Commercial")
            });

            //prepare page parameters
            searchModel.SetGridPageSize(15, "15");

            return searchModel;
        }

        /// <summary>
        /// Prepare paged list model of plugins of the official feed
        /// </summary>
        /// <param name="searchModel">Search model of plugins of the official feed</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list model of plugins of the official feed
        /// </returns>
        public virtual async Task<OfficialFeedPluginListModel> PrepareOfficialFeedPluginListModelAsync(OfficialFeedPluginSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get plugins
            var plugins = await OfficialFeedManager.GetAllPluginsAsync(categoryId: searchModel.SearchCategoryId,
                versionId: searchModel.SearchVersionId,
                price: searchModel.SearchPriceId,
                searchTerm: searchModel.SearchName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new OfficialFeedPluginListModel().PrepareToGrid(searchModel, plugins, () =>
            {
                //fill in model values from the entity
                return plugins?.Select(plugin => new OfficialFeedPluginModel
                {
                    Url = plugin.Url,
                    Name = plugin.Name,
                    CategoryName = plugin.Category,
                    SupportedVersions = plugin.SupportedVersions,
                    PictureUrl = plugin.PictureUrl,
                    Price = plugin.Price
                }) ?? new List<OfficialFeedPluginModel>();
            });

            return model;
        }
        
        /// <summary>
        /// Prepare plugin models for admin navigation
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of models
        /// </returns>
        public virtual async Task<IList<AdminNavigationPluginModel>> PrepareAdminNavigationPluginModelsAsync()
        {
            var customer = await WorkContext.GetCurrentCustomerAsync();
            var cacheKey = StaticCacheManager.PrepareKeyForDefaultCache(NopPluginDefaults.AdminNavigationPluginsCacheKey, customer);
            return await StaticCacheManager.GetAsync(cacheKey, async () =>
            {
                //get installed plugins
                return (await PluginService.GetPluginDescriptorsAsync<IPlugin>(LoadPluginsMode.InstalledOnly, customer))
                    .Where(plugin => plugin.ShowInPluginsList)
                    .Select(plugin => new AdminNavigationPluginModel
                    {
                        FriendlyName = plugin.FriendlyName,
                        ConfigurationUrl = plugin.Instance<IPlugin>().GetConfigurationPageUrl()
                    }).Where(model => !string.IsNullOrEmpty(model.ConfigurationUrl)).ToList();
            });
        }

        #endregion
    }
}