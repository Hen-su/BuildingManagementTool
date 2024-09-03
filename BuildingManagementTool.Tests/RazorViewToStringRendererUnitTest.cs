using BuildingManagementTool.Services;
using BuildingManagementTool.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class RazorViewToStringRendererUnitTest
    {
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IRazorViewEngine> _mockViewEngine;
        private Mock<ITempDataProvider> _mockTempDataProvider;
        private RazorViewToStringRenderer _renderer;

        [SetUp]
        public void SetUp()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockServiceProvider = new Mock<IServiceProvider>();    
            _mockViewEngine = new Mock<IRazorViewEngine>();
            _mockTempDataProvider = new Mock<ITempDataProvider>();
            _renderer = new RazorViewToStringRenderer(_mockServiceProvider.Object, _mockHttpContextAccessor.Object, _mockViewEngine.Object, _mockTempDataProvider.Object);
        }

        [Test]
        public async Task RenderViewToStringAsync_ValidView_ReturnString()
        {
            var expectedHtml = "<div>Test</div>";
            var mockView = new Mock<IView>();
            var viewModel = new EmailViewModel
            {
                Username = "test",
                EmailLink = "testurl"
            };
            var mockHttpContext = new Mock<HttpContext>();

            var viewResult = ViewEngineResult.Found("MockView", mockView.Object);

            _mockViewEngine.Setup(v => v.FindView(It.IsAny<ActionContext>(), "TestView", It.IsAny<bool>()))
                           .Returns(viewResult);

            mockView.Setup(v => v.RenderAsync(It.IsAny<ViewContext>()))
                    .Callback<ViewContext>(vc =>
                    {
                        vc.Writer.Write(expectedHtml);
                    })
                    .Returns(Task.CompletedTask);

            // Act
            var result = await _renderer.RenderViewToStringAsync("TestView", viewModel, mockHttpContext.Object);
            Assert.That(result.Equals(expectedHtml));
        }

        [Test]
        public async Task RenderViewToStringAsync_NullView_ReturnString()
        {
            var mockView = new Mock<IView>();
            var viewModel = new EmailViewModel
            {
                Username = "test",
                EmailLink = "testurl"
            };
            var mockHttpContext = new Mock<HttpContext>();

            var viewEngineResult = ViewEngineResult.NotFound("TestView", Enumerable.Empty<string>());

            _mockViewEngine.Setup(v => v.FindView(It.IsAny<ActionContext>(), "TestView", It.IsAny<bool>()))
                           .Returns(viewEngineResult);

            //var result = await _renderer.RenderViewToStringAsync("TestView", viewModel, mockHttpContext.Object);
            var exception = Assert.ThrowsAsync<Exception>(async () =>
                await _renderer.RenderViewToStringAsync("TestView", viewModel, mockHttpContext.Object));
            Assert.That(exception.Message.Equals("View 'TestView' not found."));
        }
    }
}
