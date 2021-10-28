using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Vendors;
using Nop.Services.Localization;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the vendor attribute model factory implementation
    /// </summary>
    public partial class VendorAttributeModelFactory : IVendorAttributeModelFactory
    {
        #region Fields

        protected ILocalizationService LocalizationService { get; }
        protected ILocalizedModelFactory LocalizedModelFactory { get; }
        protected IVendorAttributeService VendorAttributeService { get; }

        #endregion

        #region Ctor

        public VendorAttributeModelFactory(ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IVendorAttributeService vendorAttributeService)
        {
            LocalizationService = localizationService;
            LocalizedModelFactory = localizedModelFactory;
            VendorAttributeService = vendorAttributeService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare vendor attribute value search model
        /// </summary>
        /// <param name="searchModel">Vendor attribute value search model</param>
        /// <param name="vendorAttribute">Vendor attribute</param>
        /// <returns>Vendor attribute value search model</returns>
        protected virtual VendorAttributeValueSearchModel PrepareVendorAttributeValueSearchModel(VendorAttributeValueSearchModel searchModel,
            VendorAttribute vendorAttribute)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (vendorAttribute == null)
                throw new ArgumentNullException(nameof(vendorAttribute));

            searchModel.VendorAttributeId = vendorAttribute.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare vendor attribute search model
        /// </summary>
        /// <param name="searchModel">Vendor attribute search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor attribute search model
        /// </returns>
        public virtual Task<VendorAttributeSearchModel> PrepareVendorAttributeSearchModelAsync(VendorAttributeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare paged vendor attribute list model
        /// </summary>
        /// <param name="searchModel">Vendor attribute search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor attribute list model
        /// </returns>
        public virtual async Task<VendorAttributeListModel> PrepareVendorAttributeListModelAsync(VendorAttributeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get vendor attributes
            var vendorAttributes = (await VendorAttributeService.GetAllVendorAttributesAsync()).ToPagedList(searchModel);

            //prepare list model
            var model = await new VendorAttributeListModel().PrepareToGridAsync(searchModel, vendorAttributes, () =>
            {
                return vendorAttributes.SelectAwait(async attribute =>
                {
                    //fill in model values from the entity
                    var attributeModel = attribute.ToModel<VendorAttributeModel>();

                    //fill in additional values (not existing in the entity)
                    attributeModel.AttributeControlTypeName = await LocalizationService.GetLocalizedEnumAsync(attribute.AttributeControlType);

                    return attributeModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare vendor attribute model
        /// </summary>
        /// <param name="model">Vendor attribute model</param>
        /// <param name="vendorAttribute">Vendor attribute</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor attribute model
        /// </returns>
        public virtual async Task<VendorAttributeModel> PrepareVendorAttributeModelAsync(VendorAttributeModel model,
            VendorAttribute vendorAttribute, bool excludeProperties = false)
        {
            Action<VendorAttributeLocalizedModel, int> localizedModelConfiguration = null;

            if (vendorAttribute != null)
            {
                //fill in model values from the entity
                model ??= vendorAttribute.ToModel<VendorAttributeModel>();

                //prepare nested search model
                PrepareVendorAttributeValueSearchModel(model.VendorAttributeValueSearchModel, vendorAttribute);

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await LocalizationService.GetLocalizedAsync(vendorAttribute, entity => entity.Name, languageId, false, false);
                };
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await LocalizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            return model;
        }

        /// <summary>
        /// Prepare paged vendor attribute value list model
        /// </summary>
        /// <param name="searchModel">Vendor attribute value search model</param>
        /// <param name="vendorAttribute">Vendor attribute</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor attribute value list model
        /// </returns>
        public virtual async Task<VendorAttributeValueListModel> PrepareVendorAttributeValueListModelAsync(VendorAttributeValueSearchModel searchModel,
            VendorAttribute vendorAttribute)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (vendorAttribute == null)
                throw new ArgumentNullException(nameof(vendorAttribute));

            //get vendor attribute values
            var vendorAttributeValues = (await VendorAttributeService.GetVendorAttributeValuesAsync(vendorAttribute.Id)).ToPagedList(searchModel);

            //prepare list model
            var model = new VendorAttributeValueListModel().PrepareToGrid(searchModel, vendorAttributeValues, () =>
            {
                //fill in model values from the entity
                return vendorAttributeValues.Select(value => value.ToModel<VendorAttributeValueModel>());
            });

            return model;
        }

        /// <summary>
        /// Prepare vendor attribute value model
        /// </summary>
        /// <param name="model">Vendor attribute value model</param>
        /// <param name="vendorAttribute">Vendor attribute</param>
        /// <param name="vendorAttributeValue">Vendor attribute value</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor attribute value model
        /// </returns>
        public virtual async Task<VendorAttributeValueModel> PrepareVendorAttributeValueModelAsync(VendorAttributeValueModel model,
            VendorAttribute vendorAttribute, VendorAttributeValue vendorAttributeValue, bool excludeProperties = false)
        {
            if (vendorAttribute == null)
                throw new ArgumentNullException(nameof(vendorAttribute));

            Action<VendorAttributeValueLocalizedModel, int> localizedModelConfiguration = null;

            if (vendorAttributeValue != null)
            {
                //fill in model values from the entity
                model ??= vendorAttributeValue.ToModel<VendorAttributeValueModel>();

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await LocalizationService.GetLocalizedAsync(vendorAttributeValue, entity => entity.Name, languageId, false, false);
                };
            }

            model.VendorAttributeId = vendorAttribute.Id;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await LocalizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            return model;
        }

        #endregion
    }
}