using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Model;
using Mango.Services.ShoppingCartAPI.Model.Dto;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private ResponseDto _responseDto;
        private IMapper _mapper;
        private readonly AppDbContext _db;
        private readonly IProductService _productService;
        private readonly ICouponService _couponService;
        public CartAPIController(IMapper mapper, AppDbContext db, IProductService productService, ICouponService couponService)
        {
            _mapper = mapper;
            _db = db;
            _responseDto = new ResponseDto();
            _productService = productService;
            _couponService = couponService;
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCard(string userId)
        {
            try
            {
                CartDto cart = new()
                {
                    CartHeader = _mapper.Map<CartHeaderDto>(_db.CartHeaders.First(u => u.UserId == userId))
                };
                cart.CartDetails = _mapper.Map<List<CartDetailsDto>>(_db.CartDetails
                                                                    .Where(u => u.CartHeaderId == cart.CartHeader.CartHeaderId));
                IEnumerable<ProductDto> productList = await _productService.GetProducts();

                foreach (var item in cart.CartDetails)
                {
                    item.Product = productList.FirstOrDefault(u => u.ProductId == item.ProductId);
                    cart.CartHeader.CartTotal += item.Product.Price * item.Count;
                }

                //apply coupon if any
                if (!string.IsNullOrEmpty(cart.CartHeader.CouponCode))
                {
                    CouponDto coupon = await _couponService.GetCoupon(cart.CartHeader.CouponCode);
                    if (coupon != null && cart.CartHeader.CartTotal > coupon.MinAmount)
                    {
                        cart.CartHeader.CartTotal -= coupon.DiscountAmount;
                        cart.CartHeader.Discount = coupon.DiscountAmount;
                    }
                }

                _responseDto.Result = cart;
            }
            catch (Exception e)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = e.Message.ToString();
            }
            return _responseDto;
        }

        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> CartUpsert(CartDto cartDto)
        {
            try
            {
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking()
                                                            .FirstOrDefaultAsync(u => u.UserId == cartDto.CartHeader.UserId);
                if (cartHeaderFromDb == null)
                {
                    //create header and details
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                    _db.CartHeaders.Add(cartHeader);
                    await _db.SaveChangesAsync();
                    cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                    _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                    await _db.SaveChangesAsync();
                }
                else
                {
                    //if header is not null
                    //check if details has same product
                    var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                        u => u.ProductId == cartDto.CartDetails.First().ProductId &&
                        u.CartHeaderId == cartHeaderFromDb.CartHeaderId);
                    if (cartDetailsFromDb == null)
                    {
                        //create cart details
                        cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                        _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        //update details
                        cartDto.CartDetails.First().Count += cartDetailsFromDb.Count;
                        cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                        cartDto.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                        _db.CartDetails.Update(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();
                    }
                }
                _responseDto.Result = cartDto;
            }
            catch (Exception e)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = e.Message.ToString();
            }
            return _responseDto;
        }

        [HttpPost("RemoveCart")]
        public async Task<object> RemoveCart([FromBody] int cartDetailsId)
        {
            try
            {
                CartDetails cartDetails = _db.CartDetails
                    .First(u => u.CartDetailsId == cartDetailsId);

                int totalCountOfCartItem = _db.CartDetails.Where(u => u.CartHeaderId == cartDetailsId).Count();
                _db.CartDetails.Remove(cartDetails);
                if (totalCountOfCartItem == 1)
                {
                    var cartHeader = await _db.CartHeaders
                        .FirstOrDefaultAsync(u => u.CartHeaderId == cartDetails.CartHeaderId);
                    _db.CartHeaders.Remove(cartHeader);
                }
                await _db.SaveChangesAsync();

                _responseDto.Result = true;
            }
            catch (Exception e)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = e.Message.ToString();
            }
            return _responseDto;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<ResponseDto> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartFromDb = await _db.CartHeaders
                    .FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
                cartFromDb.CouponCode = cartDto.CartHeader.CouponCode;
                _db.CartHeaders.Update(cartFromDb);
                await _db.SaveChangesAsync();
                _responseDto.Result = true;
            }
            catch (Exception e)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = e.Message.ToString();
            }
            return _responseDto;
        }
    }
}
