using Bogus;
using FCG.Contracts;
using Microsoft.Extensions.Logging.Abstractions;
using NotificationsAPI;

namespace NotificationsAPI.UnitTests;

public sealed class NotificationsFixture
{
    public Faker Faker { get; } = new("pt_BR");

    public UserCreatedEvent CreateUser() => new(
        Guid.NewGuid(), Faker.Name.FullName(), Faker.Internet.Email(), DateTime.UtcNow);

    public PaymentProcessedEvent CreatePayment(string status) => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Faker.Commerce.ProductName(),
        decimal.Parse(Faker.Commerce.Price(10, 300)), status, DateTime.UtcNow);
}

public sealed class NotificationServiceTests(NotificationsFixture fixture) : IClassFixture<NotificationsFixture>
{
    private readonly NotificationLogService _service = new(NullLogger<NotificationLogService>.Instance);

    [Fact]
    public void SendWelcome_ReturnsMessageContainingUserEmail()
    {
        var user = fixture.CreateUser();

        var message = _service.SendWelcome(user);

        Assert.Contains(user.Email, message);
    }

    [Fact]
    public void SendPurchaseConfirmation_ReturnsMessageForApprovedPayment()
    {
        var payment = fixture.CreatePayment(PaymentStatuses.Approved);

        var message = _service.SendPurchaseConfirmation(payment);

        Assert.NotNull(message);
        Assert.Contains(payment.GameTitle, message);
    }

    [Fact]
    public void SendPurchaseConfirmation_ReturnsNullForRejectedPayment()
    {
        var payment = fixture.CreatePayment(PaymentStatuses.Rejected);

        Assert.Null(_service.SendPurchaseConfirmation(payment));
    }

    [Fact]
    public void CorrelationId_TrimsInboundValue() =>
        Assert.Equal("notification-flow", CorrelationId.Normalize(" notification-flow "));
}
