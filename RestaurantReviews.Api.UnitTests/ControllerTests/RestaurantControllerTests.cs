using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantReviews.Api.Controllers;
using RestaurantReviews.Api.DataAccess;
using RestaurantReviews.Api.Models;
using Xunit;

namespace RestaurantReviews.Api.UnitTests.ControllerTests
{
    public class RestaurantControllerTests
    {
        [Fact]
        public async Task GetListWithNoParamsReturnsList()
        {
            var mockRestaurantQuery = new Mock<IRestaurantQuery>();
            mockRestaurantQuery.Setup(q => q.GetRestaurants(null, null))
                .Returns(Task.FromResult(new List<Restaurant> {new Restaurant()}));
            var controller = new RestaurantController(null, mockRestaurantQuery.Object, null);

            var result = await controller.GetListAsync();

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<List<Restaurant>>(((OkObjectResult)result.Result).Value);
            var resultList = (List<Restaurant>)((OkObjectResult) result.Result).Value;
            Assert.Single(resultList);
        }
        
        [Fact]
        public async Task GetListWithNoCityReturnsBadRequest()
        {
            var controller = new RestaurantController(null, null, null);

            var result = await controller.GetListAsync(null, "PA");

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
        
        [Fact]
        public async Task GetListWithNoStateReturnsBadRequest()
        {
            var controller = new RestaurantController(null, null, null);

            var result = await controller.GetListAsync("Pittsburgh");

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetListWithParamsReturnsList()
        {
            var mockRestaurantQuery = new Mock<IRestaurantQuery>();
            mockRestaurantQuery.Setup(q => 
                    q.GetRestaurants(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new List<Restaurant> {TestData.McDonalds }));
            var controller = new RestaurantController(null, mockRestaurantQuery.Object, null);

            var result = await controller.GetListAsync("Pittsburgh", "PA");

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<List<Restaurant>>(((OkObjectResult)result.Result).Value);
            var resultList = (List<Restaurant>)((OkObjectResult) result.Result).Value;
            Assert.Single(resultList);
        }

        [Fact]
        public async Task GetSingleWithValidIdReturnsRestaurant()
        {
            var mockRestaurantQuery = new Mock<IRestaurantQuery>();
            mockRestaurantQuery.Setup(q => q.GetRestaurant(TestData.McDonalds.Id))
                .Returns(Task.FromResult(TestData.McDonalds));
            var controller = new RestaurantController(null, mockRestaurantQuery.Object, null);

            var result = await controller.GetAsync(TestData.McDonalds.Id);

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<Restaurant>(((OkObjectResult)result.Result).Value);
            var restaurant = (Restaurant)((OkObjectResult) result.Result).Value;
            Assert.Equal(TestData.McDonalds.Name, restaurant.Name);
        }
        
        [Fact]
        public async Task GetSingleWithNonexistentIdReturnsNotFound()
        {
            var mockRestaurantQuery = new Mock<IRestaurantQuery>();
            mockRestaurantQuery.Setup(q => q.GetRestaurant(TestData.McDonalds.Id))
                .Returns(Task.FromResult(TestData.McDonalds));
            var controller = new RestaurantController(null, mockRestaurantQuery.Object, null);

            var result = await controller.GetAsync(TestData.Wendys.Id);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetSingleWithInvalidIdReturnsBadRequest()
        {
            var controller = new RestaurantController(null, null, null);

            var result = await controller.GetAsync(0);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
        
        [Fact]
        public async Task PostNewRestaurantThatDoesNotExistReturnsCreated()
        {
            var mockRestaurantValidator = new Mock<IRestaurantValidator>();
            mockRestaurantValidator.Setup(v => 
                v.IsRestaurantValid(It.IsAny<NewRestaurant>())).Returns(true);
            var mockRestaurantQuery = new Mock<IRestaurantQuery>();
            mockRestaurantQuery.Setup(q => q.GetRestaurant(TestData.McDonalds.Name, TestData.McDonalds.City, TestData.McDonalds.State))
                .Returns(Task.FromResult(TestData.McDonalds));
            var mockInsertRestaurant = new Mock<IInsertRestaurant>();
            var controller = new RestaurantController(mockRestaurantValidator.Object,
                mockRestaurantQuery.Object, 
                mockInsertRestaurant.Object);

            var result = await controller.PostAsync(TestData.Wendys);

            Assert.IsType<CreatedResult>(result.Result);
            mockInsertRestaurant.Verify(i => i.Insert(It.IsAny<NewRestaurant>()),
                Times.Once);
        }
        
        [Fact]
        public async Task PostNewRestaurantThatIsInvalidReturnsBadRequest()
        {
            var mockRestaurantValidator = new Mock<IRestaurantValidator>();
            mockRestaurantValidator.Setup(v => 
                v.IsRestaurantValid(It.IsAny<NewRestaurant>())).Returns(false);
            var controller = new RestaurantController(mockRestaurantValidator.Object,
                null, null);

            var result = await controller.PostAsync(TestData.Wendys);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task PostNewRestaurantThatExistsReturnsConflict()
        {
            var mockRestaurantValidator = new Mock<IRestaurantValidator>();
            mockRestaurantValidator.Setup(v => 
                v.IsRestaurantValid(It.IsAny<NewRestaurant>())).Returns(true);
            var mockRestaurantQuery = new Mock<IRestaurantQuery>();
            mockRestaurantQuery.Setup(q => q.GetRestaurant(TestData.McDonalds.Name, TestData.McDonalds.City, TestData.McDonalds.State))
                .Returns(Task.FromResult(TestData.McDonalds));
            var mockInsertRestaurant = new Mock<IInsertRestaurant>();
            var controller = new RestaurantController(mockRestaurantValidator.Object,
                mockRestaurantQuery.Object, 
                mockInsertRestaurant.Object);

            var result = await controller.PostAsync(TestData.McDonalds);

            Assert.IsType<ConflictObjectResult>(result.Result);
        }
    }
}