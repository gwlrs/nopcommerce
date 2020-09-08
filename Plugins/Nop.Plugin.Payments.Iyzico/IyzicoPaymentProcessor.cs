﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Armut.Iyzipay.Model;
using Armut.Iyzipay.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.Iyzico.Controllers;
using Nop.Plugin.Payments.Iyzico.Models;
using Nop.Plugin.Payments.Iyzico.Services;
using Nop.Plugin.Payments.Iyzico.Validators;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Shipping;

namespace Nop.Plugin.Payments.Iyzico
{
    public class IyzicoPaymentProcessor : BasePlugin, IPaymentMethod
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentIyzicoService _iyzicoService;
        private readonly IyzicoPaymentSettings _iyzicoPaymentSettings;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPaymentService _paymentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IWorkContext _workContext;
        private readonly IShippingPluginManager _shippingPluginManager;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public IyzicoPaymentProcessor(ISettingService settingService, IPaymentIyzicoService iyzicoService, IyzicoPaymentSettings iyzicoPaymentSettings, IWebHelper webHelper, ILocalizationService localizationService, ICustomerService customerService, IShoppingCartService shoppingCartService, IPriceCalculationService priceCalculationService, IPaymentService paymentService, IHttpContextAccessor httpContextAccessor, IOrderTotalCalculationService orderTotalCalculationService, IWorkContext workContext, IShippingPluginManager shippingPluginManager, IStoreContext storeContext, IProductService productservice, ICategoryService categoryService)
        {
            this._settingService = settingService;
            this._iyzicoService = iyzicoService;
            this._iyzicoPaymentSettings = iyzicoPaymentSettings;
            this._webHelper = webHelper;
            this._localizationService = localizationService;
            this._customerService = customerService;
            this._shoppingCartService = shoppingCartService;
            this._priceCalculationService = priceCalculationService;
            this._paymentService = paymentService;
            this._httpContextAccessor = httpContextAccessor;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._workContext = workContext;
            this._shippingPluginManager = shippingPluginManager;
            this._storeContext = storeContext;
            this._productService = productservice;
            this._categoryService = categoryService;
        }

        public override void Install()
        {
            //settings
            var settings = new IyzicoPaymentSettings
            {
                ApiKey = "",
                SecretKey = "",
                BaseUrl = "",
                PaymentMethodDescription = ""
            };
            _settingService.SaveSetting(settings);

            //locales
            #region locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.ApiKey", "Iyzico API Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.ApiKey.Hint", "Enter Iyzico API Key.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.ApiKey.Required", "API Key Is Required.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.SecretKey", "Iyzico Secret Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.SecretKey.Hint", "Enter Iyzico Secret Key.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.SecretKey.Required", "Secret Key Is Required.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.BaseUrl", "Iyzico Base URL");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.BaseUrl.Hint", "Enter Iyzico Base URL.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.BaseUrl.Required", "Base URL Is Required.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.PaymentMethodDescription", "Pay by credit / debit card using Iyzico payment service");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.CardHolderName", "Card Holder Name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.CardNumber", "Card Number");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ExpirationDate", "Expiration Date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.CardCode", "Card Code");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment", "Installment");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.EmptyInstalment", "Empty Instalment");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Admin.IyzicoMessage.MessageCode", "Message Code");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Admin.IyzicoMessage.Message", "Message");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Admin.IyzicoMessage.Deleted", "Deleted");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installments", "Installments", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.InstallmentRequired", "Installment Required", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.InstallmentCount", "Installment Count", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.MonthlyPayment", "Monthly Payment", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Total", "Total", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment1", "Single Payment", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment2", "2 Installments", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment3", "3 Installments", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment6", "6 Installments", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment9", "9 Installments", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment12", "12 Installments", "en-US");
            #endregion

            //locales-on-turkish
            #region locales-on-turkish
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.CardHolderName", "Kart Sahibi", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.CardNumber", "Kart Numarası", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ExpirationDate", "Geçerlilik Tarihi", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.CardCode", "Güvenlik Kodu", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installments", "Taksit Seçenekleri", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.EmptyInstalment", "Taksit seçeneklerini görüntülemek için kart bilgilerinizi giriniz.", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.InstallmentRequired", "Taksit Gerekli", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.InstallmentNumber", "Taksit", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Price", "Taksit Tutarı", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.TotalPrice", "Toplam Tutar", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment1", "Tek Çekim", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment2", "2 Taksit", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment3", "3 Taksit", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment6", "6 Taksit", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment9", "9 Taksit", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment12", "12 Taksit", "tr-TR");
            #endregion

            //locales-error-messages-on-turkish
            #region locales-error-messages-on-turkish
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.8", "IdentityNumber gönderilmesi zorunludur!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.12", "Kart numarası geçersizdir!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.15", "Cvc geçersizdir!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.17", "ExpireYear ve expireMonth geçersizdir!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.5152", "Test kredi kartları ile ödeme yapılamaz!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10005", "İşlem onaylanmadı!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10012", "Geçersiz işlem!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10051", "Kart limiti yetersiz, yetersiz bakiye!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10054", "Son kullanma tarihi hatalı!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10057", "Kart sahibi bu işlemi yapamaz!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10093", "Kartınız internetten alışverişe kapalıdır. Açtırmak için ONAY yazıp kart son 6 haneyi 3340’a gönderebilirsiniz.", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10215", "Geçersiz kart numarası!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10226", "İzin verilen PIN giriş sayısı aşılmış!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Iyzico.Installment.Wrong", "Geçersiz taksit!", "tr-TR");
            #endregion

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<IyzicoPaymentSettings>();

            //locales
            #region locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.ApiKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.ApiKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.ApiKey.Required");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.SecretKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.SecretKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.SecretKey.Required");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.BaseUrl");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.BaseUrl.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.BaseUrl.Required");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Admin.Fields.PaymentMethodDescription");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.CardHolderName");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.CardNumber");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ExpirationDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.CardCode");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Installment");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.EmptyInstalment");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Admin.IyzicoMessage.MessageCode");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Admin.IyzicoMessage.Message");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Admin.IyzicoMessage.Deleted");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Installments");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.InstallmentRequired");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.InstallmentNumber");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Price");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.TotalPrice");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Installment1");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Installment2");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Installment3");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Installment6");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Installment9");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Installment12");
            #endregion

            //locales-error-messages-on-turkish
            #region locales-error-messages-on-turkish
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.8");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.12");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.15");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.17");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.5152");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10005");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10012");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10051");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10054");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10057");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10093");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10215");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.ErrorMessage.10226");

            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Iyzico.Installment.Wrong");
            #endregion
            base.Uninstall();
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            try
            {
                var options = IyzicoHelper.GetIyzicoOptions(_iyzicoPaymentSettings);
                var paymentCard = new PaymentCard()
                {
                    CardHolderName = processPaymentRequest.CreditCardName,
                    CardNumber = processPaymentRequest.CreditCardNumber,
                    ExpireMonth = processPaymentRequest.CreditCardExpireMonth.ToString(),
                    ExpireYear = processPaymentRequest.CreditCardExpireYear.ToString(),
                    Cvc = processPaymentRequest.CreditCardCvv2
                };

                var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
                var billingAddress = _iyzicoService.PrepareAddress(_customerService.GetCustomerBillingAddress(customer));
                var shippingAddress = _customerService.GetCustomerShippingAddress(customer) != null ? _iyzicoService.PrepareAddress(_customerService.GetCustomerShippingAddress(customer)) : billingAddress;

                var installment = GetInstallment(processPaymentRequest, paymentCard, options);

                var shoppingCart = _shoppingCartService.GetShoppingCart(customer, ShoppingCartType.ShoppingCart);
                var shoppingCartTotal = _orderTotalCalculationService.GetShoppingCartTotal(shoppingCart, out var orderDiscountAmount, out var orderAppliedDiscounts, out var appliedGiftCards, out var redeemedRewardPoints, out var redeemedRewardPointsAmount, usePaymentMethodAdditionalFee: false);

                var paymentRequest = new CreatePaymentRequest
                {
                    Price = _priceCalculationService.RoundPrice(shoppingCartTotal ?? 0).ToString("f8", CultureInfo.InvariantCulture),
                    PaidPrice = installment.TotalPrice,
                    Currency = Currency.TRY.ToString(),
                    Installment = installment.InstallmentNumber,
                    BasketId = processPaymentRequest.OrderGuid.ToString(),
                    PaymentCard = paymentCard,
                    Buyer = _iyzicoService.PrepareBuyer(processPaymentRequest.CustomerId),
                    ShippingAddress = shippingAddress,
                    BillingAddress = billingAddress,
                    BasketItems = GetItems(customer, processPaymentRequest.StoreId)
                };
                paymentRequest.PaymentGroup = PaymentGroup.LISTING.ToString();

                var payment = Payment.Create(paymentRequest, options);
                if (payment.Status != "success")
                {
                    string errorMessage = _localizationService.GetResource(String.Format("Plugins.Payments.Iyzico.ErrorMessage.{0}", payment.ErrorCode)) ?? payment.ErrorMessage;
                    result.AddError(errorMessage);
                    return result;
                }

                result.NewPaymentStatus = PaymentStatus.Pending;

                return result;
            }
            catch (Exception ex)
            {
                result.AddError(ex.Message);
                return result;
            }
        }

        /// <summary>
        /// Get installment
        /// </summary>
        /// <param name="processPaymentRequest">Process Payment Request</param>
        /// <param name="paymentCard">Payment Card</param>
        /// <param name="options">Iyzipay Options</param>
        /// <returns>installment</returns>
        private Installment GetInstallment(ProcessPaymentRequest processPaymentRequest, PaymentCard paymentCard, Armut.Iyzipay.Options options)
        {
            int.TryParse((string)processPaymentRequest.CustomValues.GetValueOrDefault(_localizationService.GetResource("Plugins.Payments.Iyzico.Installment")), out int formInstallment);

            var retrieveInstallmentInfoRequest = new RetrieveInstallmentInfoRequest()
            {
                BinNumber = paymentCard.CardNumber.ToString(),
                Locale = Locale.TR.ToString(),
                Price = _priceCalculationService.RoundPrice(processPaymentRequest.OrderTotal).ToString("f8", CultureInfo.InvariantCulture),
                ConversationId = string.Empty
            };
            var installmentInfo = InstallmentInfo.Retrieve(retrieveInstallmentInfoRequest, options);

            var installment = new Installment()
            {
                InstallmentNumber = 1,
                TotalPrice = _priceCalculationService.RoundPrice(processPaymentRequest.OrderTotal).ToString("f8", CultureInfo.InvariantCulture)
            };
            if (installmentInfo.Status == "success" && installmentInfo.InstallmentDetails.Count > 0)
            {
                var installmentDetail = installmentInfo.InstallmentDetails.FirstOrDefault().InstallmentPrices.FirstOrDefault(x => x.InstallmentNumber == formInstallment);

                installment.InstallmentNumber = installmentDetail.InstallmentNumber ?? 1;
                installment.TotalPrice = installmentDetail.TotalPrice.Replace(".", ",");
            }

            return installment;
        }

        /// <summary>
        /// Get transaction line items
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>List of transaction items</returns>
        private List<BasketItem> GetItems(Core.Domain.Customers.Customer customer, int storeId)
        {
            var items = new List<BasketItem>();

            //get current shopping cart            
            var shoppingCart = _shoppingCartService.GetShoppingCart(customer, ShoppingCartType.ShoppingCart, storeId);

            //define function to create item
            BasketItem createItem(decimal price, string productId, string productName, string categoryName, BasketItemType itemType = BasketItemType.PHYSICAL)
            {
                return new BasketItem
                {
                    Id = productId,
                    Name = productName,
                    Category1 = categoryName,
                    ItemType = itemType.ToString(),
                    Price = Convert.ToDecimal(price, CultureInfo.InvariantCulture).ToString("f8", CultureInfo.InvariantCulture),
                };
            }

            items.AddRange(shoppingCart.Where(shoppingCartItem => shoppingCartItem.ProductId != 0).Select(shoppingCartItem =>
            {
                var product = _productService.GetProductById(shoppingCartItem.ProductId);

                var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id).FirstOrDefault();

                var category = _categoryService.GetCategoryById(productCategories.CategoryId);

                return createItem(product.Price * shoppingCartItem.Quantity,
                    product.Id.ToString(),
                    product.Name,
                    category.Name);
            }));

            //LoadAllShippingRateComputationMethods
            var shippingRateComputationMethods = _shippingPluginManager.LoadActivePlugins(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id);
            //shipping without tax
            decimal taxRate = 0;
            var shoppingCartShipping = _orderTotalCalculationService.GetShoppingCartShippingTotal(shoppingCart, false, out taxRate /*shippingRateComputationMethods*/);
            if (shoppingCartShipping.HasValue)
            {
                items.Add(createItem(shoppingCartShipping ?? 0,
                    Guid.NewGuid().ToString(),
                    "Shipping",
                    "Shipping",
                    BasketItemType.VIRTUAL));
            }

            return items;
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var processPaymentRequest = _httpContextAccessor.HttpContext?.Session?.Get<ProcessPaymentRequest>("OrderPaymentInfo");

            if (processPaymentRequest != null)
            {
                var customer = _customerService.GetCustomerById(_workContext.CurrentCustomer.Id);

                var shoppingCart = _shoppingCartService.GetShoppingCart(customer, ShoppingCartType.ShoppingCart);

                var shoppingCartTotal = _orderTotalCalculationService.GetShoppingCartTotal(shoppingCart, out var orderDiscountAmount, out var orderAppliedDiscounts, out var appliedGiftCards, out var redeemedRewardPoints, out var redeemedRewardPointsAmount, usePaymentMethodAdditionalFee: false);

                var options = IyzicoHelper.GetIyzicoOptions(_iyzicoPaymentSettings);

                int.TryParse((string)processPaymentRequest.CustomValues.GetValueOrDefault(_localizationService.GetResource("Plugins.Payments.Iyzico.Installment")), out int formInstallment);

                var retrieveInstallmentInfoRequest = new RetrieveInstallmentInfoRequest()
                {
                    BinNumber = processPaymentRequest.CreditCardNumber.Substring(0, 6),
                    Locale = Locale.TR.ToString(),
                    Price = _priceCalculationService.RoundPrice(shoppingCartTotal ?? 0).ToString("f8", CultureInfo.InvariantCulture),
                    ConversationId = string.Empty
                };

                var installmentInfo = InstallmentInfo.Retrieve(retrieveInstallmentInfoRequest, options);

                if (installmentInfo.Status == "success" && installmentInfo.InstallmentDetails.Count > 0)
                {
                    decimal.TryParse(installmentInfo.InstallmentDetails.FirstOrDefault().InstallmentPrices.FirstOrDefault(x => x.InstallmentNumber == formInstallment).TotalPrice.Replace(".", ","), out decimal installmentTotalPrice);

                    var fee = installmentTotalPrice - (shoppingCartTotal ?? 0);

                    return _paymentService.CalculateAdditionalFee(cart, fee, false);
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest) => new CapturePaymentResult();

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest) => new RefundPaymentResult();

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest) => new VoidPaymentResult();

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest) => new ProcessPaymentResult();

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest) => new CancelRecurringPaymentResult();

        public bool CanRePostProcessPayment(Nop.Core.Domain.Orders.Order order) => false;

        public Type GetControllerType() => typeof(PaymentIyzicoController);

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool HidePaymentMethod(IList<Nop.Core.Domain.Orders.ShoppingCartItem> cart) => false;

        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            //validate payment info (custom validation)
            var validationResult = new PaymentInfoValidator(_localizationService).Validate(new PaymentInfoModel
            {
                CardholderName = form[nameof(PaymentInfoModel.CardholderName)],
                CardNumber = form[nameof(PaymentInfoModel.CardNumber)],
                ExpireMonth = form[nameof(PaymentInfoModel.ExpireMonth)],
                ExpireYear = form[nameof(PaymentInfoModel.ExpireYear)],
                CardCode = form[nameof(PaymentInfoModel.CardCode)],
                Installment = form[nameof(PaymentInfoModel.Installment)]
            });
            if (!validationResult.IsValid)
                return validationResult.Errors.Select(error => error.ErrorMessage).ToList();

            return new List<string>();
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var paymentRequest = new ProcessPaymentRequest();

            //pass custom values to payment processor
            //if (_iyzicoPaymentSettings.UseInstallment)
            var installment = form[nameof(PaymentInfoModel.Installment)];
            if (!StringValues.IsNullOrEmpty(installment) && !installment.FirstOrDefault().Equals(Guid.Empty.ToString()))
                paymentRequest.CustomValues.Add(_localizationService.GetResource("Plugins.Payments.Iyzico.Installment"), installment.FirstOrDefault());

            //set card details
            paymentRequest.CreditCardName = form[nameof(PaymentInfoModel.CardholderName)];
            paymentRequest.CreditCardNumber = form[nameof(PaymentInfoModel.CardNumber)];
            paymentRequest.CreditCardExpireMonth = int.Parse(form[nameof(PaymentInfoModel.ExpireMonth)]);
            paymentRequest.CreditCardExpireYear = int.Parse(form[nameof(PaymentInfoModel.ExpireYear)]);
            paymentRequest.CreditCardCvv2 = form[nameof(PaymentInfoModel.CardCode)];

            return paymentRequest;
        }

        public string PaymentMethodDescription => _iyzicoPaymentSettings.PaymentMethodDescription;

        public override string GetConfigurationPageUrl() => $"{_webHelper.GetStoreLocation()}Admin/PaymentIyzico/Configure";

        public string GetPublicViewComponentName() => "PaymentIyzico";
    }
}
