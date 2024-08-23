using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    public PostController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("Posts")]
    public IEnumerable<Post> GetPosts()
    {
        string sql = @"SELECT PostId,
                    UserId,
                    PostTitle,
                    PostContent,
                    PostCreated,
                    PostUpdated 
                FROM Posts";
                
        return _dapper.LoadData<Post>(sql);
    }

    [HttpGet("PostSingle/{postId}")]
    public Post GetPostSingle(int postId)
    {
        string sql = @"SELECT PostId,
                    UserId,
                    PostTitle,
                    PostContent,
                    PostCreated,
                    PostUpdated 
                FROM Posts
                    WHERE PostId = @PostId";
                
        return _dapper.LoadDataSingle<Post>(sql, new { PostId = postId });
    }

    [HttpGet("PostsByUser/{userId}")]
    public IEnumerable<Post> GetPostsByUser(int userId)
    {
        string sql = @"SELECT PostId,
                    UserId,
                    PostTitle,
                    PostContent,
                    PostCreated,
                    PostUpdated 
                FROM Posts
                    WHERE UserId = @UserId";
                
        return _dapper.LoadData<Post>(sql, new { UserId = userId });
    }

    [HttpGet("MyPosts")]
    public IEnumerable<Post> GetMyPosts()
    {
        string sql = @"SELECT PostId,
                    UserId,
                    PostTitle,
                    PostContent,
                    PostCreated,
                    PostUpdated 
                FROM Posts
                    WHERE UserId = @UserId";
                
        return _dapper.LoadData<Post>(sql, new { UserId = this.User.FindFirst("userId")?.Value });
    }

    [HttpGet("PostsBySearch/{searchParam}")]
    public IEnumerable<Post> PostsBySearch(string searchParam)
    {
        string sql = @"SELECT PostId,
                    UserId,
                    PostTitle,
                    PostContent,
                    PostCreated,
                    PostUpdated 
                FROM Posts
                    WHERE PostTitle LIKE @SearchParam
                        OR PostContent LIKE @SearchParam";
                
        return _dapper.LoadData<Post>(sql, new { SearchParam = $"%{searchParam}%" });
    }

    [HttpPost("Post")]
    public IActionResult AddPost(PostToAddDto postToAdd)
    {
        string sql = @"
            INSERT INTO Posts(
                UserId,
                PostTitle,
                PostContent,
                PostCreated,
                PostUpdated) VALUES (
                @UserId,
                @PostTitle,
                @PostContent,
                NOW(),
                NOW())";

        var parameters = new 
        {
            UserId = this.User.FindFirst("userId")?.Value,
            postToAdd.PostTitle,
            postToAdd.PostContent
        };

        if (_dapper.ExecuteSqlWithParameters(sql, parameters))
        {
            return Ok();
        }

        throw new Exception("Failed to create new post!");
    }

    [HttpPut("Post")]
    public IActionResult EditPost(PostToEditDto postToEdit)
    {
        string sql = @"
            UPDATE Posts 
                SET PostContent = @PostContent,
                    PostTitle = @PostTitle,
                    PostUpdated = NOW()
                WHERE PostId = @PostId
                AND UserId = @UserId";

        var parameters = new 
        {
            postToEdit.PostContent,
            postToEdit.PostTitle,
            postToEdit.PostId,
            UserId = this.User.FindFirst("userId")?.Value
        };

        if (_dapper.ExecuteSqlWithParameters(sql, parameters))
        {
            return Ok();
        }

        throw new Exception("Failed to edit post!");
    }

    [HttpDelete("Post/{postId}")]
    public IActionResult DeletePost(int postId)
    {
        string sql = @"DELETE FROM Posts 
                WHERE PostId = @PostId
                AND UserId = @UserId";

        var parameters = new 
        {
            PostId = postId,
            UserId = this.User.FindFirst("userId")?.Value
        };
            
        if (_dapper.ExecuteSqlWithParameters(sql, parameters))
        {
            return Ok();
        }

        throw new Exception("Failed to delete post!");
    }
}