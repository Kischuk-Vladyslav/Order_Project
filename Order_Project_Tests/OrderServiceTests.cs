using Moq;
using Order_Project.Services;
using Order_Project.Services.Intefraces;
using Order_Project.Models;
namespace Order_Project_Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IInventoryService> _inventoryMock;
        private readonly Mock<IPaymentService> _paymentMock;
        private readonly Mock<INotificationService> _notificationMock;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _inventoryMock = new Mock<IInventoryService>();
            _paymentMock = new Mock<IPaymentService>();
            _notificationMock = new Mock<INotificationService>();

            _service = new OrderService(_inventoryMock.Object, _paymentMock.Object, _notificationMock.Object);
        }

        /// <summary>
        /// Tests whether ValidateInput() method throws exception when product is null or empty
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void CreateOrder_ProductIsNullOrEmpty_ThrowsArgumentException(string str)
        {
            // Arrange
            var service = _service;

            // Act
            void action() => service.CreateOrder(str, 1);

            // Assert
            Assert.Throws<ArgumentException>(action);
        }

        /// <summary>
        /// Tests whether ValidateInput() method throws exception when quantity is not positive
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CreateOrder_QuantityIsNotPositive_ThrowsArgumentException(int number)
        {
            // Arrange
            var service = _service;

            // Act
            void action() => service.CreateOrder("Product", number);

            // Assert
            Assert.Throws<ArgumentException>(action);
        }

        /// <summary>
        /// Tests whether Order object c=reated sucessfully after valid input
        /// </summary>
        [Fact]
        public void CreateOrder_ValidInput_ReturnsOrder()
        {
            // Arrange
            _inventoryMock.Setup(i => i.CheckStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);

            // Act
            var order = _service.CreateOrder("Product", 1);

            // Assert
            Assert.Equal("Product", order.Product);
            Assert.Equal(1, order.Quantity);
        }

        /// <summary>
        /// Tests if CreateOrder() throws InvalidOperationException if not enough stock
        /// </summary>
        [Fact]
        public void CreateOrder_NotEnoughStock_ThrowsInvalidOperationException()
        {
            // Arrange
            _inventoryMock.Setup(i => i.CheckStock(It.IsAny<string>(), It.IsAny<int>())).Returns(false);

            // Act
            void action() => _service.CreateOrder("Product", 1);

            // Assert
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void CreateOrder_PaymentFails_ThrowsInvalidOperationException()
        {
            // Arrange
            _inventoryMock.Setup(i => i.CheckStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(false);

            // Act
            void action() => _service.CreateOrder("Product", 1);

            // Assert
            Assert.Throws<InvalidOperationException>(action);
        }


    }
}