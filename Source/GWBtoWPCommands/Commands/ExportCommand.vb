Imports Hinshlabs.CommandLineParser
Imports MetaWebLogEdit.MetaWeblogApi
Imports System.Net
Imports System.Xml
Imports BlogML.Xml
Imports CookComputing.MetaWeblog
Imports CookComputing.Blogger
Imports BlogML.Xml.BlogMLBlog
Imports BlogML
Imports System.ServiceModel.Syndication
Imports System.IO
Imports HtmlAgilityPack

Public Class ExportCommand
    Inherits CommandBase(Of AuthenticatedBlog)

    Public Sub New(parent As CommandCollection)
        MyBase.New(parent)
    End Sub

    Private _Server As GwbMetaWeblog

    Friend ReadOnly Property Server As GwbMetaWeblog
        Get
            If _Server Is Nothing Then
                _Server = New GwbMetaWeblog
                _Server.Credentials = New NetworkCredential(Me.CommandLine.Username, Me.CommandLine.Password)
            End If
            Return _Server
        End Get
    End Property

    Public Overrides ReadOnly Property Description As String
        Get
            Return "Exports all blog posts"
        End Get
    End Property

    Public Overrides ReadOnly Property Name As String
        Get
            Return "export"
        End Get
    End Property

    Public Overrides ReadOnly Property Qualifications As String
        Get
            Return ""
        End Get
    End Property

    Protected Overrides Function RunCommand() As Integer
        Dim blogs() As CookComputing.Blogger.BlogInfo = Server.getUsersBlogs(String.Empty, Me.CommandLine.Username, Me.CommandLine.Password)
        Dim b As CookComputing.Blogger.BlogInfo
        For Each b In blogs

            Console.WriteLine("The URL of '{0}' is {1}", b.blogName, b.url)
            Console.WriteLine(" Creating BlogML")
            Dim xblog As BlogMLBlog = New BlogMLBlog()
            xblog.Title = b.blogName
            xblog.RootUrl = b.url
            Console.WriteLine(" Processing categories")
            Dim categories = From c In Server.getCategories(b.blogName, Me.CommandLine.Username, Me.CommandLine.Password) Select New BlogMLCategory() With {.ID = c.categoryid, .Title = c.title, .Description = c.description, .Approved = True}
            Console.WriteLine(" Adding {0} categories", categories.Count)
            xblog.Categories.AddRange(categories)
            Console.WriteLine(" Processing posts")
            Dim posts = From p In Server.getRecentPosts(b.blogName, Me.CommandLine.Username, Me.CommandLine.Password, 1000) Select BuildPost(b, p, xblog.Categories)
            Console.WriteLine(" Adding {0} posts", posts.Count)
            xblog.Posts.AddRange(posts)
            Console.WriteLine(" Processing BlogML")
            Dim path As String = String.Format("c:\temp\{0}-{1}.xml", b.blogid, Now.ToString("yyyy-MM-dd-HH-mm-ss"))
            Console.WriteLine(" Writing BlogML")
            Using writer As StreamWriter = File.CreateText(path)
                BlogMLSerializer.Serialize(writer, xblog)
            End Using
            Console.WriteLine(" Done")
        Next
        Console.WriteLine("Done")
        Return 0
    End Function

    Public Function BuildPost(b As BlogInfo, p As CookComputing.MetaWeblog.Post, c As CategoryCollection) As BlogMLPost
        Console.WriteLine("  Processing Post: {0}", p.title)
        Dim post As New BlogMLPost() With {
                .PostType = BlogPostTypes.Normal, _
                .Title = p.title, _
                .DateCreated = p.dateCreated, _
                .DateModified = DateTime.Today, _
                .ID = p.postid.ToString(), _
                .PostUrl = p.permalink, _
                .Content = New BlogMLContent() With {.Text = p.description}
            }
        ' Add categories
        Dim catrefs = (From cname In p.categories Select New BlogMLCategoryReference With {.Ref = cname}).ToList
        Console.WriteLine("  Adding {0} categories to {1}", catrefs.Count, p.title)
        post.Categories.AddRange(catrefs)
        ' Add comments

        Using Xml As XmlReader = XmlReader.Create(String.Format("http://blog.hinshelwood.com/comments/commentRss/{0}.aspx", p.postid))
            Try
                Dim sf As SyndicationFeed = SyndicationFeed.Load(Xml)
                Dim items = (From item In sf.Items _
                             Select New BlogMLComment With { _
                                 .Content = New BlogMLContent() With {.Text = item.Summary.Text}, _
                                 .DateCreated = item.PublishDate.DateTime, _
                                 .DateModified = item.LastUpdatedTime.DateTime, _
                                .UserName = (From creator In item.ElementExtensions.ReadElementExtensions(Of String)("creator", "http://purl.org/dc/elements/1.1/")).SingleOrDefault _
                             }).ToList
                Console.WriteLine("  Adding {0} comments to {1}", items.Count, p.title)
                post.Comments.AddRange(items)
            Catch ex As Exception
                Console.WriteLine(ex.ToString)
            End Try
            
        End Using
        ' Add Attachements
        Dim HD As New HtmlDocument()
        HD.LoadHtml(p.description)
        ' get images from img
        Dim images = HD.DocumentNode.SelectNodes("//img")
        If Not images Is Nothing Then
            Try
                Dim imgImages = (From x In images, a In x.Attributes _
                                 Where a.Name = "src" _
                                 And Not String.IsNullOrEmpty(a.Value) _
                                 AndAlso a.Value.StartsWith("http://geekswithblogs.net/images/") _
                                 Select New BlogMLAttachment With {.Embedded = False, .Url = a.Value})
                Console.WriteLine("  Adding {0} attachments from images", imgImages.Count)
                post.Attachments.AddRange(imgImages.ToList)
            Catch ex As Exception
                Console.WriteLine(ex.ToString)
            End Try
        End If
        
        ' Get Images from a
        Dim linkImages = HD.DocumentNode.SelectNodes("//a")
        If Not linkImages Is Nothing Then
            Try
                Dim aImages = (From x In linkImages, a In x.Attributes _
                                 Where a.Name = "href" _
                                 And GetMimeType(a.Value).Contains("image") _
                                 Select New BlogMLAttachment With {.Embedded = False, .Url = a.Value}).ToList
                Console.WriteLine("  Adding {0} attachments from links", aImages.Count)
                post.Attachments.AddRange(aImages)
            Catch ex As Exception
                Console.WriteLine(ex.ToString)
            End Try
        End If
       
      

        ' get images with 
        'http://markitup.com/Posts/Post.aspx?postId=38b0afe9-1cf0-4ddb-ab9c-59bc7f9e5b5a


        Return post
    End Function

    Private Function GetMimeType(fileName As String) As String
        Dim mimeType As String = "application/unknown"
        Try

            Dim ext As String = System.IO.Path.GetExtension(fileName).ToLower()
            Dim regKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext)
            If Not regKey Is Nothing AndAlso Not regKey.GetValue("Content Type") Is Nothing Then
                mimeType = regKey.GetValue("Content Type").ToString()
            End If

        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
        Return mimeType
    End Function



    Public Overrides ReadOnly Property Switches As System.Collections.ObjectModel.ReadOnlyCollection(Of Hinshlabs.CommandLineParser.SwitchInfo)
        Get
            Return Me.CommandLine.Switches
        End Get
    End Property

    Public Overrides ReadOnly Property Synopsis As String
        Get
            Return "Exports all blog posts and images"
        End Get
    End Property

    Public Overrides ReadOnly Property Title As String
        Get
            Return "export"
        End Get
    End Property

    Protected Overrides Function ValidateCommand() As Boolean
        Return True
    End Function

End Class
