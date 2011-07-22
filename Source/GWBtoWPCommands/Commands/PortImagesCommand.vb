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
Imports CookComputing

Public Class PortImagesCommand
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
            Return "Downloads all images stored extrnaly to your blog, uploads them and updates links"
        End Get
    End Property

    Public Overrides ReadOnly Property Name As String
        Get
            Return "portimages"
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


            Dim allPosts = From p In Server.getRecentPosts(b.blogName, Me.CommandLine.Username, Me.CommandLine.Password, 1000).ToList
            Dim posts = (From p In allPosts Where p.categories.Contains("Uncategorized")).ToList
            Console.WriteLine(" Adding {0} posts", posts.Count)
            Dim processAll As DateTime = Now
            Dim countP As Integer = 0
            For Each p In posts
                countP = countP + 1
                Console.WriteLine("    Processing post {0} of {1} : {2} ", countP, posts.Count, p.title)
                Dim processPost As DateTime = Now
                Dim images As List(Of String) = Me.GetImageList(p)
                Console.WriteLine("     Found {0} images", images.Count)
                ' Get all images
                Dim countI As Integer = 0
                For Each i In images
                    countI = countI + 1
                    Dim processImage As DateTime = Now
                    Dim ipath As String = i
                    ipath = ipath.Replace("http://geekswithblogs.net/images/geekswithblogs_net/hinshelm/", "GWB-")
                    ipath = ipath.Replace("file:///C:/Users/martihins/AppData/Local/Temp/", "GWB-")
                    ipath = ipath.Replace("file:///C:/Documents and Settings/martihins/Application Data/Windows Live Writer/", "GWB-")
                    ipath = ipath.Replace("http://www.bdharry.members.winisp.net/BlogImages", "bdharry")
                    ipath = ipath.Replace("http://www.danielmoth.com/Blog", "danielmoth")
                    '%20Announcement%20Graphic%5B1%5D_c3082cc1-154f-4e97-a993-18d2d9d0ccbe
                    ipath = ipath.Replace("%20", " ")
                    ipath = ipath.Replace("%5B", "[")
                    ipath = ipath.Replace("%5D", "]")
                    Console.WriteLine("      Processing image {0} of {1} : {2} ", countI, images.Count, ipath)
                    Dim localpath As String = String.Format("c:\temp\download\{0}", ipath.Replace("/", "\"))
                    If Not System.IO.File.Exists(localpath) Then
                        Try
                            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(localpath))
                            Dim client = New WebClient()
                            client.DownloadFile(i, localpath)
                            Console.WriteLine("         Downloaded..")
                        Catch ex As Exception
                            Console.WriteLine("         Download Failed..")
                        End Try

                    Else
                        Console.WriteLine("         Download Skipped (DONE)..")
                    End If

                    '----------- Upload

                    If System.IO.File.Exists(localpath) Then
                        Try
                            Dim ProcessUpload As DateTime = Now

                            Dim bytes = My.Computer.FileSystem.ReadAllBytes(localpath)
                            Console.WriteLine("         Updoading...")
                            Dim urldata As UrlData = Server.newMediaObject(b.blogid, Me.CommandLine.Username, Me.CommandLine.Password, _
                                                    New FileData With { _
                                                        .name = ipath.Replace("\", "-").Replace("/", "-"), _
                                                        .type = GetMimeType(localpath), _
                                                        .bits = bytes
                                                    })

                            p.description = p.description.Replace(i, urldata.url)
                            Console.WriteLine("         Upload Complete in {0}s...", Now.Subtract(ProcessUpload).TotalSeconds)
                            If Server.editPost(p.postid, Me.CommandLine.Username, Me.CommandLine.Password, p, True) Then
                                Dim newName As String = System.IO.Path.GetDirectoryName(localpath) & ipath.Replace("\", "-").Replace("/", "-")
                                System.IO.File.Move(localpath, newName)
                                Console.WriteLine("         Fixed")
                            End If
                            Console.WriteLine("      Processing image {0} of {1} : DONE in {2}s ", countI, images.Count, Now.Subtract(processImage).TotalSeconds)
                        Catch ex As Exception
                            Console.WriteLine("      Processing image {0} of {1} : FAILED (Exception) in {2}s ", countI, images.Count, Now.Subtract(processImage).TotalSeconds)
                            Console.WriteLine(ex.ToString)
                        End Try
                    Else
                        Console.WriteLine("      Processing image {0} of {1} : FAILED (no download) in {2}s ", countI, images.Count, Now.Subtract(processImage).TotalSeconds)
                    End If

                Next

                Console.WriteLine("    Processing post {0} of {1} : DONE in {2}s ", countP, posts.Count, Now.Subtract(processPost).TotalSeconds)
            Next
            Console.WriteLine("    Processing : DONE in {2}s ", countP, posts.Count, Now.Subtract(processAll).TotalSeconds)
        Next
        Console.WriteLine("Done")
        Return 0
    End Function

    Private Function GetImageList(p As MetaWeblog.Post) As List(Of String)
        Dim images As New List(Of String)
        ' Add Attachements
        Dim HD As New HtmlDocument()
        HD.LoadHtml(p.description)
        ' get images from img
        Dim img = HD.DocumentNode.SelectNodes("//img")
        If Not images Is Nothing Then
            Try
                Dim imgs = (From x In img, a In x.Attributes _
                                 Where a.Name = "src" _
                                 And Not String.IsNullOrEmpty(a.Value) _
                                 AndAlso a.Value.StartsWith("http://geekswithblogs.net/images/") _
                                 Select a.Value)
                Console.WriteLine("  Adding {0} attachments from images", imgs.Count)
                'http://geekswithblogs.net/images/geekswithblogs_net/hinshelm/Windows-Live-Writer/In-Place-upgrade-of-TFS-2008-to-TFS-2010_A159/image_thumb_3.png
                images.AddRange(imgs)
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
                                  And Not String.IsNullOrEmpty(a.Value) _
                                 AndAlso a.Value.StartsWith("http://geekswithblogs.net/images/") _
                                 And GetMimeType(a.Value).Contains("image") _
                                 Select a.Value).ToList
                Console.WriteLine(" l Adding {0} attachments from links", aImages.Count)
                images.AddRange(aImages)
            Catch ex As Exception
                Console.WriteLine(ex.ToString)
            End Try
        End If

       
        Return images
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
