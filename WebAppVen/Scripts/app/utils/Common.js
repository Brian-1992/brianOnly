Ext.define('WEBAPP.utils.Common', {
    singleton: true,

    getUrlParam: function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    },

    cloneStore: function (source) {
        source = Ext.isString(source) ? Ext.data.StoreManager.lookup(source) : source;

        var target = Ext.create(source.$className, {
            model: source.model
        });

        target.add(Ext.Array.map(source.getRange(), function (record) {
            return record.copy();
        }));

        return target;
    },

    /* postForm:送出表單
    用法: PostForm(url, paramsArr);
    人員: 江吉威 2018/12/3           */
    postForm: function (url, p) {
        var f = $('<form/>').attr('action', url).attr('method', 'POST');
        if (p) {
            for (i = 0; i < p.length; i++)
                f.append($('<input/>').attr('name', p[i].name).attr('value', p[i].value));
        }
        f.appendTo('body').submit().remove();
    }
});
