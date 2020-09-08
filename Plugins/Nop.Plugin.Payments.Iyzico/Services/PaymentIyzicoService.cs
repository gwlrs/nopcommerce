using System;
using Armut.Iyzipay.Model;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;

namespace Nop.Plugin.Payments.Iyzico.Services
{
    public class PaymentIyzicoService : IPaymentIyzicoService
    {
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IyzicoPaymentSettings _iyzicoPaymentSettings;
        private readonly ICountryService _countryService;


        public PaymentIyzicoService(ICustomerService customerService, IyzicoPaymentSettings iyzicoPaymentSettings, IGenericAttributeService genericAttributeService, ICountryService countryService)
        {
            this._customerService = customerService;
            this._iyzicoPaymentSettings = iyzicoPaymentSettings;
            this._genericAttributeService = genericAttributeService;
            this._countryService = countryService;
        }

        public virtual Buyer PrepareBuyer(int customerId)
        {
            var customer = _customerService.GetCustomerById(customerId);

            var customerName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute);
            var customerSurName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute);
            var customerIdentityNumber = _genericAttributeService.GetAttribute<string>(customer, "IdentityNumber");
            var customerGsmNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute);
            var billingAddress = _customerService.GetCustomerBillingAddress(customer);

            var buyer = new Buyer
            {
                Id = customer.CustomerGuid.ToString(),
                Name = customerName,
                Surname = customerSurName,
                Email = customer.Email,
                IdentityNumber = customerIdentityNumber,
                RegistrationAddress = billingAddress?.Address1 ,
                Ip = customer.LastIpAddress,
                City = billingAddress?.City,
                Country = _countryService.GetCountryByAddress(billingAddress).Name,
                ZipCode = billingAddress.ZipPostalCode,
                GsmNumber = customerGsmNumber,
            };

            return buyer;
        }

        public virtual Address PrepareAddress(Core.Domain.Common.Address address)
        {
            return new Address
            {
                ContactName = String.Format("{0} {1}", address.FirstName, address.LastName),
                City = address.City,
                Country = _countryService.GetCountryByAddress(address).Name,
                Description = address.Address1,
                ZipCode = address.ZipPostalCode
            };
        }
    }
}