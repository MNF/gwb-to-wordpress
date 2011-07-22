Imports MetaWebLogEdit.MetaWeblogApi
Imports HtmlAgilityPack
Imports System.Globalization

Friend Class GWBProcedures

    Public Shared Function GetCategories(ByVal categories() As String) As String
        Dim output As New System.Text.StringBuilder
        output.Append("<div style=""padding-bottom: 0px; margin: 0px; padding-left: 0px; padding-right: 0px; display: inline; float: none; padding-top: 0px"" id=""scid:0767317B-992E-4b12-91E0-4F059A8CECA8:54cb562d-cd68-471b-964b-c94173131b0e"" class=""wlWriterEditableSmartContent"">Technorati Tags: ")
        If categories Is Nothing Then
            output.AppendFormat("<a href=""http://technorati.com/tags/{0}"" rel=""tag"">{1}</a>", "None", "None")
        Else
            For Each c In categories
                output.AppendFormat("<a href=""http://technorati.com/tags/{0}"" rel=""tag"">{1}</a>&nbsp;", c.Replace(" ", "+"), c)
            Next
        End If
        output.Append("</div>")
        Return output.ToString
    End Function

    Public Shared Function InsertWlWriterTags(ByVal post As CookComputing.MetaWeblog.Post) As CookComputing.MetaWeblog.Post
        post.description = post.description & GetCategories(post.categories)
        Return post
    End Function

    Public Shared Function FindPostsWithNoTechnoTags(ByVal posts As List(Of CookComputing.MetaWeblog.Post)) As List(Of CookComputing.MetaWeblog.Post)
        Dim noTags As New List(Of CookComputing.MetaWeblog.Post)
        For Each p In posts
            If GWBProcedures.FindWlWriterTags(p).Count = 0 Then
                noTags.Add(p)
            End If
        Next
        Return noTags
    End Function

    Public Shared Function FindPostsWithNoCategories(ByVal posts As List(Of CookComputing.MetaWeblog.Post)) As List(Of CookComputing.MetaWeblog.Post)
        Dim noCats As New List(Of CookComputing.MetaWeblog.Post)
        For Each p In posts
            If p.categories Is Nothing OrElse p.categories.Count = 0 Then
                noCats.Add(p)
            End If
        Next
        Return noCats
    End Function

    Public Shared Function FindWlWriterTags(ByVal post As CookComputing.MetaWeblog.Post) As List(Of String)
        Dim found As New List(Of String)
        Dim classtags As New List(Of String)
        classtags.Add("wlWriterSmartContent")
        classtags.Add("wlWriterEditableSmartContent")
        'Find techno tags
        Dim desc As New System.IO.StringReader(post.description)
        Dim doc As New HtmlDocument

        doc.Load(desc)
        For Each classtag In classtags
            Dim results As HtmlNodeCollection = doc.DocumentNode.SelectNodes(String.Format("//div[@class='{0}']", classtag))
            If Not results Is Nothing Then
                For Each writerBit As HtmlNode In results
                    found.Add(writerBit.OuterHtml)
                Next
            End If
        Next


        Return found
    End Function

    Public Shared Function StripWlWriterTags(ByVal post As CookComputing.MetaWeblog.Post) As CookComputing.MetaWeblog.Post
        Dim classtags As New List(Of String)
        classtags.Add("wlWriterSmartContent")
        classtags.Add("wlWriterEditableSmartContent")
        'Find techno tags
        Dim desc As New System.IO.StringReader(post.description)
        Dim doc As New HtmlDocument
        doc.Load(desc)
        For Each classtag In classtags
            Dim results As HtmlNodeCollection = doc.DocumentNode.SelectNodes(String.Format("//div[@class='{0}']", classtag))
            If Not results Is Nothing Then
                For Each writerBit As HtmlNode In results
                    doc.DocumentNode.RemoveChild(writerBit)
                Next
            End If
        Next
        Dim writer As New System.IO.StringWriter
        doc.Save(writer)
        post.description = writer.ToString
        Return post
    End Function

    Public Shared Function StripScriptTags(ByVal post As CookComputing.MetaWeblog.Post, ByVal tag As String) As CookComputing.MetaWeblog.Post
        'Find techno tags
        Dim desc As New System.IO.StringReader(post.description)
        Dim doc As New HtmlDocument
        doc.Load(desc)
        Dim results As HtmlNodeCollection = doc.DocumentNode.SelectNodes(String.Format("//{0}", tag))
        If Not results Is Nothing Then
            For Each writerBit As HtmlNode In results
                doc.DocumentNode.RemoveChild(writerBit)
            Next
        End If
        Dim writer As New System.IO.StringWriter
        doc.Save(writer)
        post.description = writer.ToString
        Return post
    End Function

    Public Shared Function UrlReplacement(ByVal post As CookComputing.MetaWeblog.Post, ByVal oldText As String, ByVal newText As String) As CookComputing.MetaWeblog.Post
        Dim desc As New System.IO.StringReader(post.description)
        Dim doc As New HtmlDocument
        doc.Load(desc)
        Dim results As HtmlNodeCollection = doc.DocumentNode.SelectNodes(String.Format("//a"))
        If Not results Is Nothing Then
            For Each writerBit As HtmlNode In results
                writerBit.Attributes("href").Value = writerBit.Attributes("href").Value.Replace(oldText, newText)
            Next
        End If
        Dim writer As New System.IO.StringWriter
        doc.Save(writer)
        post.description = writer.ToString
        Return post
    End Function

    Public Shared Function RemovenonTechnoratiTags(ByVal post As CookComputing.MetaWeblog.Post) As CookComputing.MetaWeblog.Post
        Dim desc As New System.IO.StringReader(post.description)
        Dim doc As New HtmlDocument
        doc.Load(desc)
        Dim results As HtmlNodeCollection = doc.DocumentNode.SelectNodes(String.Format("//a[@rel='tag']"))
        If Not results Is Nothing Then
            For Each writerBit As HtmlNode In results
                If Not writerBit.Attributes("href").Value.Contains("technorati") Then
                    ' Remove the tag
                    writerBit.Attributes.Remove("rel")
                End If
            Next
        End If
        Dim writer As New System.IO.StringWriter
        doc.Save(writer)
        post.description = writer.ToString
        Return post
    End Function

    Public Shared Function getCatagory(ByVal cname As String, ByVal categories As List(Of CookComputing.MovableType.Category)) As CookComputing.MovableType.Category
        Return (From c In categories Where c.categoryName = cname).SingleOrDefault
    End Function

    Public Shared Function ComputeCategories(ByVal post As CookComputing.MetaWeblog.Post, ByVal categories As List(Of CookComputing.MovableType.Category)) As List(Of CookComputing.MovableType.Category)
        Dim lsta As New Dictionary(Of CookComputing.MovableType.Category, List(Of String))

        'Add all categories
        For Each c In categories
            lsta.Add(c, New List(Of String))
        Next
        ' Add mappings

        lsta(getCatagory("ALM", categories)).Add("Team System")

        lsta(getCatagory("TFS", categories)).Add("Team System")
        lsta(getCatagory("TFS", categories)).Add("team foundation serve")
        lsta(getCatagory("TFS", categories)).Add("tfs")
        lsta(getCatagory("TFS", categories)).Add("Team System")
        lsta(getCatagory("TFS", categories)).Add("Team System")

        lsta(getCatagory("TFS 2010", categories)).Add("TFS2010")
        lsta(getCatagory("TFS 2010", categories)).Add("TFS 2010")
        lsta(getCatagory("TFS 2010", categories)).Add("team foundation server 2010")

        lsta(getCatagory("TFS 2008", categories)).Add("TFS2008")
        lsta(getCatagory("TFS 2008", categories)).Add("TFS 2008")
        lsta(getCatagory("TFS 2008", categories)).Add("team foundation server 2008")

        lsta(getCatagory("TFS 2005", categories)).Add("TFS2005")
        lsta(getCatagory("TFS 2005", categories)).Add("TFS 2005")
        lsta(getCatagory("TFS 2005", categories)).Add("team foundation server 2005")

        lsta(getCatagory("VS 2005", categories)).Add("VS2005")
        lsta(getCatagory("VS 2005", categories)).Add("VS 2005")
        lsta(getCatagory("VS 2005", categories)).Add("Visual Studio 2005")

        lsta(getCatagory("VS 2008", categories)).Add("VS2008")
        lsta(getCatagory("VS 2008", categories)).Add("VS 2008")
        lsta(getCatagory("VS 2008", categories)).Add("Visual Studio 2008")

        lsta(getCatagory("VS 2010", categories)).Add("VS2010")
        lsta(getCatagory("VS 2010", categories)).Add("VS 2010")
        lsta(getCatagory("VS 2010", categories)).Add("Visual Studio 2010")

        lsta(getCatagory("SharePoint", categories)).Add("sharepoint")
        lsta(getCatagory("SharePoint", categories)).Add("MOSS")


        lsta(getCatagory("SP 2007", categories)).Add("Sharepoint 2007")
        lsta(getCatagory("SP 2007", categories)).Add("SP2007")
        lsta(getCatagory("SP 2007", categories)).Add("Sharepoint Services 3")

        lsta(getCatagory("SP 2010", categories)).Add("sharepoint")
        lsta(getCatagory("SP 2010", categories)).Add("Sharepoint 2010")
        lsta(getCatagory("SP 2010", categories)).Add("SP2010")
        lsta(getCatagory("SP 2010", categories)).Add("Sharepoint Services 4")
        lsta(getCatagory("SP 2010", categories)).Add("Sharepoint Foundation")

        lsta(getCatagory("SSW", categories)).Add("SSW")
        lsta(getCatagory("SSW", categories)).Add("Superior Software for Windows")


        lsta(getCatagory("MVVM", categories)).Add("MVVM")
        lsta(getCatagory("MVVM", categories)).Add("Model View View Model")
        lsta(getCatagory("MVVM", categories)).Add("Model View-View Model")
        lsta(getCatagory("MVVM", categories)).Add("Model-View-View-Model")
        lsta(getCatagory("MVVM", categories)).Add("View Model")

        lsta(getCatagory("WM6", categories)).Add("Windows Mobile")
        lsta(getCatagory("WP7", categories)).Add("Windows Phone")

        lsta(getCatagory("Live", categories)).Add("Skydrive")
        lsta(getCatagory("Live", categories)).Add("Live Writer")

        lsta(getCatagory("Dyslexia", categories)).Add("dyslexia")
        lsta(getCatagory("Dyslexia", categories)).Add("dyspraxia")
        lsta(getCatagory("Dyslexia", categories)).Add("dyscalcula")

        lsta(getCatagory("WCF", categories)).Add("WCF")
        lsta(getCatagory("WCF", categories)).Add("Communication Framework")

        lsta(getCatagory("WPF", categories)).Add("WPF")
        lsta(getCatagory("WPF", categories)).Add("Windows Presentation Framework")

        lsta(getCatagory("MOSS", categories)).Add("MOSS")
        lsta(getCatagory("MOSS", categories)).Add("office sharepoint server")

        lsta(getCatagory("TFBS", categories)).Add("TFBS")
        lsta(getCatagory("TFBS", categories)).Add("foundation build")
        lsta(getCatagory("TFBS", categories)).Add("team build")
        lsta(getCatagory("TFBS", categories)).Add("MSBuild")

        lsta(getCatagory("Silverlight", categories)).Add("Silverlight")

        lsta(getCatagory("Scrum", categories)).Add("Scrum")

        lsta(getCatagory("Branching", categories)).Add("Branching")

        Dim newCats As New List(Of CookComputing.MovableType.Category)
        'Add original cats
        For Each c In post.categories
            newCats.Add(getCatagory(c, categories))
        Next
        ' Add new Cats
        For Each cat In lsta.Keys
            If Not newCats.Contains(cat) Then
                For Each searchTrm In lsta(cat)
                    If post.description.Contains(searchTrm) Then
                        If Not newCats.Contains(cat) Then
                            newCats.Add(cat)
                        End If
                    End If
                Next
            End If
        Next


        Return newCats
    End Function

    Public Shared Function CatsToString(ByVal cats As List(Of CookComputing.MovableType.Category)) As String
        Dim s As New System.Text.StringBuilder
        For Each c In cats
            s.AppendFormat("{0}, ", c.categoryName)
        Next
        Return s.ToString
    End Function

    Public Shared Function CatsToString(ByVal cats() As CookComputing.MovableType.Category) As String
        Dim s As New System.Text.StringBuilder
        For Each c In cats
            s.AppendFormat("{0}, ", c.categoryName)
        Next
        Return s.ToString
    End Function

    Public Shared Function CatsToString(ByVal cats() As String) As String
        Dim s As New System.Text.StringBuilder
        For Each c In cats
            s.AppendFormat("{0}, ", c)
        Next
        Return s.ToString
    End Function


End Class
