﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Companies;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;

namespace Nop.Web.Controllers.Api.Security
{
    [Produces("application/json")]
    [Route("api/account")]
    public class AccountApiController : BaseApiController
    {
        #region Fields
        private readonly IStoreContext _storeContext;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IConfiguration _config;
        private readonly IAddressService _addressService;
        private readonly IEncryptionService _encryptionService;
        private readonly MediaSettings _mediaSettings;
        private readonly IPictureService _pictureService;
        private readonly ICompanyService _companyService;

        #endregion

        #region Ctor

        public AccountApiController(ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            CustomerSettings customerSettings,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IWorkflowMessageService workflowMessageService,
             ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IAuthenticationService authenticationService,
            IShoppingCartService shoppingCartService,
            IConfiguration config,
            IAddressService addressService,
            IEncryptionService encryptionService,
            MediaSettings mediaSettings,
            IPictureService pictureService,
            ICompanyService companyService)
        {
            _storeContext = storeContext;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _customerSettings = customerSettings;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _workflowMessageService = workflowMessageService;
            _workContext = workContext;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _authenticationService = authenticationService;
            _shoppingCartService = shoppingCartService;
            _config = config;
            _addressService = addressService;
            _encryptionService = encryptionService;
            _mediaSettings = mediaSettings;
            _pictureService = pictureService;
            _companyService = companyService;
        }

        #endregion

        #region Methods

        public class LoginApiModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string PushToken { get; set; }
            public bool IsFromGoogle { get; set; }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginApiModel model)
        {
            if (!ModelState.IsValid)
                return Ok(new { success = false, message = GetModelErrors(ModelState) });

            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(model.Email, model.Password);

            //checking if customer comes from goolge
            if (model.IsFromGoogle)
            {
                var customer = await _customerService.GetCustomerByEmailAsync(model.Email);
                if (customer != null)
                    loginResult = CustomerLoginResults.Successful;
            }
            switch (loginResult)
            {
                case CustomerLoginResults.Successful:
                    {
                        var customer = await _customerService.GetCustomerByEmailAsync(model.Email);
                        if (customer == null)
                            return Ok(new { success = false, message = await _localizationService.GetResourceAsync("Customer.Not.Found") });

                        customer.PushToken = model.PushToken;
                        await _customerService.UpdateCustomerAsync(customer);

                        await _workContext.SetCurrentCustomerAsync(customer);

                        //migrate shopping cart
                        await _shoppingCartService.MigrateShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), customer, true);

                        //sign in new customer
                        await _authenticationService.SignInAsync(customer, false);

                        var jwt = new JwtService(_config);
                        var token = jwt.GenerateSecurityToken(customer.Email, customer.Id);

                        var shippingAddress = customer.ShippingAddressId.HasValue ? await _addressService.GetAddressByIdAsync(customer.ShippingAddressId.Value) : null;

                        var firstName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                        var lastName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute);

                        return Ok(new
                        {
                            success = true,
                            message = await _localizationService.GetResourceAsync("Customer.Login.Successfully"),
                            token,
                            pushToken = customer.PushToken,
                            shippingAddress,
                            firstName,
                            lastName,
                            RemindMeNotification = customer.RemindMeNotification,
                            RateReminderNotification = customer.RateReminderNotification,
                            OrderStatusNotification = customer.OrderStatusNotification,
                            avatar = await _pictureService.GetPictureUrlAsync(await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute), _mediaSettings.AvatarPictureSize, true)
                        });
                    }
                case CustomerLoginResults.CustomerNotExist:
                    return Ok(new { success = false, message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist") });
                case CustomerLoginResults.Deleted:
                    return Ok(new { success = false, message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted") });
                case CustomerLoginResults.NotActive:
                    return Ok(new { success = false, message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive") });
                case CustomerLoginResults.NotRegistered:
                    return Ok(new { success = false, message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered") });
                case CustomerLoginResults.LockedOut:
                    return Ok(new { success = false, message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut") });
                case CustomerLoginResults.WrongPassword:
                default:
                    return Ok(new { success = false, message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials") });
            }
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return Ok(new { success = false, message = await _localizationService.GetResourceAsync("Customer.Not.Found") });

            //customer.PushToken = null;
            //await _customerService.UpdateCustomerAsync(customer);

            //standard logout 
            await _authenticationService.SignOutAsync();

            return Ok(new { success = true, message = await _localizationService.GetResourceAsync("Customer.Logout.Successfully") });
        }

        [AllowAnonymous]
        [HttpGet("check-customer-token")]
        public async Task<IActionResult> CheckCustomerToken()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return Ok(new { success = false, message = await _localizationService.GetResourceAsync("Customer.Not.Found") });

            var jwt = new JwtService(_config);
            var token = jwt.GenerateSecurityToken(customer.Email, customer.Id);
            var shippingAddress = customer.ShippingAddressId.HasValue ? await _addressService.GetAddressByIdAsync(customer.ShippingAddressId.Value) : null;
            var firstName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute);
            var lastName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute);

            return Ok(new
            {
                success = true,
                token,
                pushToken = customer.PushToken,
                shippingAddress,
                firstName,
                lastName,
                RemindMeNotification = customer.RemindMeNotification,
                RateReminderNotification = customer.RateReminderNotification,
                OrderStatusNotification = customer.OrderStatusNotification,
                avatar = await _pictureService.GetPictureUrlAsync(await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute), _mediaSettings.AvatarPictureSize, true)
            });
        }

        #endregion
    }
}
