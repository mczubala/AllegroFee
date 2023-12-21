using System.Globalization;
using System.Net;
using AutoFixture;
using MFC.Interfaces;
using MFC.Models;
using MFC.Responses;
using MFC.Services;
using Moq;
using Newtonsoft.Json;
using Refit;
using Xunit;

namespace MFC.BusinessLogic.UnitTests;

public class CalculationServiceUnitTests
{
    private readonly Mock<IAllegroApiClient> _mockAllegroApiClient;
    private readonly Mock<IAccessTokenProvider> _mockAccessTokenProvider;
    private readonly Fixture _fixture;

    public CalculationServiceUnitTests()
    {
        _mockAllegroApiClient = new Mock<IAllegroApiClient>();
        _mockAccessTokenProvider = new Mock<IAccessTokenProvider>();
        _fixture = new Fixture();
    }
    
    [Fact]
    public async Task GetCalculatedTotalOfferFeeByIdAsync_ReturnsCorrectData()
    { 
        // Arrange
        var testOfferId = _fixture.Create<string>();
        var fakeAccessToken = _fixture.Create<string>();
        // Create a fake order with a fake offer id
        var fakeOrder = _fixture.Create<Order>();
        // Set the fake offer id to the test offer id
        fakeOrder.LineItems[0].Offer.Id = testOfferId;
        
        var httpResponseMessageOrder = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(fakeOrder))
        };
        // Create a fake api response with the fake order
        var fakeApiOrderResponse = new ApiResponse<Order>(httpResponseMessageOrder, fakeOrder, null,null);
        
        // Create a fake billing entry with the fake order
        _fixture.Customize<BillingEntry>(c => c
            .With(b => b.Order, fakeOrder)
            .With(b => b.Value, 
                new Value {Amount = _fixture.Create<decimal>()})
        );  
        
        var fakeBillingEntries = _fixture.CreateMany<BillingEntry>().ToList();
        // Create a fake billings object with the fake billing entries
        var fakeBillings = new Billings { BillingEntries = fakeBillingEntries };

        var httpResponseMessageBillings = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(fakeBillings))
        };
        var fakeApiBillingsResponse = new ApiResponse<Billings>(httpResponseMessageBillings, fakeBillings, null,null);

        _mockAllegroApiClient
            .Setup(x => x.GetBillingByOfferIdAsync(testOfferId, It.IsAny<string>()))
            .ReturnsAsync(fakeApiBillingsResponse);
        
        _mockAllegroApiClient
            .Setup(x => x.GetOrderByIdAsync(fakeOrder.Id, It.IsAny<string>()))
            .ReturnsAsync(fakeApiOrderResponse);
        
        // Mock the GetAccessForUserTokenAsync method to return the fake access token
        _mockAccessTokenProvider
            .Setup(x => x.GetAccessForUserTokenAsync())
            .ReturnsAsync(fakeAccessToken);
       
        // Mock the GetCalculatedTotalOfferSaleAsync method to return the fake total sale
        var service = new CalculationService(
            _mockAllegroApiClient.Object,
            _mockAccessTokenProvider.Object);
        
        var result = await service.GetCalculatedTotalOfferFeeByIdAsync(testOfferId);
        
        Assert.NotNull(result);
        Assert.Equal(ServiceStatusCodes.StatusCode.Success, result.ResponseStatus);
        Assert.Equal(testOfferId, result.Data.OfferId);
    }
}