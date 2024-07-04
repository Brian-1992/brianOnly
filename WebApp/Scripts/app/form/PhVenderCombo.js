/** */
Ext.define('WEBAPP.form.PhVenderCombo', {
    extend: 'WEBAPP.form.QueryCombo',
    alias: 'widget.PhVenderCombo',
    constructor: function (config) {
        var me = this;
        config = config || {};

        Ext.apply(config, me.getDefaults());
        Ext.apply(me, config);
        me.callParent(arguments);
    },

    getDefaults: function () {
        return {
            displayField: 'AGEN_NO',
            valueField: 'AGEN_NO',
            requiredFields: ['EASYNAME','AGEN_NAMEC'],
            tpl: new Ext.XTemplate(
                '<tpl for=".">',
                '<tpl if="VALUE==\'\'">',
                '<div class="x-boundlist-item" style="height:auto;">{AGEN_NO}&nbsp;</div>',
                '<tpl else>',
                '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                '<span style="color:red">{AGEN_NO}</span><br/>&nbsp;<span style="color:blue">{EASYNAME}</span></div>',
                '</tpl></tpl>', {
                    formatText: function (text) {
                        return Ext.util.Format.htmlEncode(text);
                    }
                })
        };
    }
});
