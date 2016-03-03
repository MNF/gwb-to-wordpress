
GeeksWithBlogs was my first ever blog and my first post was on 22nd June 2006\. Since then very little functionality has been added. This is not a complaint, but rather an observation that it is very hard to keep up with all of the blogging capabilities that people want. My point would be: “Why bother!”

I proposed a [Vote for GeeksWithBlogs moving to WordPress](http://geekswithblogs.uservoice.com/forums/57394-suggestions-for-the-community/suggestions/1494319-move-to-wordpress-as-a-platform?ref=title), but this was rejected. With that and a couple of other decisions that were made I needed to move my entire blog from GWB to WordPress.

* * *

Unfortunately the owners of GeeksWithBlogs are not forthcoming with data dumps and with the whole idea that you as the author own the content and not them, so I had to do this the hard way.

Herein you will find the tools that I created to migrate all of my posts from GeeksWithBlogs (without the help of a 301 redirect) to WordPress. You can chose to migrate to Self-Hosted, Hosted or whatever.

*THESE TOOLS ARE NOT PRODUCTION READY AND REQUIRE A DEVELOPER TO RUN*

I have only ever run these tools under debug and Alfa modes and I tweak each run of the tool for the things I am trying to achieve. There is no easy way to do this and I have created a process and tools to support that process that worked for me. You will need to **download the source** and make the tools work for you. I am DONE with this project as I moved [http://geekswithblogs.net/hinshelm](http://geekswithblogs.net/hinshelm) to [http://blog.hinshelwood.com](http://blog.hinshelwood.com), but if you want to contribute then let me know and i will add you.

Here is the process and tools that you will have to customise and use:

1.  **Get all posts from GeeksWithBlogs (**<span style="color: #c0504d;">ExportCommand</span>** )**

    For each Post on GeeksWithBlogs

    1.  Add it to a BlogML
    2.  Save BlogML
2.  **Post all saved posts to new WordPress site (Wordpress has its own importer)**

    For each Post in BlogML

    1.  Publish to WordPress
    2.  Save
3.  **Update all Posts to change context**

    For each Post on WordPress:

    1.  Get List of All Images (**<span style="color: #c0504d;">ExportImagesCommand</span>** )

        For each Image:

        1.  Download the image locally
        2.  Upload the image to WordPress
        3.  Change all instances of the image in your post to be the new WordPress location
        4.  Save post
    2.  Get List of all links (**<span style="color: #c0504d;">RewriteUrlCommand</span>** )

        For each Link :

        1.  If link matches a pattern then change it to the new pattern
        2.  Save post
4.  **Shorten all GeeksWithBlogs posts to avoid duplicate content penalties (**<span style="color: #c0504d;">RewritePostsCommand</span>** )**

    For each Post on GeeksWithBlogs:

    1.  Add “*Moved to: XXXX“ to the start
    2.  Shorten and add “*Moved to: XXXX“ to the end
    3.  Add “I have moved my blog to “XXX” to the end
    4.  Save post
5.  DONE

There are many other tools, but they all need customising to some degree to do exactly the things you want.

## All Available Tools

*   **CleanerTags** - Deletes all Tags and replaces with categories
*   **Export** - Exports all blog posts to BlogML
*   **ExportImages** - Exports all blog post images
*   **FindEmptyCatagorys** - Find all posts with no category and add one
*   **PortImages** - Downloads all images stored externally to your blog, uploads them and updates links
*   **RewriteCatagorys** – Loops through all posts and manipulates the categories
*   **RewritePosts** - Loops through all posts and manipulates the Posts
*   **RewriteTechnorati** - Loops through all posts and rewrites the technorati tags
*   **RewriteUrl** - Loops through all posts and rewrites a url
