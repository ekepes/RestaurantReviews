using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RestaurantReviews.Api.DataAccess;
using RestaurantReviews.Api.Models;

namespace RestaurantReviews.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewValidator _reviewValidator;
        private readonly IReviewQuery _reviewQuery;
        private readonly IInsertReview _insertReview;
        private readonly IDeleteReview _deleteReview;

        public ReviewController(IReviewValidator reviewValidator, 
            IReviewQuery reviewQuery, 
            IInsertReview insertReview, 
            IDeleteReview deleteReview)
        {
            _reviewValidator = reviewValidator;
            _reviewQuery = reviewQuery;
            _insertReview = insertReview;
            _deleteReview = deleteReview;
        }
        
        /// <summary>
        /// Returns the review at the given internal identifier.
        /// </summary>
        /// <param name="id">The internal identifier of the review.</param>
        /// <returns>The Review</returns>
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Review>> GetAsync(long id)
        {
            if (id <= 0)
            {
                return BadRequest("id must be greater than 0");
            }
            
            var review = await _reviewQuery.GetReview(id);
            if (review == null)
            {
                return NotFound();
            }

            return Ok(review);
        }
        
        /// <summary>
        /// Returns all of the reviews for a particular user.
        /// </summary>
        /// <param name="reviewerEmail">The email address of the user who's reviews are requested.</param>
        /// <returns>A collection of Reviews.</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<List<Review>>> GetListAsync(string reviewerEmail)
        {
            if (string.IsNullOrWhiteSpace(reviewerEmail))
            {
                return BadRequest("ReviewerEmail is required.");
            }
            
            return Ok(await _reviewQuery.GetReviews(reviewerEmail));
        }

        /// <summary>
        /// Adds a review for a restaurant.
        /// </summary>
        /// <param name="review">A review to add to the system.</param>
        /// <returns>The restaurant, with the internal Id added.</returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<bool>> PostAsync(NewReview review)
        {
            if (!_reviewValidator.IsReviewValid(review))
            {
                return BadRequest("Reviews require a valid restaurant, reviewer email, and a rating between 0 and 5.");
            }
            
            var id = await _insertReview.Insert(review);

            return Created(nameof(GetAsync), id != 0);
        }

        
        [HttpDelete]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<bool>> DeleteAsync(long reviewId)
        {
            if (reviewId <= 0)
            {
                return BadRequest("ReviewId is required.");
            }

            var rowsAffected = await _deleteReview.Delete(reviewId);
            if (rowsAffected != 1)
            {
                return NotFound(false);
            }

            return Ok(true);
        }
    }
}