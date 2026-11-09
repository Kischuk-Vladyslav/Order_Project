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

        /// <summary>
        /// Checks whether exception is thrown when paymentt fails during order creation
        /// </summary>
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

        /// <summary>
        /// Checks whether SendConfirmation() method is never called if payment is unsuccessful during order creation
        /// </summary>
        [Fact]
        public void CreateOrder_InvalidOrder_DoesntCallNotificationService()
        {
            // Arrange
            _inventoryMock.Setup(i => i.CheckStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(false);

            // Act
            void action() => _service.CreateOrder("Product", 1);

            // Assert
            Assert.Throws<InvalidOperationException>(action);
            _notificationMock.Verify(n => n.SendConfirmation(It.IsAny<Order>()), Times.Never);
        }

        /// <summary>
        /// Checks whether IncreaseStock() method called once if payment is unsuccessful during order creation
        /// </summary>
        [Fact]
        public void CreateOrder_InvalidOrder_СallsIncreaseOrderOnce()
        {
            // Arrange
            _inventoryMock.Setup(i => i.CheckStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(false);

            // Act
            void action() => _service.CreateOrder("Product", 1);

            // Assert
            Assert.Throws<InvalidOperationException>(action);
            _inventoryMock.Verify(i => i.IncreaseStock(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        /// <summary>
        /// Checks whether _orders list is updated correctly after successful order creation
        /// </summary>
        [Fact]
        public void CreateOrder_ValidOrder_UpdatesOrdersList()
        {
            // Arrange
            _inventoryMock.Setup(i => i.CheckStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);

            // Act
            var order = _service.CreateOrder("Product", 1);

            // Assert
            Assert.Contains(order, _service.GetOrders());
        }

        /// <summary>
        /// Checks whether CreateOrder does not return null after order creation
        /// </summary>
        [Fact]
        public void CreateOrder_ValidOrder_DoesntReturnNull()
        {
            // Arrange
            _inventoryMock.Setup(i => i.CheckStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);

            // Act
            var order = _service.CreateOrder("Product", 1);

            // Assert
            Assert.NotNull(order);
        }

        /// <summary>
        /// Checks whether UpdateOrder() method returns false when orderId is not found
        /// </summary>
        [Fact]
        public void UpdateOrder_OrderIdNotFound_ReturnsFalse()
        {
            // Act
            var result = _service.UpdateOrder(999, 2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Checks whether UpdateOrder() method returns false when newQuantity is not positive
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void UpdateOrder_NewQuantityNotPositive_ReturnsFalse(int num)
        {
            // Arrange
            _inventoryMock.Setup(i => i.CheckStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);
            var order = _service.CreateOrder("Product", 1);

            // Act
            var result = _service.UpdateOrder(order.Id, num);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Checks whether UpdateOrder() method returns true and updates the order quantity when valid orderId and newQuantity are provided
        /// </summary>
        [Fact]
        public void UpdateOrder_ValidOrderIdAndNewQuantity_ReturnsTrue()
        {
            // Arrange
            _inventoryMock.Setup(i => i.CheckStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);
            var order = _service.CreateOrder("Product", 1);

            // Act
            var result = _service.UpdateOrder(order.Id, 2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Checks whether UpdateOrder() method correctly updates the order quantity when valid orderId and newQuantity are provided
        /// </summary>
        [Fact]
        public void UpdateOrder_ValidOrderIdAndNewQuantity_UpdatesOrderQuantity()
        {
            // Arrange
            _inventoryMock.Setup(i => i.CheckStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);
            var order = _service.CreateOrder("Product", 1);
            // Act
            _service.UpdateOrder(order.Id, 2);
            // Assert
            var updatedOrder = _service.GetOrders().Find(o => o.Id == order.Id);
            Assert.Equal(2, updatedOrder.Quantity);
        }

        [Fact]
        public void UpdateOrder_ValidData_ChangesQuantity()
        {
            // Arrange
            _inventoryMock.Setup(i => i.CheckStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);

            var order = _service.CreateOrder("Laptop", 1);
            int oldQuantity = order.Quantity;

            // Act
            _service.UpdateOrder(order.Id, 5);

            // Assert
            Assert.NotEqual(oldQuantity, order.Quantity);
        }

        /// <summary>
        /// Checks whether RemoveOrder() method correctly increases inventory stock when an order is removed
        /// </summary>
        [Fact]
        public void RemoveOrder_ValidData_IncreasesInventoryStock()
        {
            // Arrange
            _inventoryMock.Setup(i => i.CheckStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);
            var order = _service.CreateOrder("Product", 1);

            // Act
            _service.RemoveOrder(order.Id);

            // Assert
            _inventoryMock.Verify(i => i.IncreaseStock(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        /// <summary>
        /// Checks whether RemoveOrder() method correctly removes the order from the list when valid data is provided
        /// </summary>
        [Fact]
        public void RemoveOrder_ValidData_RemovesOrderFromList()
        {
            // Arrange
            _inventoryMock.Setup(i => i.CheckStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);
            var order = _service.CreateOrder("Product", 1);

            // Act
            _service.RemoveOrder(order.Id);

            // Assert
            Assert.DoesNotContain(order, _service.GetOrders());
        }

        [Fact]
        public void GetOrders_NoOrders_ReturnsEmptyList()
        {
            // Act
            var result = _service.GetOrders();

            // Assert
            Assert.Empty(result);
        }
    }
}