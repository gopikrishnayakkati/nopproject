﻿var Admin = Admin || {};

Admin.Navigation = (function () {
    var buildMap = function () {
        var map = {};

        var linkElements = $("a.nav-link");

        linkElements.each(function () {
            var parents = $(this).parentsUntil(".nav-sidebar");
            var href;
            var title;
            var parent;
            var grandParent;
            var n1;
            var n2;
            var n3;
            switch (parents.length) {
                // items in level one
                case 1:
                    {
                        href = $(parents).find("a").attr("href");
                        title = $(parents).find("a").find("p").html();
                        map[href] = { title: title, link: href, parent: null, grandParent: null };

                        break;
                    }
                    // items in level two, these items have parent but have not grand parent
                case 3:
                    {
                        href = $(parents).eq(0).find("a").attr("href");
                        title = $(parents).eq(0).find("a").find("p").html();
                        n1 = title.indexOf("<i class");
                        parent = $(parents).eq(2).find("a").find("p").html();
                        n2 = parent.indexOf("<i class");
                        parent = parent.substring(0, n2);
                        map[href] = { title: title, link: href, parent: parent, grandParent: null };

                        break;
                    }
                    // items in level three, these items have both parent and grand parent
                case 5:
                    {
                        href = $(parents).eq(0).find("a").attr("href");
                        title = $(parents).eq(0).find("a").find("p").html();
                        parent = $(parents).eq(2).find("a").find("p").html();
                        n2 = parent.indexOf("<i class");
                        parent = parent.substring(0, n2);
                        grandParent = $(parents).eq(4).find("a").find("p").html();
                        n3 = grandParent.indexOf("<i class");
                        grandParent = grandParent.substring(0, n3);
                        map[href] = { title: title, link: href, parent: parent, grandParent: grandParent };
                        break;
                    }
                default: break;
            }
        });

        return map;
    };

    var map;

    var init = function () {
        map = buildMap();
        $.ajax({
            cache: false,
            url: rootAppPath + 'Admin/Plugin/AdminNavigationPlugins',
            type: "GET",
            async: false,
            success: function (data, textStatus, jqXHR) {
              result = data;
            }
        });

        for (i = 0; i < result.length; i++) {
            map[result[i].link] = result[i];
        }
    };
    var events = {};
    return {
        enumerate: function (callback) {
            for (var url in map) {
                var node = map[url];
                callback.call(node, node);
            }
        },
        open: function (url) {
            if (events["open"]) {
                var event = $.Event("open", { url: url });
                events["open"].fire(event);
                if (event.isDefaultPrevented())
                    return;
            }
            window.location.href = url;
        },

        initOnce: function () {
            if (!map)
                init();
        },
        init: init
    };
})();
