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
Imports Common
Imports CsUtilities
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
        Dim folderPath As String = "c:\temp\"
        Dim logPath = String.Format(folderPath + "ConsoleLog-{0}-{1}.log", Me.CommandLine.Username, Now.ToString("yyyy-MM-dd-HH-mm-ss"))
        Dim consoleLog As New CsUtilities.ConsoleCopy(logPath)
        Dim blogs() As CookComputing.Blogger.BlogInfo = Server.getUsersBlogs(String.Empty, Me.CommandLine.Username, Me.CommandLine.Password)
        Dim b As CookComputing.Blogger.BlogInfo
        For Each b In blogs

            Console.WriteLine("The URL of '{0}' is {1}", b.blogName, b.url)
            Console.WriteLine(" Creating BlogML")
            Dim xblog As BlogMLBlog = New BlogMLBlog()
            xblog.Title = b.blogName
            xblog.RootUrl = b.url
            Dim author = New BlogMLAuthor With {.Title = Me.CommandLine.Username}
            xblog.Authors.Add(author)
            Console.WriteLine(" Processing categories")
            Dim categories = From c In Server.getCategories(b.blogName, Me.CommandLine.Username, Me.CommandLine.Password) Select New BlogMLCategory() With {.ID = c.categoryid, .Title = c.title, .Description = c.description, .Approved = True}
            Console.WriteLine(" Adding {0} categories", categories.Count)
            xblog.Categories.AddRange(categories)
            Console.WriteLine(" Processing posts")
            Dim posts = From p In Server.getRecentPosts(b.blogName, Me.CommandLine.Username, Me.CommandLine.Password, 1000) Select BuildPost(b, p, xblog.Categories)
            Console.WriteLine(" Adding {0} posts", posts.Count)
            xblog.Posts.AddRange(posts)

            Dim siteAuthentication = New SiteAuthentication() With {.Username = Me.CommandLine.Username, .Password = Me.CommandLine.Password}
            Console.WriteLine(" Load Articles")
            LoadArticles(b, xblog, siteAuthentication, folderPath)

            Console.WriteLine(" Processing BlogML")
            Dim path As String = String.Format(folderPath + "Blog-{0}-{1}-{2}.xml", Me.CommandLine.Username, b.blogid, Now.ToString("yyyy-MM-dd-HH-mm-ss"))
            Console.WriteLine(" Writing BlogML")
            Using writer As StreamWriter = File.CreateText(path)
                BlogMLSerializer.Serialize(writer, xblog)
            End Using
            Console.WriteLine(" Done")
        Next
        Console.WriteLine("Done")
        consoleLog.Dispose()
        Return 0
    End Function

    Public Function BuildPost(b As BlogInfo, p As CookComputing.MetaWeblog.Post, c As CategoryCollection) As BlogMLPost
        Console.WriteLine("  Processing Post: {0} created {1:yyyy-MM-dd}", p.title, p.dateCreated)
        Try

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
            If p.categories IsNot Nothing Then
                Dim catrefs = (From cname In p.categories Select New BlogMLCategoryReference With {.Ref = cname}).ToList
                Console.WriteLine("  Adding {0} categories ", catrefs.Count) ' to {1}, p.title)
                post.Categories.AddRange(catrefs)
            End If
            ' Add comments
            Try
                Dim blogUrl = b.url.TrimEnd("Default.aspx")
                Using Xml As XmlReader = XmlReader.Create(String.Format(blogUrl + "Comments/commentRss/{0}.aspx", p.postid))
                    Dim sf As SyndicationFeed = SyndicationFeed.Load(Xml)
                    Dim items = (From item In sf.Items _
                                 Select New BlogMLComment With { _
                                     .ID = item.Id, _
                                     .Content = New BlogMLContent() With {.Text = item.Summary.Text}, _
                                     .DateCreated = item.PublishDate.DateTime, _
                                     .DateModified = item.LastUpdatedTime.DateTime, _
                                    .UserName = (From creator In item.ElementExtensions.ReadElementExtensions(Of String)("creator", "http://purl.org/dc/elements/1.1/")).SingleOrDefault _
                                 }).ToList
                    If items.Count > 0 Then
                        Console.WriteLine("  Adding {0} comments to post {1}", items.Count, p.postid)
                        post.Comments.AddRange(items)
                    End If
                End Using
            Catch ex As Exception
                'TODO Find how get comments
                Console.WriteLine("Tried to create comments for {0} : {1} ", p.title, ex.ToString)
            End Try
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
                    If imgImages.Count > 0 Then
                        Console.WriteLine("  Adding {0} attachments from images", imgImages.Count)
                        post.Attachments.AddRange(imgImages.ToList)
                    End If
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
                    If aImages.Count > 0 Then
                        Console.WriteLine("  Adding {0} attachments from links", aImages.Count)
                        post.Attachments.AddRange(aImages)
                    End If
                Catch ex As Exception
                    Console.WriteLine(ex.ToString)
                End Try
            End If



            ' get images with 
            'http://markitup.com/Posts/Post.aspx?postId=38b0afe9-1cf0-4ddb-ab9c-59bc7f9e5b5a


            Return post
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
            Return Nothing
        End Try

    End Function

    Private Function GetMimeType(fileName As String) As String
        Dim mimeType As String = "application/unknown"
        Try
            If fileName.StartsWith("javascript") Then
                Return "application/javascript"
            End If
            Dim ext As String = System.IO.Path.GetExtension(fileName).ToLower()
            Dim regKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext)
            If Not regKey Is Nothing AndAlso Not regKey.GetValue("Content Type") Is Nothing Then
                mimeType = regKey.GetValue("Content Type").ToString()
            End If

        Catch ex As Exception
            Console.WriteLine(" GetMimeType of " + fileName + " caused " + ex.ToString)
        End Try
        Return mimeType
    End Function

    Protected Function LoadArticles(b As CookComputing.Blogger.BlogInfo, xblog As BlogMLBlog, siteAuthentication As SiteAuthentication, outputFolderPath As String) As Integer
        Console.WriteLine(" Processing categories- find them with the list of relevant articles")
        Dim blogUrl = b.url.TrimEnd("/Default.aspx")
        Dim categoriesWithLinks = ArticlesFromAdmin.LoadCategories(blogUrl, siteAuthentication, outputFolderPath)
        Dim categories = From c In categoriesWithLinks Select c.MlCategory
        Console.WriteLine(" Adding {0} categories", categoriesWithLinks.Count)
        xblog.Categories.AddRange(categories)
        Console.WriteLine(" Processing articles")
        Dim posts = ArticlesFromAdmin.GetAllArticleLinks(blogUrl, siteAuthentication, categoriesWithLinks, outputFolderPath)
        'From p In .'Server.getRecentPosts(b.blogName, Me.CommandLine.Username, Me.CommandLine.Password, 1000) Select BuildPost(b, p, xblog.Categories)
        Console.WriteLine(" Adding {0} posts", posts.Count)
        xblog.Posts.AddRange(posts)


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
