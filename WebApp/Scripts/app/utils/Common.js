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
                f.append($('<input/>').attr('name', escapeHtml(p[i].name)).attr('value', escapeHtml(p[i].value)));
        }
        f.appendTo('body').submit().remove();
    },

    /* 將 複數 form 裡面的 wasDirty 重置 為 false */
    cleanForms(forms) {
        if (Array.isArray(forms)) {
            forms.forEach((form) => this.cleanForm(form));
        }
    },

    /* 將 form 裡面的 wasDirty 重置 為 false */
    cleanForm(form) {
        var items = form.getForm().getFields().items,
            i = 0,
            len = items.length;
        for (; i < len; i++) {
            var c = items[i];
            if (c.mixins && c.mixins.field && typeof c.mixins.field['initValue'] == 'function') {
                c.mixins.field.initValue.apply(c);
                c.wasDirty = false;
            }
        }
    }
});

var entityMap = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;',
    '`': '&#x60;',
    '=': '&#x3D;'
};

function escapeHtml(string) {
    if (string) {
        return String(string).replace(/[&<>"'`=]/g, function (s) {
            return entityMap[s];
        });
    }
    else
        return string;
}