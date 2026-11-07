using Moq;
using Order_Project.Services;
using Order_Project.Services.Intefraces;

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
    }
}