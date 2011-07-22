Imports System
Imports System.Net
Imports CookComputing.XmlRpc
Imports CookComputing.MetaWeblog


Namespace MetaWeblogApi



    '/ <summary> 
    '/ This class can be used to programmatically interact with a Weblog on 
    '/ MSN Spaces using the MetaWeblog API. 
    '/ </summary> 
    '<XmlRpcUrl("http://geekswithblogs.net/hinshelm/services/metablogapi.aspx")> _ 
    'TODO: You need to put your own URL in here (Not perfect, fix if you can)
    '<XmlRpcUrl("http://blog.hinshelwood.com/xmlrpc.php")> _
    Class GwbMetaWeblog
        Inherits XmlRpcClientProtocol

        '/ <summary> 
        '/ Returns the most recent draft and non-draft blog posts sorted in descending order by publish date. 
        '/ </summary> 
        '/ <param name="blogid">This should be the string MyBlog, which indicates that the post is being created in the user’s blog.</param> 
        '/ <param name="username">The name of the user’s space.</param> 
        '/ <param name="password">The user’s secret word.</param> 
        '/ <param name="numberOfPosts">The number of posts to return. The maximum value is 20.</param> 
        '/ <returns></returns> 
        <XmlRpcMethod("metaWeblog.getRecentPosts")> _
        Public Function getRecentPosts(ByVal blogid As String, ByVal username As String, ByVal password As String, ByVal numberOfPosts As Integer) As CookComputing.MetaWeblog.Post()

            Return CType(Me.Invoke("getRecentPosts", New Object() {blogid, username, password, numberOfPosts}), CookComputing.MetaWeblog.Post())
        End Function 'getRecentPosts


        '/ <summary> 
        '/ Posts a new entry to a blog. 
        '/ </summary> 
        '/ <param name="blogid">This should be the string MyBlog, which indicates that the post is being created in the user’s blog.</param> 
        '/ <param name="username">The name of the user’s space.</param> 
        '/ <param name="password">The user’s secret word.</param> 
        '/ <param name="post">A struct representing the content to update. </param> 
        '/ <param name="publish">If false, this is a draft post.</param> 
        '/ <returns>The postid of the newly-created post.</returns> 
        <XmlRpcMethod("metaWeblog.newPost")> _
        Public Function newPost(ByVal blogid As String, ByVal username As String, ByVal password As String, ByVal content As CookComputing.MetaWeblog.Post, ByVal publish As Boolean) As String

            Return CStr(Me.Invoke("newPost", New Object() {blogid, username, password, content, publish}))
        End Function 'newPost

        '/ <summary> 
        '/ Edits an existing entry on a blog. 
        '/ </summary> 
        '/ <param name="postid">The ID of the post to update.</param> 
        '/ <param name="username">The name of the user’s space.</param> 
        '/ <param name="password">The user’s secret word.</param> 
        '/ <param name="post">A struct representing the content to update. </param> 
        '/ <param name="publish">If false, this is a draft post.</param> 
        '/ <returns>Always returns true.</returns> 
        <XmlRpcMethod("metaWeblog.editPost")> _
        Public Function editPost(ByVal postid As String, ByVal username As String, ByVal password As String, ByVal content As CookComputing.MetaWeblog.Post, ByVal publish As Boolean) As Boolean

            Return CBool(Me.Invoke("editPost", New Object() {postid, username, password, content, publish}))
        End Function 'editPost

        '/ <summary> 
        '/ Deletes a post from the blog. 
        '/ </summary> 
        '/ <param name="appKey">This value is ignored.</param> 
        '/ <param name="postid">The ID of the post to update.</param> 
        '/ <param name="username">The name of the user’s space.</param> 
        '/ <param name="password">The user’s secret word.</param> 
        '/ <param name="post">A struct representing the content to update. </param> 
        '/ <param name="publish">This value is ignored.</param> 
        '/ <returns>Always returns true.</returns> 
        <XmlRpcMethod("blogger.deletePost")> _
        Public Function deletePost(ByVal appKey As String, ByVal postid As String, ByVal username As String, ByVal password As String, ByVal publish As Boolean) As Boolean

            Return CBool(Me.Invoke("deletePost", New Object() {appKey, postid, username, password, publish}))
        End Function 'deletePost


        '/ <summary> 
        '/ Returns information about the user’s space. An empty array is returned if the user does not have a space. 
        '/ </summary> 
        '/ <param name="appKey">This value is ignored.</param> 
        '/ <param name="postid">The ID of the post to update.</param> 
        '/ <param name="username">The name of the user’s space.</param>
        '/ <returns>An array of structs that represents each of the user’s blogs.
        '/ The array will contain a maximum of one struct, since a user can only have a single space with a single blog.</returns> 
        <XmlRpcMethod("blogger.getUsersBlogs")> _
        Public Function getUsersBlogs(ByVal appKey As String, ByVal username As String, ByVal password As String) As CookComputing.Blogger.BlogInfo()

            Return CType(Me.Invoke("getUsersBlogs", New Object() {appKey, username, password}), CookComputing.Blogger.BlogInfo())
        End Function 'getUsersBlogs

        '/ <summary> 
        '/ Returns basic user info (name, e-mail, userid, and so on). 
        '/ </summary> 
        '/ <param name="appKey">This value is ignored.</param> 
        '/ <param name="postid">The ID of the post to update.</param> 
        '/ <param name="username">The name of the user’s space.</param>
        '/ <returns>A struct containing profile information about the user. 
        '/Each struct will contain the following fields: nickname, userid, url, e-mail, 
        '/lastname, and firstname.</returns> 
        <XmlRpcMethod("blogger.getUserInfo")> _
        Public Function getUserInfo(ByVal appKey As String, ByVal username As String, ByVal password As String) As CookComputing.Blogger.UserInfo

            Return CType(Me.Invoke("getUserInfo", New Object() {appKey, username, password}), CookComputing.Blogger.UserInfo)
        End Function 'getUserInfo


        '/ <summary> 
        '/ Returns a specific entry from a blog. 
        '/ </summary> 
        '/ <param name="postid">The ID of the post to update.</param> 
        '/ <param name="username">The name of the user’s space.</param> 
        '/ <param name="password">The user’s secret word.</param> 
        '/ <returns>Always returns true.</returns> 
        <XmlRpcMethod("metaWeblog.getPost")> _
        Public Function getPost(ByVal postid As String, ByVal username As String, ByVal password As String) As CookComputing.MetaWeblog.Post

            Return CType(Me.Invoke("getPost", New Object() {postid, username, password}), CookComputing.MetaWeblog.Post)
        End Function 'getPost

        '/ <summary> 
        '/ Returns the list of categories that have been used in the blog. 
        '/ </summary> 
        '/ <param name="blogid">This should be the string MyBlog, which indicates that the post is being created in the user’s blog.</param> 
        '/ <param name="username">The name of the user’s space.</param> 
        '/ <param name="password">The user’s secret word.</param> 
        '/ <returns>An array of structs that contains one struct for each category.
        '/ Each category struct will contain a description field that contains the name of the category.</returns> 
        <XmlRpcMethod("metaWeblog.getCategories")> _
        Public Function getCategories(ByVal blogid As String, ByVal username As String, ByVal password As String) As CookComputing.MetaWeblog.CategoryInfo()

            Return CType(Me.Invoke("getCategories", New Object() {blogid, username, password}), CookComputing.MetaWeblog.CategoryInfo())
        End Function 'getCategories

        <XmlRpcMethod("mt.getCategoryList")> _
        Public Function getCategoryList(ByVal blogid As String, ByVal username As String, ByVal password As String) As CookComputing.MovableType.Category()
            Return CType(Me.Invoke("getCategoryList", New Object() {blogid, username, password}), CookComputing.MovableType.Category())
        End Function


        <XmlRpcMethod("mt.getPostCategories")> _
        Public Function getPostCategories(ByVal postid As String, ByVal username As String, ByVal password As String) As CookComputing.MovableType.Category()
            Return CType(Me.Invoke("getCategoryList", New Object() {postid, username, password}), CookComputing.MovableType.Category())
        End Function


        <XmlRpcMethod("mt.setPostCategories")> _
        Public Function setPostCategories(ByVal postid As String, ByVal username As String, ByVal password As String, ByVal categories() As CookComputing.MovableType.Category) As Boolean
            Return CType(Me.Invoke("setPostCategories", New Object() {postid, username, password, categories}), Boolean)
        End Function

        <XmlRpcMethod("metaWeblog.newMediaObject")> _
        Public Function newMediaObject(blogid As String, username As String, password As String, file As CookComputing.MetaWeblog.FileData) As CookComputing.MetaWeblog.UrlData
            Return CType(Me.Invoke("newMediaObject", New Object() {blogid, username, password, file}), UrlData)
        End Function
    End Class
End Namespace