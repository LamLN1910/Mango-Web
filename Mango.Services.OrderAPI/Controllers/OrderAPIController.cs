using AutoMapper;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Mango.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        private ResponseDto _responseDto;
        private IMapper _mapper;
        private readonly AppDbContext _db;
        private readonly IProductService _productService;
        private readonly IConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        public OrderAPIController(IMapper mapper, AppDbContext db, IProductService productService, IMessageBus messageBus, IConfiguration configuration)
        {
            _mapper = mapper;
            _db = db;
            _responseDto = new ResponseDto();
            _productService = productService;
            _messageBus = messageBus;
            _configuration = configuration;
        }

        [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
        {
            try
            {
                OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
                orderHeaderDto.OrderTime = DateTime.Now;
                orderHeaderDto.Status = SD.Status_Pending;
                orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetails);

                OrderHeader orderCreated = _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
                await _db.SaveChangesAsync();
                orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
                _responseDto.Result = orderHeaderDto;
            }
            catch (Exception e)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = e.Message;
            }
            return _responseDto;
        }
    }
}
