/** */
Ext.define('WEBAPP.form.CHK_NoCombo', {
    extend: 'WEBAPP.form.QueryCombo',
    alias: 'widget.chk_nocombo',
    constructor: function (config) {
        var me = this;
        config = config || {};

        Ext.apply(config, me.getDefaults());
        Ext.apply(me, config);
        me.callParent(arguments);
    },

    getDefaults: function () {
        return {
            displayField: 'CHK_NO',
            valueField: 'CHK_NO',
            requiredFields: ['CHK_PERIOD', 'CHK_TYPE_NAME', 'CHK_YM'],
            //storeAutoLoad: true,
            //queryUrl: '/api/AA0041/GetMMCodeCombo',
            tpl: new Ext.XTemplate(
                '<tpl for=".">',
                '<tpl if="VALUE==\'\'">',
                '<div class="x-boundlist-item" style="height:auto;">{CHK_NO}&nbsp;</div>',
                '<tpl else>',
                '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                '<span style="color:red">{CHK_NO}</span><br/>&nbsp;<span style="color:blue">{CHK_PERIOD}({CHK_YM})</span>&nbsp;<span style="color:blue">{CHK_TYPE_NAME}</span></div>',
                '</tpl></tpl>', {
                    formatText: function (text) {
                        return Ext.util.Format.htmlEncode(text);
                    }
                })
        };
    }
});
