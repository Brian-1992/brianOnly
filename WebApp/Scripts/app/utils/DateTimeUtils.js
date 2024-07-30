Ext.define('WEBAPP.utils.DateTimeUtils', {
    singleton: true,

    // 解析 ASP.NET MVC Controller 所使用的日期格式：\/Date(Tick)\/
    parseMsDate: function (dtString) {
        var reMsAjax = /^\/Date\((d|-|.*)\)[\/|\\]$/;
        a = reMsAjax.exec(dtString);
        if (a) {
            var b = a[1].split(/[-,.]/);
            return new Date(+b[0]);
        }
        return null;
    }
});