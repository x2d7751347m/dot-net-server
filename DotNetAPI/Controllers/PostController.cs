using System.Data;
using Dapper;
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
                
        return _dapper.LoadDataSingle<Post, dynamic>(sql, new { PostId = postId });
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
                
        return _dapper.LoadData<Post, dynamic>(sql, new { UserId = userId });
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
                
        return _dapper.LoadData<Post, dynamic>(sql, new { UserId = this.User.FindFirst("userId")?.Value });
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
                
        return _dapper.LoadData<Post, dynamic>(sql, new { SearchParam = $"%{searchParam}%" });
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
        
        DynamicParameters sqlParameters = new DynamicParameters();

        sqlParameters.Add("@UserId", User.FindFirst("userId")?.Value, DbType.String);
        sqlParameters.Add("@PostTitle", postToAdd.PostTitle, DbType.String);
        sqlParameters.Add("@PostContent", postToAdd.PostContent, DbType.String);
        
        if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
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
        
        DynamicParameters sqlParameters = new DynamicParameters();

        sqlParameters.Add("@PostContent", postToEdit.PostContent, DbType.String);
        sqlParameters.Add("@PostTitle", postToEdit.PostTitle, DbType.String);
        sqlParameters.Add("@PostId", postToEdit.PostId, DbType.String);
        sqlParameters.Add("@UserId", User.FindFirst("userId")?.Value, DbType.String);

        if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
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
        
        DynamicParameters sqlParameters = new DynamicParameters();

        sqlParameters.Add("@PostId", postId, DbType.String);
        sqlParameters.Add("@UserId", User.FindFirst("userId")?.Value, DbType.String);
            
        if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
        {
            return Ok();
        }

        throw new Exception("Failed to delete post!");
    }
}