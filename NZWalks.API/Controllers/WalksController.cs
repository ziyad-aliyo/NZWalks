using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;
using System.Globalization;

namespace NZWalks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalksController : ControllerBase
    {
        private readonly NZWalksDbContext dbContext;

        public WalksController(NZWalksDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost]

        public async Task<IActionResult> Create(AddWalkRequestDto addWalkRequestDto)
        {

            var walkDomainModel = new Walk
            {
                Name = addWalkRequestDto.Name,
                Description = addWalkRequestDto.Description,
                LengthInKm = addWalkRequestDto.LengthInKm,
                WalkImageUrl = addWalkRequestDto.WalkImageUrl,
                DifficultyId = addWalkRequestDto.DifficultyId,
                RegionId = addWalkRequestDto.RegionId

            };

            await dbContext.Walks.AddAsync(walkDomainModel);
            await dbContext.SaveChangesAsync();



            walkDomainModel = await dbContext.Walks
                .Include("Difficulty")
                .Include("Region")
                .FirstOrDefaultAsync(x => x.Id == walkDomainModel.Id);

            var walkDto = new WalkDto
            {

                Id = walkDomainModel.Id,
                Name = walkDomainModel.Name,
                Description = walkDomainModel.Description,
                LengthInKm = walkDomainModel.LengthInKm,
                WalkImageUrl = walkDomainModel.WalkImageUrl,
                //DifficultyId = walkDomainModel.DifficultyId,
                //RegionId = walkDomainModel.RegionId

                Difficulty = new DifficultyDto
                {
                    Id = walkDomainModel.Difficulty.Id,
                    Name = walkDomainModel.Difficulty.Name,
                },

                Region = new RegionDto
                {
                    Id = walkDomainModel.Region.Id,
                    Code = walkDomainModel.Region.Code,
                    Name = walkDomainModel.Region.Name,
                    RegionImageUrl = walkDomainModel.Region.RegionImageUrl

                }


            };

            return CreatedAtAction(nameof(GetById), new { id = walkDto.Id }, walkDto);


        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
                                                [FromQuery] string? filterOn,
                                                [FromQuery] string? filterQuery,
                                                [FromQuery] string? sortBy,
                                                [FromQuery] bool? isAscending,
                                                [FromQuery] int pageNumber = 1,   
                                                [FromQuery] int pageSize = 10)
        {
            var walksQuery = dbContext.Walks
                .Include("Difficulty")
                .Include("Region")
                .AsQueryable();

            if (string.IsNullOrWhiteSpace(filterOn) == false && string.IsNullOrWhiteSpace(filterQuery) == false)
            {
                if (filterOn.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    walksQuery = walksQuery.Where(x => x.Name.Contains(filterQuery));
                }
            }

            if (string.IsNullOrWhiteSpace(sortBy) == false)
            {
                bool asc = isAscending ?? true;

                if (sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    walksQuery = asc ? walksQuery.OrderBy(x => x.Name) : walksQuery.OrderByDescending(x => x.Name);
                }
                else if (sortBy.Equals("Length", StringComparison.OrdinalIgnoreCase))
                {
                    walksQuery = asc ? walksQuery.OrderBy(x => x.LengthInKm) : walksQuery.OrderByDescending(x => x.LengthInKm);
                }
            }


            var skipResults = (pageNumber - 1) * pageSize;

            walksQuery = walksQuery.Skip(skipResults).Take(pageSize);



            var walkDomainModel = await walksQuery.ToListAsync();

            var walksDto = new List<WalkDto>();
            foreach (var walk in walkDomainModel)
            {
                walksDto.Add(new WalkDto()
                {
                    Id = walk.Id,
                    Name = walk.Name,
                    Description = walk.Description,
                    LengthInKm = walk.LengthInKm,
                    WalkImageUrl = walk.WalkImageUrl,

                    Region = new RegionDto()
                    {
                        Id = walk.Region.Id,
                        Code = walk.Region.Code,
                        Name = walk.Region.Name,
                        RegionImageUrl = walk.Region.RegionImageUrl
                    },

                    Difficulty = new DifficultyDto()
                    {
                        Id = walk.Difficulty.Id,
                        Name = walk.Difficulty.Name,
                    }
                });
            }

            return Ok(walksDto);
        }
        
        [HttpGet]
        [Route("{id:Guid}")]

        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var walkDomainModel = await dbContext.Walks
                .Include("Difficulty")
                .Include("Region")
                .FirstOrDefaultAsync(x => x.Id == id);

            if (walkDomainModel == null)
                return NotFound();

            var walkDto = new WalkDto()
            {
                Id = walkDomainModel.Id,
                Name = walkDomainModel.Name,
                Description = walkDomainModel.Description,
                LengthInKm = walkDomainModel.LengthInKm,
                WalkImageUrl = walkDomainModel.WalkImageUrl,
                //DifficultyId = walkDominModel.DifficultyId,
                //RegionId = walkDominModel.RegionId

                Difficulty = new DifficultyDto
                {
                    Id = walkDomainModel.Difficulty.Id,
                    Name = walkDomainModel.Difficulty.Name,
                },

                Region = new RegionDto
                {
                    Id = walkDomainModel.Region.Id,
                    Code = walkDomainModel.Region.Code,
                    Name = walkDomainModel.Region.Name,
                    RegionImageUrl = walkDomainModel.Region.RegionImageUrl

                }

            };

            return Ok(walkDto);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateWalkRequestDto updateWalkRequestDto)
        {
            var walkDomainModel = await dbContext.Walks.FirstOrDefaultAsync(x => x.Id == id);
            if (walkDomainModel == null)
                return NotFound();

            walkDomainModel.Name = updateWalkRequestDto.Name;
            walkDomainModel.Description = updateWalkRequestDto.Description;
            walkDomainModel.LengthInKm = updateWalkRequestDto.LengthInKm;
            walkDomainModel.WalkImageUrl = updateWalkRequestDto.WalkImageUrl;

            walkDomainModel.DifficultyId = updateWalkRequestDto.DifficultyId;
            walkDomainModel.RegionId = updateWalkRequestDto.RegionId;

            await dbContext.SaveChangesAsync();

            walkDomainModel = await dbContext.Walks
            .Include("Difficulty")
            .Include("Region")
            .FirstOrDefaultAsync(x => x.Id == id);

            var walkDto = new WalkDto()
            {
                Id = walkDomainModel.Id,
                Name = walkDomainModel.Name,
                Description = walkDomainModel.Description,
                LengthInKm = walkDomainModel.LengthInKm,
                WalkImageUrl = walkDomainModel.WalkImageUrl,

                Difficulty = new DifficultyDto()
                {
                    Id = walkDomainModel.Difficulty.Id,
                    Name = walkDomainModel.Difficulty.Name,
                },

                Region = new RegionDto()
                {
                    Id = walkDomainModel.Region.Id,
                    Code = walkDomainModel.Region.Code,
                    Name = walkDomainModel.Region.Name,
                    RegionImageUrl = walkDomainModel.Region.RegionImageUrl,

                }


            };
            return Ok(walkDto);

        }

        [HttpDelete]
        [Route("{id:Guid}")]

        public async Task<IActionResult> Delete(Guid id)
        {
            var walkDomainModel = await dbContext.Walks
            .Include("Difficulty")
            .Include("Region")
            .FirstOrDefaultAsync(x => x.Id == id);

            if (walkDomainModel == null)
                return NotFound();

            dbContext.Walks.Remove(walkDomainModel);
            await dbContext.SaveChangesAsync();


            var walkDto = new WalkDto()
            {
                Id = walkDomainModel.Id,
                Name = walkDomainModel.Name,
                Description = walkDomainModel.Description,
                LengthInKm = walkDomainModel.LengthInKm,
                WalkImageUrl = walkDomainModel.WalkImageUrl,

                Difficulty = new DifficultyDto()
                {
                    Id = walkDomainModel.Difficulty.Id,
                    Name = walkDomainModel.Difficulty.Name,
                },

                Region = new RegionDto()
                {
                    Id = walkDomainModel.Region.Id,
                    Code = walkDomainModel.Region.Code,
                    Name = walkDomainModel.Region.Name,
                    RegionImageUrl = walkDomainModel.Region.RegionImageUrl,

                }
            };
            return Ok(walkDto);
        }
    }
}
